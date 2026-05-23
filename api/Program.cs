
using System.ComponentModel.DataAnnotations;
using BonchCalendar;
using BonchCalendar.Health;
using BonchCalendar.Services;
using BonchCalendar.Utils;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default)
);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddProblemDetails(configure =>
{
	configure.CustomizeProblemDetails = context =>
	{
		context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
		context.ProblemDetails.Extensions["naas_reason"] = new NaasReasons().GetReason();
	};
});

builder.Services
	.AddSingleton<IssueTrackingService>()
	.AddScoped<TimetableService>()
	.AddScoped<ApiService>()
	.AddScoped<ParsingService>();

builder.Services.AddHealthChecks()
	.AddCheck<ApiHealthCheck>("timetable_website");

builder.Services.AddCors(options =>
	options.AddDefaultPolicy(policy =>
		policy
			.WithMethods(["GET"])
			.AllowAnyOrigin()
			.AllowAnyHeader()
	)
);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();
app.UseStatusCodePages();
app.MapOpenApi();

app.MapHealthChecks("/health", new HealthCheckOptions
{
	ResponseWriter = HealthCheckWriter.WriteHealthCheckResponse
});

ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
IssueTrackingService tracker = app.Services.GetRequiredService<IssueTrackingService>();
List<string> ids = [];

app.MapGet("/stats", () => Results.Ok(new StatsResponse(ids.Count)))
	.WithName("GetStats")
	.WithDescription("Get basic usage statistics.")
	.Produces<StatsResponse>(StatusCodes.Status200OK);

app.MapGet("/faculties", async ([FromServices] ApiService apiService) =>
{
	try
	{
		Dictionary<int, string> faculties = await apiService.GetFacultiesListAsync();
		logger.LogInformation("Fetched {Count} faculties.", faculties.Count);
		tracker.TrackFacultyFetch(true);
		return Results.Ok(faculties);
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Failed to fetch faculties list.");
		tracker.TrackFacultyFetch(false);
		return Results.Problem("Failed to fetch faculties list.", statusCode: StatusCodes.Status500InternalServerError);
	}
})
	.WithName("GetFaculties")
	.WithDescription("Gets the list of faculties.")
	.ProducesProblem(StatusCodes.Status500InternalServerError)
	.Produces<Dictionary<int, string>>(StatusCodes.Status200OK);

app.MapGet("/groups", async ([FromServices] ApiService apiService, int facultyId, [Range(0, 5)] int year) =>
{
	try
	{
		Dictionary<int, string> groups = await apiService.GetGroupsListAsync(facultyId, year);
		logger.LogInformation("Fetched {Count} groups (facultyId: {FacultyId}, year: {Year}).", groups.Count, facultyId, year);
		tracker.TrackGroupFetch(facultyId, year, true);
		return Results.Ok(groups);
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Failed to fetch groups list (facultyId: {FacultyId}, year: {Year}).", facultyId, year);
		tracker.TrackGroupFetch(facultyId, year, false);
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

app.MapGet("/timetable/{facultyId}/{groupId}", async (
	int facultyId, int groupId, string? id,
	[FromServices] TimetableService timetableService
) =>
{
	string cacheFile = Path.Combine(Path.GetTempPath(), $"bonch_cal_{groupId}.ics");
	string? content = await timetableService.TryServingFromCacheAsync(groupId);
	bool hasId = !string.IsNullOrEmpty(id);

	if (hasId && id is not "download" && !ids.Contains(id!))
		ids.Add(id!);

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
		tracker.TrackTimetableFetch(facultyId, groupId, true);
		return Results.Text(content, contentType: "text/calendar");
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Failed to generate timetable for {FacultyId}/{GroupId}.", facultyId, groupId);
		tracker.TrackTimetableFetch(facultyId, groupId, false);

		if (content is not null)
		{
			logger.LogWarning("[Fallback] Serving timetable from cache ({CacheFile}).", cacheFile);
			return Results.Text(content, contentType: "text/calendar");
		}

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

app.Run();
