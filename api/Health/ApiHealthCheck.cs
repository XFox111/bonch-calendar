using BonchCalendar.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BonchCalendar.Health;

public class ApiHealthCheck(ApiService groupService) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context, CancellationToken cancellationToken = default
	)
	{
		try
		{
			Dictionary<int, string> faculties = await groupService.GetFacultiesListAsync();

			if (faculties.Count > 0)
				return HealthCheckResult.Healthy();

			return HealthCheckResult.Degraded(description: "Timetable website looks to be up, but returned an empty list of faculties.");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy(description: "Timetable website appears to be down.", exception: ex);
		}
	}
}
