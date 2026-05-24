using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BonchCalendar.Utils;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace BonchCalendar.Services;

public partial class ParsingService
{
	/// <summary>
	/// Parse general timetable document.
	/// </summary>
	/// <param name="rawHtml">HTML document retrieved from the API.</param>
	/// <param name="semesterStartDate"><see cref="DateTime"/> that represents the first day of current semester.</param>
	/// <param name="groupName">Name of a group this timetable is for.</param>
	/// <returns>An array of <see cref="CalendarEvent"/>s</returns>
	public CalendarEvent[] ParseGeneralTimetable(string rawHtml, DateTime semesterStartDate, string groupName)
	{
		using IHtmlDocument doc = new HtmlParser().ParseDocument(rawHtml);

		IHtmlCollection<IElement> rawClasses = doc.QuerySelectorAll(".pair");
		List<CalendarEvent> classes = [];

		foreach (IElement classItem in rawClasses)
		{
			var (className, classType, professors, auditorium) = ParseBaseInfo(classItem);
			int weekday = int.Parse(classItem.GetAttribute("weekday")!);

			string timeLabelText = classItem.ParentElement!.ParentElement!.Children[0].TextContent;
			Match timeMatch = ParserUtils.TimeLabelRegex().Match(timeLabelText);
			string number = timeMatch.Success ? timeMatch.Groups["number"].Value : timeLabelText;
			(TimeSpan startTime, TimeSpan endTime) = !timeMatch.Success ?
				ParserUtils.GetTimesFromLabel(timeLabelText) : // If the label for some reason doesn't contain start and end time, we can infer it from class' number
				(
					TimeSpan.Parse(timeMatch.Groups["start"].Value),
					TimeSpan.Parse(timeMatch.Groups["end"].Value)
				);

			int[] weeks = [
				.. ParserUtils.NumberRegex().Matches(classItem.QuerySelector(".weeks")!.TextContent)
					.Select(i => int.Parse(i.Value))
			];

			foreach (int week in weeks)
			{
				DateTime classDate = semesterStartDate
					.AddDays((week - 1) * 7)          // Move to the correct week
					.AddDays(weekday - 1);            // Move to the correct weekday

				classes.Add(CreateEvent(
					title: $"{number}. {className} ({classType})",
					location: auditorium,
					description: CreateDescription(groupName, professors, auditorium, weeks),
					date: classDate,
					startTime,
					endTime
				));
			}
		}

		return [.. classes];
	}

	/// <summary>
	/// Parse exam timetable document.
	/// </summary>
	/// <param name="rawHtml">HTML document, retrieved from the API.</param>
	/// <param name="groupName">Name of a group this timetable is for.</param>
	/// <returns>An array of <see cref="CalendarEvent"/>s</returns>
	public CalendarEvent[] ParseExamTimetable(string rawHtml, string groupName)
	{
		using IHtmlDocument doc = new HtmlParser().ParseDocument(rawHtml);

		IHtmlCollection<IElement> rawClasses = doc.QuerySelectorAll(".pair");
		List<CalendarEvent> classes = new(rawClasses.Count);

		foreach (IElement classItem in rawClasses)
		{
			var (className, classType, professors, auditorium) = ParseBaseInfo(classItem);

			DateTime classDate = DateTime.ParseExact(classItem.Children[0].ChildNodes[0].TextContent, "dd.MM.yyyy", CultureInfo.InvariantCulture);
			Match timeMatch = ParserUtils.ExamTimeRegex().Match(classItem.GetAttribute("pair")!);

			if (!timeMatch.Success)
				timeMatch = ParserUtils.ExamTimeAltRegex().Match(classItem.GetAttribute("pair")!);

			string number = timeMatch.Groups["number"].Success ?
				$"{timeMatch.Groups["number"].Value}. " : string.Empty;

			TimeSpan startTime = TimeSpan.Parse(timeMatch.Groups["start"].Value.Replace('.', ':'));
			TimeSpan endTime = TimeSpan.Parse(timeMatch.Groups["end"].Value.Replace('.', ':'));

			classes.Add(CreateEvent(
				title: $"{number}{className} ({classType})",
				location: auditorium,
				description: CreateDescription(groupName, professors, auditorium),
				date: classDate,
				startTime,
				endTime
			));
		}

		return [.. classes];
	}

	// Create a calendar event
	private static CalendarEvent CreateEvent(string title, string location, string description, DateTime date, TimeSpan startTime, TimeSpan endTime) =>
		new()
		{
			Summary = title,
			Description = description,
			Start = new CalDateTime(date.Add(startTime - TimeSpan.FromHours(3)).ToUniversalTime()),
			End = new CalDateTime(date.Add(endTime - TimeSpan.FromHours(3)).ToUniversalTime()),
			Location = location
		};

	// Create event description
	private static string CreateDescription(string groupName, string[] professors, string auditorium, int[]? weeks = null)
	{
		string str = $"""
		Группа: {groupName}
		Преподаватель(и):
		- {string.Join("\n- ", professors)}
		""";

		if (weeks is not null && weeks.Length > 0)
			str += $"\nНедели: {string.Join(", ", weeks)}";

		// Attempt to recognize wing and room number
		Match auditoriumMatch = ParserUtils.AuditoriumRegex().Match(auditorium);

		if (!auditoriumMatch.Success)
			auditoriumMatch = ParserUtils.AuditoriumAltRegex().Match(auditorium);

		// If successful, we can add a nav.sut.ru map link
		if (auditoriumMatch.Success)
			str += "\n\n" + $"""
			ГУТ.Навигатор:
			https://nav.sut.ru/?cab=k{auditoriumMatch.Groups["wing"].Value}-{auditoriumMatch.Groups["room"].Value}
			""";

		// Some shameless self-promotion
		str += "\n\n" + "Создано при помощи сервиса Бонч.Календарь: https://bonch.xfox111.net";

		return str;
	}

	// Parse basic info for a class
	private static (string className, string classType, string[] professors, string auditorium) ParseBaseInfo(IElement classElement)
	{
		string className = classElement.QuerySelector(".subect")?.TextContent ?? string.Empty;
		string classType = classElement.QuerySelector(".type")?.TextContent
			.Replace("(", string.Empty).Replace(")", string.Empty).Trim() ?? string.Empty;

		string[] professors = classElement.QuerySelector(".teacher[title]")?.GetAttribute("title")
				?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

		string auditorium = classElement.QuerySelector(".aud")?.TextContent
			.Replace("ауд.:", string.Empty).Replace(';', ',').Trim() ?? string.Empty;

		return (className, classType, professors, auditorium);
	}
}
