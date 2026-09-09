using System.ComponentModel.DataAnnotations;
using BonchCalendar.Services;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace BonchCalendar.Endpoints;

public static partial class Endpoints
{
	/// <summary>
	/// Maps an endpoint to retrieve timetable for a specific group from sut.ru API
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static RouteHandlerBuilder MapGetTimetable(this IEndpointRouteBuilder builder) =>
		builder.MapGet("/timetable/{facultyId}/{groupId}", async (
			int facultyId, int groupId, [StringLength(48)] string? id,
			TimetableService timetableService,
			ILogger<Program> logger,
			IssueTrackingService tracker,
			StatsService statsService
		) =>
		{
			(Calendar? calendar, string? content) = await timetableService.TryGetTimetableFromCacheAsync(groupId);
			bool calendarWasChanged = false;
			bool hasId = !string.IsNullOrEmpty(id);

			if (content is not null)
				logger.LogInformation("Serving timetable for {FacultyId}/{GroupId} from cache.", facultyId, groupId);

			// If this is the first request with given id, we record it.
			// "download" is a special "ID" that is used solely for downloading timetable as file.
			if (hasId && id is not "download")
				statsService.RecordId(id!);

			if (calendar is null || content is null)
				try
				{
					(calendar, content) = await timetableService.GetTimetableAsync(facultyId, groupId);
					tracker.TrackTimetableFetch(facultyId, groupId, true);      // Track whether retrieving timetable for this specific group was successul or not.
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed to generate timetable for {FacultyId}/{GroupId}.", facultyId, groupId);
					tracker.TrackTimetableFetch(facultyId, groupId, false);     // Track whether retrieving timetable for this specific group was successul or not.

					return Results.Problem(
						"Failed to fetch timetable",
						statusCode: StatusCodes.Status503ServiceUnavailable,
						extensions: new Dictionary<string, object?>
						{
							["facultyId"] = facultyId,
							["groupId"] = groupId
						}
					);
				}

			if (!hasId)
			{
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
					End = new CalDateTime((DateTime.Today + TimeSpan.FromHours(16) + TimeSpan.FromMinutes(15)).ToUniversalTime())
				});
				calendarWasChanged = true;
				logger.LogInformation("Deprecation notice appended to calendar {FacultyId}/{GroupId}.", facultyId, groupId);
			}

			if (calendarWasChanged)
				content = new CalendarSerializer().SerializeToString(calendar)!;

			return Results.Text(content, contentType: "text/calendar");
		})
			.WithName("GetTimetable")
			.WithDescription("Gets the iCal timetable for the specified group. If the request contains a valid ID, it will be recorded for statistics purposes. If the request does not contain an ID, a deprecation notice will be added to the calendar. ID can be any string up to 48 characters long. If the ID is 'download', it will not be recorded for statistics purposes.")
			.Produces<string>(StatusCodes.Status200OK, "text/calendar")
			.ProducesProblem(StatusCodes.Status503ServiceUnavailable)
			.ProducesValidationProblem();
}
