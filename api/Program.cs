
using System.ComponentModel.DataAnnotations;
using BonchCalendar;
using BonchCalendar.Health;
using BonchCalendar.Services;
using BonchCalendar.Utils;
using HealthChecks.UI.Client;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
	ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapGet("/faculties", async ([FromServices] ApiService apiService) =>
{
	logger.LogInformation("Fetching faculties list.");
	Dictionary<int, string> faculties = await apiService.GetFacultiesListAsync();
	return Results.Ok(faculties);
})
	.WithName("GetFaculties")
	.WithDescription("Gets the list of faculties.")
	.Produces<Dictionary<int, string>>(StatusCodes.Status200OK);

app.MapGet("/groups", async ([FromServices] ApiService apiService, int facultyId, [Range(1, 5)] int course) =>
{
	logger.LogInformation("Fetching groups list for faculty {FacultyId} and course {Course}.", facultyId, course);
	Dictionary<int, string> groups = await apiService.GetGroupsListAsync(facultyId, course);
	return Results.Ok(groups);
})
	.WithName("GetGroups")
	.WithDescription("Gets the list of groups for the specified faculty and course.")
	.Produces<Dictionary<int, string>>(StatusCodes.Status200OK)
	.ProducesValidationProblem();

app.MapGet("/timetable/{facultyId}/{groupId}", async (
	int facultyId, int groupId,
	[FromServices] ApiService apiService,
	[FromServices] ParsingService parsingService
) =>
{
	logger.LogInformation("Generating timetable for group {GroupId} of faculty {FacultyId}.", groupId, facultyId);
	string cacheFile = Path.Combine(Path.GetTempPath(), $"bonch_cal_{groupId}.ics");

	if (File.Exists(cacheFile) && (DateTime.UtcNow - File.GetLastWriteTimeUtc(cacheFile)).TotalHours < 6)
	{
		if (args.Contains("--no-cache"))
			logger.LogWarning("Cache disabled via --no-cache, regenerating timetable for group {GroupId}.", groupId);
		else
		{
			logger.LogInformation("Serving timetable for group {GroupId} from cache.", groupId);
			return Results.Text(await File.ReadAllTextAsync(cacheFile), contentType: "text/calendar");
		}
	}

	DateTime semesterStartDate = await apiService.GetSemesterStartDateAsync(groupId);
	string groupName = (await apiService.GetGroupsListAsync(facultyId, 0))[groupId];

	string classesRaw = await apiService.GetScheduleDocumentAsync(groupId, TimetableType.Classes);
	List<CalendarEvent> timetable = [.. parsingService.ParseGeneralTimetable(classesRaw, semesterStartDate, groupName)];

	TimetableType[] types = [TimetableType.Attestations, TimetableType.Exams, TimetableType.ExamsForExtramural];
	foreach (TimetableType type in types)
	{
		classesRaw = await apiService.GetScheduleDocumentAsync(groupId, type);
		timetable.AddRange(parsingService.ParseExamTimetable(classesRaw, groupName));
	}

	Calendar calendar = new();
	calendar.Properties.Add(new CalendarProperty("X-WR-CALNAME", groupName));
	calendar.Properties.Add(new CalendarProperty("X-WR-TIMEZONE", "Europe/Moscow"));
	calendar.Properties.Add(new CalendarProperty("REFRESH-INTERVAL;VALUE=DURATION", "PT6H"));
	calendar.Events.AddRange(timetable);
	calendar.AddTimeZone(new VTimeZone("Europe/Moscow"));
	string serialized = new CalendarSerializer().SerializeToString(calendar)!;

	await File.WriteAllTextAsync(cacheFile, serialized);
	logger.LogInformation("Cached timetable for group {GroupId} to {CacheFile}.", groupId, cacheFile);
	return Results.Text(serialized, contentType: "text/calendar");
})
	.WithName("GetTimetable")
	.WithDescription("Gets the iCal timetable for the specified group.")
	.Produces<string>(StatusCodes.Status200OK, "text/calendar")
	.ProducesValidationProblem();

app.Run();
