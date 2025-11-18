using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BonchCalendar.Utils;

namespace BonchCalendar.Services;

public class ApiService
{
	public async Task<Dictionary<int, string>> GetFacultiesListAsync() =>
		ParseListResponse(await SendRequestAsync(new()
		{
			["choice"] = "1",       // "choice" is always "1" (idk why, don't ask me)
			["schet"] = GetCurrentSemesterId()
		}));

	public async Task<Dictionary<int, string>> GetGroupsListAsync(int facultyId, int course) =>
		ParseListResponse(await SendRequestAsync(new()
		{
			["choice"] = "1",
			["schet"] = GetCurrentSemesterId(),
			["faculty"] = facultyId.ToString(), // Specifying faculty ID returns a list of groups
			["kurs"] = course.ToString()        // Course number is actually optional, but filters out other groups. Can be set 0 to get all groups of the faculty.
		}));

	public async Task<string> GetScheduleDocumentAsync(int groupId, TimetableType timetableType) =>
		await SendRequestAsync(new()
		{
			["schet"] = GetCurrentSemesterId(),
			["type_z"] = ((int)timetableType).ToString(),
			["group"] = groupId.ToString()
		});

	public async Task<DateTime> GetSemesterStartDateAsync(int groupId)
	{
		using HttpClient client = new();
		string content = await client.GetStringAsync($"https://www.sut.ru/studentu/raspisanie/raspisanie-zanyatiy-studentov-ochnoy-i-vecherney-form-obucheniya?group={groupId}");

		using IHtmlDocument doc = new HtmlParser().ParseDocument(content);
		string labelText = doc.QuerySelector("a#rasp-prev + div:nth-child(2) > span")!.TextContent;
		int weekNumber = int.Parse(ParserUtils.NumberRegex().Match(labelText).Value);
		DateTime currentDate = DateTime.Today;
		currentDate = currentDate
			.AddDays(-(int)currentDate.DayOfWeek + 1) // Move to Monday
			.AddDays(-7 * (weekNumber - 1));          // Move back to the first week

		return currentDate;
	}

	private static Dictionary<int, string> ParseListResponse(string responseContent) =>
		responseContent
			.Split(';', StringSplitOptions.RemoveEmptyEntries)
			.Select(item => item.Split(','))
			.ToDictionary(
				parts => int.Parse(parts[0]),
				parts => parts[1]
			);

	public async Task<string> SendRequestAsync(Dictionary<string, string> formData)
	{
		HttpRequestMessage request = new(HttpMethod.Post, "https://cabinet.sut.ru/raspisanie_all_new.php")
		{
			Content = new FormUrlEncodedContent(formData)
		};

		using HttpClient client = new(new HttpClientHandler
		{
			// Sometimes Bonch being Bonch just doesn't renew its SSL certificates properly
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

		int termStartYear = now.Year - 2000;    // We need only last two digits (e.g. 25 for 2025)

		if (now.Month < 8)      // Before August means we are in the second semester of the previous academic year
			termStartYear--;

		return $"205.{termStartYear}{termStartYear + 1}/{currentSemester}";
	}
}
