using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;

namespace BonchCalendar.Services;

/// <summary>
/// Service for retrieving timetable.
/// </summary>
public class TimetableService(
	ApiService apiService,
	ParsingService parsingService,
	ILogger<TimetableService> logger,
	IHostEnvironment environment
)
{
	/// <summary>
	/// Try to retrieve timetable from application's cache.
	/// </summary>
	/// <param name="groupId">ID of a group to retrieve timetable for.</param>
	/// <returns><c>null</c> if cache for this timetable is not present, or is older than 6 hours. Otherwise, timetable content in iCal format.</returns>
	public async Task<string?> TryGetTimetableFromCacheAsync(int groupId)
	{
		string cacheFile = GetCachePath(groupId);

		if (!File.Exists(cacheFile) || (DateTime.UtcNow - File.GetLastWriteTimeUtc(cacheFile)).TotalHours >= 6)
			return null;

		if (environment.IsDevelopment())
		{
			logger.LogWarning("Caching is disabled for development environment.");
			return null;
		}

		logger.LogInformation("Calendar for group {GroupId} is present in cache ({CacheFile}).", groupId, cacheFile);

		return await File.ReadAllTextAsync(cacheFile);
	}

	/// <summary>
	/// Retrieve timetable for specified group from sut.ru API.
	/// </summary>
	/// <param name="saveToCache">If set to <c>true</c>, result timetable will be wirtten to cache for that group.</param>
	/// <param name="transform">Action delegate that can be used to manipulate the result <see cref="Calendar"/> object, before converting it to iCal.</param>
	/// <returns>A string that contains timetable in iCal format.</returns>
	public async Task<string> GetTimetableAsync(int facultyId, int groupId, bool saveToCache = true, Action<Calendar>? transform = null)
	{
		// We need semester start date, since the regular timetable is represented on sut.ru semester week numbers.
		DateTime semesterStartDate = await apiService.GetSemesterStartDateAsync(groupId);
		string groupName = (await apiService.GetGroupsListAsync(facultyId, 0))[groupId];

		// Retrieve and parse regular timetable first, since it's the only timetable that has different structure.
		string classesRaw = await apiService.GetScheduleDocumentAsync(groupId, TimetableType.Classes);
		List<CalendarEvent> timetable = [.. parsingService.ParseGeneralTimetable(classesRaw, semesterStartDate, groupName)];

		// Retrieve and parse other timetables using a loop, since they all can be parsed using the same parser.
		TimetableType[] types = [TimetableType.Attestations, TimetableType.Exams, TimetableType.ExamsForExtramural];
		foreach (TimetableType type in types)
		{
			classesRaw = await apiService.GetScheduleDocumentAsync(groupId, type);
			timetable.AddRange(parsingService.ParseExamTimetable(classesRaw, groupName));
		}

		// Create and configure the calendar.
		Calendar calendar = new();
		calendar.AddTimeZone(new VTimeZone("Europe/Moscow"));
		calendar.Properties.Add(new CalendarProperty("X-WR-CALNAME", groupName));
		calendar.Properties.Add(new CalendarProperty("X-WR-TIMEZONE", "Europe/Moscow"));
		calendar.Properties.Add(new CalendarProperty("REFRESH-INTERVAL;VALUE=DURATION", "PT6H"));	// Specifies how often calendar client should poll for new timetable.
		calendar.Events.AddRange(timetable);

		transform?.Invoke(calendar);		// If transform delegate is not null, invoke it.

		// Serialize calendar to iCal format.
		string content = new CalendarSerializer().SerializeToString(calendar)!;

		if (saveToCache)
		{
			string cacheFile = GetCachePath(groupId);
			await File.WriteAllTextAsync(cacheFile, content);
			logger.LogInformation("Cache updated: {CacheFile}", cacheFile);
		}

		return content;
	}

	private static string GetCachePath(int groupId) =>
		Path.Combine(Path.GetTempPath(), $"bonch_cal_{groupId}.ics");
}
