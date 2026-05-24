namespace BonchCalendar;

/// <summary>
/// Response body object for /stats endpoint.
/// </summary>
/// <param name="ActiveUsers">Number of active users.</param>
public record StatsResponse(int ActiveUsers);
