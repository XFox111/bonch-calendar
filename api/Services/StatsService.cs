namespace BonchCalendar.Services;

/// <summary>
/// Service for tracking unique visits to /timetable endpoint, essentially for counting unique users.
/// </summary>
public class StatsService
{
	private readonly HashSet<string> ids = [];

	/// <summary>
	/// Gets the number of unique IDs recorded by this service. This can be used to estimate the number of unique users that have accessed the /timetable endpoint.
	/// </summary>
	/// <returns>The number of unique IDs recorded by this service.</returns>
	public int GetUniqueIdCount() => ids.Count;

	/// <summary>
	/// Records a unique ID. If the ID has already been recorded, it will not be added again.
	/// </summary>
	/// <param name="id">The unique ID to record.</param>
	public void RecordId(string id)
	{
		if (!string.IsNullOrEmpty(id) && !ids.Contains(id))
			ids.Add(id);
	}
}
