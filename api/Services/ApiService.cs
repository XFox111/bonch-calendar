using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BonchCalendar.Utils;

namespace BonchCalendar.Services;

// For better understanding of what is happening here,
// I recommend visiting https://cabinet.sut.ru/raspisanie_all_new.php
// and trying to send requests yourself.

/// <summary>
/// Service for calling sut.ru API.
/// </summary>
public class ApiService
{
	/// <summary>
	/// Retrieve list of faculties and their IDs.
	/// </summary>
	/// <returns>A dictionary, where the key is faculty's ID and the value is faculty's name.</returns>
	public async Task<Dictionary<int, string>> GetFacultiesListAsync() =>
		ParseListResponse(await SendRequestAsync(new()
		{
			["choice"] = "1",       // "choice" is always "1" (idk why, don't ask me)
			["schet"] = GetCurrentSemesterId()
		}));

	/// <summary>
	/// Retrieve list of groups for specified faculty and year.
	/// </summary>
	/// <param name="facultyId">ID of selected faculty.</param>
	/// <param name="year">An academic year. Should be from 0 to 5.</param>
	/// <returns>A dictionary, where the key is group's ID and the value is group's name.</returns>
	/// <remarks>
	/// If <paramref name="year"/> is set to 0, all groups for the specified faculty will be retrieved instead.
	/// </remarks>
	public async Task<Dictionary<int, string>> GetGroupsListAsync(int facultyId, int year) =>
		ParseListResponse(await SendRequestAsync(new()
		{
			["choice"] = "1",
			["schet"] = GetCurrentSemesterId(),
			["faculty"] = facultyId.ToString(), // Specifying faculty ID returns a list of groups
			["kurs"] = year.ToString()        // Course number is actually optional, but filters out other groups. Can be set 0 to get all groups of the faculty.
		}));

	/// <summary>
	/// Retrieve timetable document for the specified group.
	/// </summary>
	/// <param name="groupId">ID of selected group.</param>
	/// <param name="timetableType">Type of a timetable to retrieve.</param>
	/// <returns>A string, represeting raw HTML document, that contains the timetable.</returns>
	public async Task<string> GetScheduleDocumentAsync(int groupId, TimetableType timetableType) =>
		await SendRequestAsync(new()
		{
			["schet"] = GetCurrentSemesterId(),
			["type_z"] = ((int)timetableType).ToString(),
			["group"] = groupId.ToString()
		});

	/// <summary>
	/// Retrieve current semester start date.
	/// </summary>
	/// <param name="groupId">ID of a group.</param>
	/// <returns>A <see cref="DateTime"/> object, representing the first day of current semester.</returns>
	/// <remarks>
	/// <paramref name="groupId"/> can be any valid group ID. We only need it for retrieving a correct HTML document.
	/// </remarks>
	public async Task<DateTime> GetSemesterStartDateAsync(int groupId)
	{
		using HttpClient client = new();
		// We go to this URL, since it has to contain current week number,
		// which we can use to calculate the first day of the semester.
		// If we don't specify group, we'll get a page listing all available groups,
		// which doesn't contain current week number, thus, rendering it useless for us.
		string content = await client.GetStringAsync($"https://www.sut.ru/studentu/raspisanie/raspisanie-zanyatiy-studentov-ochnoy-i-vecherney-form-obucheniya?group={groupId}");

		using IHtmlDocument doc = new HtmlParser().ParseDocument(content);

		// 1. Get <a> tag with id "rasp-prev"
		// 2. Get it's neighbor <div> tag that is the second child of their parent tag
		// 3. Get <span> tag inside the <div>
		string labelText = doc.QuerySelector("a#rasp-prev + div:nth-child(2) > span")!.TextContent;

		// Content of the <span> tag is supposed to be something like "Нечетная неделя (15)"
		// So, we can use regular expressions to get the "15" part and parse it to an integer.
		int weekNumber = int.Parse(ParserUtils.NumberRegex().Match(labelText).Value);

		DateTime currentDate = DateTime.Today;
		currentDate = currentDate
			.AddDays(-(int)currentDate.DayOfWeek + 1) // Move to Monday
			.AddDays(-7 * (weekNumber - 1));          // Move back to the first week

		return currentDate;
	}

	// Utility method that converts faculty or group list response into a dictionary.
	// It expected the reponse to be in format: "1,Group 1;2,Group2;..."
	private static Dictionary<int, string> ParseListResponse(string responseContent) =>
		responseContent
			.Split(';', StringSplitOptions.RemoveEmptyEntries)
			.Select(item => item.Split(','))
			.ToDictionary(
				parts => int.Parse(parts[0]),
				parts => parts[1]
			);

	// Utility method for sending request to sut.ru API.
	private static async Task<string> SendRequestAsync(Dictionary<string, string> formData)
	{
		HttpRequestMessage request = new(HttpMethod.Post, "https://cabinet.sut.ru/raspisanie_all_new.php")
		{
			Content = new FormUrlEncodedContent(formData)
		};

		using HttpClient client = new(new HttpClientHandler
		{
			// Sometimes Bonch being Bonch just doesn't renew its SSL certificates properly,
			// so we just assume that we're in the right place.
			ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
		});

		HttpResponseMessage response = await client.SendAsync(request);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}

	private static string GetCurrentSemesterId()
	{
		DateTime now = DateTime.Today;
		int currentSemester = now.Month is >= 8 or < 2
			? 1         // August through January - first semester
			: 2;        // Everything else - second

		int academicYearStartYear = now.Year - 2000;    // We need only last two digits (e.g. 25 for 2025)
		// P.S. I am not a fun of this variable name either.

		if (now.Month < 8)      // Before August means we are in the second semester of the previous academic year
			academicYearStartYear--;

		return $"205.{academicYearStartYear}{academicYearStartYear + 1}/{currentSemester}";
	}
}
