
using System.ComponentModel.DataAnnotations;
using BonchCalendar;
using BonchCalendar.Health;
using BonchCalendar.Services;
using BonchCalendar.Utils;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

// Adding static JSON serializer since we're running in Native AOT
builder.Services.ConfigureHttpJsonOptions(options =>
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default)
);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();      // OpenAPI specification generator
builder.Services.AddValidation();   // Request validation

// Customizing non-200 responses to include trace identifier and a no-as-a-service reason
builder.Services.AddProblemDetails(configure =>
{
	configure.CustomizeProblemDetails = context =>
	{
		context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
		context.ProblemDetails.Extensions["naas_reason"] = new NaasReasons().GetReason();
	};
});

builder.Services
	.AddSingleton<IssueTrackingService>()       // Service for tracking latest API request results
	.AddScoped<TimetableService>()              // Service for generating a timetable
	.AddScoped<ApiService>()                    // Service for making API calls to sut.ru API
	.AddScoped<ParsingService>();               // Service for parsing timetable from sut.ru

builder.Services.AddHealthChecks()
	.AddCheck<ApiHealthCheck>("timetable_website");     // Healthcheck service

// Get ORIGIN_DOMAIN environmental variable
string? corsDomain = Environment.GetEnvironmentVariable("ORIGIN_DOMAIN");

// Configure defautl CORS policy
builder.Services.AddCors(options =>
	options.AddDefaultPolicy(policy =>
	{
		// Allow only GET requests with any headers
		policy
			.WithMethods(["GET"])
			.AllowAnyHeader();

		// If ORIGIN_DOMAIN environmental variable is set, allow request only from this domain,
		// otherwise allow request from any domains.
		if (string.IsNullOrWhiteSpace(corsDomain))
			policy.AllowAnyOrigin();
		else
			policy.WithOrigins(corsDomain);
	})
);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors();                  // Enable CORS
app.UseStatusCodePages();       // Enable default JSON response body for non-200 responses.
app.MapOpenApi();               // Map OpenAPI sepcification. Available at /openapi/v1.json

// Map healthcheck endpoint with custom response writer
// Remark: /health and /openapi/v1.json endpoints are not present in OpenAPI specification.
app.MapHealthChecks("/health", new HealthCheckOptions
{
	ResponseWriter = HealthCheckWriter.WriteHealthCheckResponse
});

// Request singleton services which will be used in subsequent endpoints
ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
IssueTrackingService tracker = app.Services.GetRequiredService<IssueTrackingService>();

// List of identifiers for tracking unique visits to /timetable endpoint (essentially, for unique user counting).
List<string> ids = [];

// Statistics endpoint. Shows number of active users.
app.MapGet("/stats", () => Results.Ok(new StatsResponse(ids.Count)))
	.WithName("GetStats")
	.WithDescription("Get basic usage statistics.")
	.Produces<StatsResponse>(StatusCodes.Status200OK);

// Retrieve list of faculties and their IDs from sut.ru API
app.MapGet("/faculties", async ([FromServices] ApiService apiService) =>
{
	try
	{
		Dictionary<int, string> faculties = await apiService.GetFacultiesListAsync();
		logger.LogInformation("Fetched {Count} faculties.", faculties.Count);
		tracker.TrackFacultyFetch(true);        // Record that last attempt to retrieve faculties was successful
		return Results.Ok(faculties);
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Failed to fetch faculties list.");
		tracker.TrackFacultyFetch(false);       // Record that last attempt to retrieve faculties was unsuccessful
		return Results.Problem("Failed to fetch faculties list.", statusCode: StatusCodes.Status500InternalServerError);
	}
})
	.WithName("GetFaculties")
	.WithDescription("Gets the list of faculties.")
	.ProducesProblem(StatusCodes.Status500InternalServerError)
	.Produces<Dictionary<int, string>>(StatusCodes.Status200OK);

// Retrieve list of groups for a chosen faculty and year.
// Year can be in range from 1 to 5.
// If year is not specified, all groups for chosen faculty will be retrieved.
app.MapGet("/groups", async ([FromServices] ApiService apiService, int facultyId, [Range(1, 5)] int? year) =>
{
	year ??= 0;     // Setting year to 0 (show all groups) if not specified in the request.

	try
	{
		Dictionary<int, string> groups = await apiService.GetGroupsListAsync(facultyId, year.Value);
		logger.LogInformation("Fetched {Count} groups (facultyId: {FacultyId}, year: {Year}).", groups.Count, facultyId, year);
		tracker.TrackGroupFetch(facultyId, year.Value, true);   // Track whether retrieving groups for this specific faculty and year was successul or not.
		return Results.Ok(groups);
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Failed to fetch groups list (facultyId: {FacultyId}, year: {Year}).", facultyId, year);
		tracker.TrackGroupFetch(facultyId, year.Value, false);  // Track whether retrieving groups for this specific faculty and year was successul or not.
		return Results.Problem(
			"Failed to fetch groups list.",
			statusCode: StatusCodes.Status500InternalServerError,
			extensions: new Dictionary<string, object?>
			{
				["facultyId"] = facultyId,
				["year"] = year
			}
		);
	}
})
	.WithName("GetGroups")
	.WithDescription("Gets the list of groups for the specified faculty and course.")
	.Produces<Dictionary<int, string>>(StatusCodes.Status200OK)
	.ProducesProblem(StatusCodes.Status500InternalServerError)
	.ProducesValidationProblem();

