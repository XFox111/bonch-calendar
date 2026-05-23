namespace BonchCalendar.Services;

public class IssueTrackingService
{
	private bool _isLastFacultyFetchSuccessful = true;

	private readonly List<string> _unsuccessfulGroupFetches = [];

	private readonly List<string> _unsuccessfulTimetableFetches = [];

	public void TrackFacultyFetch(bool isSuccessful) =>
		_isLastFacultyFetchSuccessful = isSuccessful;

	public void TrackGroupFetch(int facultyId, int course, bool isSuccessful)
	{
		string key = $"{facultyId}/{course}";

		if (!isSuccessful)
		{
			if (!_unsuccessfulGroupFetches.Contains(key))
				_unsuccessfulGroupFetches.Add(key);
		}
		else
			_unsuccessfulGroupFetches.Remove(key);
	}

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

	public Dictionary<string, object> GetReport()
	{
		Dictionary<string, object> report = [];

		if (!_isLastFacultyFetchSuccessful)
			report.Add("/faculties", false);

		if (_unsuccessfulGroupFetches.Count > 0)
			report.Add("/groups", _unsuccessfulGroupFetches.ToArray());

		if (_unsuccessfulTimetableFetches.Count > 0)
			report.Add("/timetable", _unsuccessfulTimetableFetches.ToArray());

		return report;
	}
}
