using BonchCalendar.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BonchCalendar.Health;

/// <summary>
/// Healthcheck service for sut.ru API.
/// </summary>
public class ApiHealthCheck(IssueTrackingService trackingService) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context, CancellationToken cancellationToken = default
	)
	{
		Dictionary<string, object> report = trackingService.GetReport();

		// We deem service "unhealthy" if any of the last requests to the API were unsuccessful.
		if (report.Count > 0)
			return HealthCheckResult.Unhealthy(
				description: "We're having issues with fetching data from the timetable website.",
				data: report
			);

		return HealthCheckResult.Healthy();
	}
}
