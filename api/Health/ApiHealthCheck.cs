using BonchCalendar.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BonchCalendar.Health;

public class ApiHealthCheck(IssueTrackingService trackingService) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context, CancellationToken cancellationToken = default
	)
	{
		Dictionary<string, object> report = trackingService.GetReport();

		if (report.Count > 0)
			return HealthCheckResult.Unhealthy(description: "We're having issues with fetching data from the timetable website.", data: report);

		return HealthCheckResult.Healthy();
	}
}
