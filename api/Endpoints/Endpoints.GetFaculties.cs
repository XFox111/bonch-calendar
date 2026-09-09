using BonchCalendar.Services;
using Microsoft.AspNetCore.Mvc;

namespace BonchCalendar.Endpoints;

/// <summary>
/// Maps endpoints for retrieving faculties, groups and timetable from sut.ru API
/// </summary>
public static partial class Endpoints
{
	/// <summary>
	/// Maps an endpoint to retrieve list of faculties and their IDs from sut.ru API
	/// </summary>
	public static RouteHandlerBuilder MapGetFaculties(this IEndpointRouteBuilder builder) =>
		builder.MapGet("/faculties", async (
			ApiService apiService,
			ILogger<Program> logger,
			IssueTrackingService tracker
		) =>
		{
			try
			{
				Dictionary<int, string> faculties = await apiService.GetFacultiesListAsync();
				logger.LogInformation("Fetched {Count} faculties.", faculties.Count);
				tracker.TrackFacultyFetch(true);        // Record that last attempt to retrieve faculties was successful
				return Results.Ok(faculties);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to fetch faculties list.");
				tracker.TrackFacultyFetch(false);       // Record that last attempt to retrieve faculties was unsuccessful
				return Results.Problem("Failed to fetch faculties list.", statusCode: StatusCodes.Status503ServiceUnavailable);
			}
		})
			.WithName("GetFaculties")
			.WithDescription("Gets the list of faculties.")
			.ProducesProblem(StatusCodes.Status503ServiceUnavailable)
			.Produces<Dictionary<int, string>>(StatusCodes.Status200OK);
}
