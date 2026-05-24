namespace BonchCalendar.Services;

/// <summary>
/// Service that tracks results of the most recent requests.
/// </summary>
public class IssueTrackingService
{
	private bool _isLastFacultyFetchSuccessful = true;

	private readonly List<string> _unsuccessfulGroupFetches = [];

	private readonly List<string> _unsuccessfulTimetableFetches = [];

	/// <summary>
	/// Record whether the last attempt to retrieve faculty list was successful.
	/// </summary>
	/// <param name="isSuccessful">Set <c>true</c> if the attempt was successful. Otherwise, set <c>false</c></param>
	public void TrackFacultyFetch(bool isSuccessful) =>
		_isLastFacultyFetchSuccessful = isSuccessful;

	/// <summary>
	/// Record whether the last attempt to retrieve groups for provided faculty and term year was successful.
	/// </summary>
	/// <param name="facultyId">ID of a faculty which was used to retrieve the group list.</param>
	/// <param name="termYear">Term year which was used to retrieve the group list.</param>
	/// <param name="isSuccessful">Set <c>true</c> if the attempt was successful. Otherwise, set <c>false</c></param>
	public void TrackGroupFetch(int facultyId, int termYear, bool isSuccessful)
	{
		string key = $"{facultyId}/{termYear}";

		if (!isSuccessful)
		{
			if (!_unsuccessfulGroupFetches.Contains(key))
				_unsuccessfulGroupFetches.Add(key);
		}
		else
			_unsuccessfulGroupFetches.Remove(key);
	}

	/// <summary>
	/// Record whether the last attempt to retrieve timetable for provided group was successful.
	/// </summary>
	/// <param name="facultyId">ID of a faculty the group belongs to.</param>
	/// <param name="groupId">ID of a group the timetable was retrieved for.</param>
	/// <param name="isSuccessful">Set <c>true</c> if the attempt was successful. Otherwise, set <c>false</c></param>
	public void TrackTimetableFetch(int facultyId, int groupId, bool isSuccessful)
	{
		string key = $"{facultyId}/{groupId}";

		if (!isSuccessful)
		{
			if (!_unsuccessfulTimetableFetches.Contains(key))
				_unsuccessfulTimetableFetches.Add(key);
		}
		else
			_unsuccessfulTimetableFetches.Remove(key);
	}

	/// <summary>
	/// Get report on the success of latest retrieval attempts.
	/// </summary>
	/// <returns>A dictionary for each of the tracked groups.</returns>
	/// <remarks>
	/// If the dictionary is empty, that means that there're no known issues.
	/// </remarks>
	public Dictionary<string, object> GetReport()
	{
		Dictionary<string, object> report = [];

		if (!_isLastFacultyFetchSuccessful)
			report.Add("/faculties", false);

		if (_unsuccessfulGroupFetches.Count > 0)
			report.Add("/groups", _unsuccessfulGroupFetches.ToArray());

		if (_unsuccessfulTimetableFetches.Count > 0)
			report.Add("/timetable", _unsuccessfulTimetableFetches.ToArray());

		// No issues example:
		/*
		 * { }
		 */

		// Report example with issues:
		/*
		 * {
		 *     "/faculties": false,
		 *     "/groups": [
		 *         "123/1",
		 *         "321/3"
		 *     ],
		 *     "/timetable": [
		 *         "123/321",
		 *         "456/654"
		 *     ],
		 * }
		 */

		return report;
	}
}