// Retrieve timetable for specified group in form of iCal.
app.MapGet("/timetable/{facultyId}/{groupId}", async (
	int facultyId, int groupId, string? id,
	[FromServices] TimetableService timetableService
) =>
{
	string? content = await timetableService.TryGetTimetableFromCacheAsync(groupId);
	bool hasId = !string.IsNullOrEmpty(id);

	// If this is the first request with given id, we record it.
	// "download" is a special "ID" that is used solely for downloading timetable as file.
	if (hasId && id is not "download" && !ids.Contains(id!))
		ids.Add(id!);

	// If we have a valid cache, we serve it, instead of retrieving new timetable from sut.ru API
	// We don't serve cache for requests with no ID, since they have a special behavior.
	if (content is not null && hasId)
	{
		logger.LogInformation("Serving timetable for {FacultyId}/{GroupId} from cache.", facultyId, groupId);
		return Results.Text(content, contentType: "text/calendar");
	}

	try
	{
		logger.LogInformation("Begin generating timetable for {FacultyId}/{GroupId}.", facultyId, groupId);

		if (hasId)
			content = await timetableService.GetTimetableAsync(facultyId, groupId, saveToCache: true);
		else
		{
			// For requests with no ID we append an event at 7pm that asks users to update their calendar URL,
			// since now all /timetable requests must have one.
			// This is a temporary behavior that will be changed to just sending 4xx response instead later.
			content = await timetableService.GetTimetableAsync(facultyId, groupId, saveToCache: false, transform: calendar =>
				calendar.Events.Add(new()
				{
					Summary = "Важно: обновите календарь расписания",
					Description = """
					Ваша ссылка на календарь устарела. Пожалуйста, обновите ее чтобы продолжить пользоватся сервисом.

					Новая ссылка позволит нам собирать статистику о количестве активных пользователей. Важно: мы НЕ собираем какие-либо персональные данные! Новая ссылка лишь позволит нам узнать точное количесво пользователей, что очень важно для продолжения работы сервиса.

					Для того чтобы обновить ссылку:
					1. Перейдите на сайт https://bonch.xfox111.net/
					2. Повторите все действия что и при создании календаря
					3. Удалите старый календарь

					Просим прощения за доставленные неудобства.

					Если возникнут вопросы – обращайтесь на почту feedback@xfox111.net

					Это событие будет появляться каждый день в 19:00 до тех пор, пока ссылка не будет обновлена.
					""",
					Location = "https://bonch.xfox111.net",
					Start = new CalDateTime((DateTime.Today + TimeSpan.FromHours(16)).ToUniversalTime()),
					End = new CalDateTime((DateTime.Today + TimeSpan.FromHours(16) + TimeSpan.FromMinutes(15)).ToUniversalTime()),
				})
			);
			logger.LogInformation("Deprecation notice appended to calendar {FacultyId}/{GroupId}.", facultyId, groupId);
		}

		logger.LogInformation("Fetched timetable for {FacultyId}/{GroupId}.", facultyId, groupId);
		tracker.TrackTimetableFetch(facultyId, groupId, true);      // Track whether retrieving timetable for this specific group was successul or not.
		return Results.Text(content, contentType: "text/calendar");
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Failed to generate timetable for {FacultyId}/{GroupId}.", facultyId, groupId);
		tracker.TrackTimetableFetch(facultyId, groupId, false);     // Track whether retrieving timetable for this specific group was successul or not.

		return Results.Problem(
			"Failed to fetch timetable",
			statusCode: StatusCodes.Status500InternalServerError,
			extensions: new Dictionary<string, object?>
			{
				["facultyId"] = facultyId,
				["groupId"] = groupId
			}
		);
	}
})
	.WithName("GetTimetable")
	.WithDescription("Gets the iCal timetable for the specified group.")
	.Produces<string>(StatusCodes.Status200OK, "text/calendar")
	.ProducesProblem(StatusCodes.Status500InternalServerError)
	.ProducesValidationProblem();

// Start the application.
app.Run();
