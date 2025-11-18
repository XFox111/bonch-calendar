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
				ParserUtils.GetTimesFromLabel(timeLabelText) :
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

				classes.Add(GetEvent(
					$"{number}. {className} ({classType})", auditorium,
					GetDescription(groupName, professors, auditorium, weeks),
					classDate, startTime, endTime));
			}
		}

		return [.. classes];
	}

	public CalendarEvent[] ParseExamTimetable(string rawHtml, string groupName)
	{
		using IHtmlDocument doc = new HtmlParser().ParseDocument(rawHtml);

		IHtmlCollection<IElement> rawClasses = doc.QuerySelectorAll(".pair");
		List<CalendarEvent> classes = new(rawClasses.Count);

		foreach (IElement classItem in rawClasses)
		{
			var (className, classType, professors, auditorium) = ParseBaseInfo(classItem);

			DateTime classDate = DateTime.Parse(classItem.Children[0].ChildNodes[0].TextContent, CultureInfo.GetCultureInfo("ru-RU"));
			Match timeMatch = ParserUtils.ExamTimeRegex().Match(classItem.GetAttribute("pair")!);

			if (!timeMatch.Success)
				timeMatch = ParserUtils.ExamTimeAltRegex().Match(classItem.GetAttribute("pair")!);

			string number = timeMatch.Groups["number"].Success ?
				$"{timeMatch.Groups["number"].Value}. " : string.Empty;

			TimeSpan startTime = TimeSpan.Parse(timeMatch.Groups["start"].Value.Replace('.', ':'));
			TimeSpan endTime = TimeSpan.Parse(timeMatch.Groups["end"].Value.Replace('.', ':'));

			classes.Add(GetEvent(
				$"{number}{className} ({classType})", auditorium,
				GetDescription(groupName, professors, auditorium),
				classDate, startTime, endTime));
		}

		return [.. classes];
	}

	private static CalendarEvent GetEvent(string title, string auditorium, string description, DateTime date, TimeSpan startTime, TimeSpan endTime) =>
		new()
		{
			Summary = title,
			Description = description,
			Start = new CalDateTime(date.Add(startTime - TimeSpan.FromHours(3)).ToUniversalTime()),
			End = new CalDateTime(date.Add(endTime - TimeSpan.FromHours(3)).ToUniversalTime()),
			Location = auditorium
		};

	private static string GetDescription(string groupName, string[] professors, string auditorium, int[]? weeks = null)
	{
		string str = $"""
		Группа: {groupName}
		Преподаватель(и):
		- {string.Join("\n- ", professors)}
		""";

		if (weeks is not null && weeks.Length > 0)
			str += $"\nНедели: {string.Join(", ", weeks)}";

		Match auditoriumMatch = ParserUtils.AuditoriumRegex().Match(auditorium);

		if (!auditoriumMatch.Success)
			auditoriumMatch = ParserUtils.AuditoriumAltRegex().Match(auditorium);

		if (auditoriumMatch.Success)
			str += "\n\n" + $"""
			ГУТ.Навигатор:
			https://nav.sut.ru/?cab=k{auditoriumMatch.Groups["wing"].Value}-{auditoriumMatch.Groups["room"].Value}
			""";

		return str;
	}

	private static (string className, string classType, string[] professors, string auditorium) ParseBaseInfo(IElement classElement)
	{
		string className = classElement.QuerySelector(".subect")!.TextContent;
		string classType = classElement.QuerySelector(".type")!.TextContent
			.Replace("(", string.Empty).Replace(")", string.Empty).Trim();

		string[] professors = classElement.QuerySelector(".teacher[title]")!.GetAttribute("title")
				!.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		string auditorium = classElement.QuerySelector(".aud")!.TextContent
			.Replace("ауд.:", string.Empty).Replace(';', ',').Trim();

		return (className, classType, professors, auditorium);
	}
}
