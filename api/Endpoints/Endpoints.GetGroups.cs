using System.ComponentModel.DataAnnotations;
using BonchCalendar.Services;

namespace BonchCalendar.Endpoints;

public static partial class Endpoints
{
	/// <summary>
	/// Maps an endpoint to retrieve list of groups for a specific faculty and course year from sut.ru API
	/// </summary>
	public static RouteHandlerBuilder MapGetGroups(this IEndpointRouteBuilder builder) =>
		builder.MapGet("/groups", async (
			int facultyId,
			[Range(1, 5)] int? year,
			ApiService apiService,
			ILogger<Program> logger,
			IssueTrackingService tracker
		) =>
		{
			year ??= 0;     // Setting year to 0 (show all groups) if not specified in the request.

			try
			{
				Dictionary<int, string> groups = await apiService.GetGroupsListAsync(facultyId, year.Value);
				logger.LogInformation("Fetched {Count} groups (facultyId: {FacultyId}, year: {Year}).", groups.Count, facultyId, year);
				tracker.TrackGroupFetch(facultyId, year.Value, true);   // Track whether retrieving groups for this specific faculty and year was successul or not.
				return Results.Ok(groups);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to fetch groups list (facultyId: {FacultyId}, year: {Year}).", facultyId, year);
				tracker.TrackGroupFetch(facultyId, year.Value, false);  // Track whether retrieving groups for this specific faculty and year was successul or not.
				return Results.Problem(
					"Failed to fetch groups list.",
					statusCode: StatusCodes.Status503ServiceUnavailable,
					extensions: new Dictionary<string, object?>
					{
						["facultyId"] = facultyId,
						["year"] = year
					}
				);
			}
		})
			.WithName("GetGroups")
			.WithDescription("Gets the list of groups for the specified faculty and course year. If year is not specified, all groups for that faculty will be returned.")
			.Produces<Dictionary<int, string>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status503ServiceUnavailable)
			.ProducesValidationProblem();
}
