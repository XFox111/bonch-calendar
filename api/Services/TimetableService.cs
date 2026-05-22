using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;

namespace BonchCalendar.Services;

public class TimetableService(
	ApiService apiService,
	ParsingService parsingService,
	ILogger<TimetableService> logger,
	IHostEnvironment environment
)
{
	public async Task<string?> TryServingFromCacheAsync(int groupId)
	{
		string cacheFile = GetCachePath(groupId);

		if (!File.Exists(cacheFile) || (DateTime.UtcNow - File.GetLastWriteTimeUtc(cacheFile)).TotalHours >= 6)
			return null;

		if (environment.IsDevelopment())
		{
			logger.LogWarning("Caching is disabled for development environment.");
			return null;
		}

		return await File.ReadAllTextAsync(cacheFile);
	}

	public async Task<string> GetTimetableAsync(int facultyId, int groupId, bool saveToCache = true, Action<Calendar>? transform = null)
	{
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
		calendar.AddTimeZone(new VTimeZone("Europe/Moscow"));
		calendar.Properties.Add(new CalendarProperty("X-WR-CALNAME", groupName));
		calendar.Properties.Add(new CalendarProperty("X-WR-TIMEZONE", "Europe/Moscow"));
		calendar.Properties.Add(new CalendarProperty("REFRESH-INTERVAL;VALUE=DURATION", "PT6H"));
		calendar.Events.AddRange(timetable);
		transform?.Invoke(calendar);
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
