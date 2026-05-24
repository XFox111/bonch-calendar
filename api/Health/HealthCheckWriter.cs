using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BonchCalendar.Health;

public static class HealthCheckWriter
{
	private static readonly byte[] _emptyResponse = [ (byte)'{', (byte)'}' ];
	private static readonly JsonSerializerContext _jsonContext = CreateSerializerContext();

	public static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
	{
		if (report is null)
		{
			// Just in case, but this should not ever happen.
			await context.Response.BodyWriter.WriteAsync(_emptyResponse).ConfigureAwait(false);
			return;
		}

		// Create DTO from the report.
		HealthResponse response = new(
			Status: report.Status,
			TotalDuration: report.TotalDuration,
			Entries: report.Entries.ToDictionary(
				e => e.Key,
				e => new HealthResponseEntry(
					Status: e.Value.Status,
					Description: e.Value.Description,
					Duration: e.Value.Duration,
					Data: e.Value.Data
				)
			)
		);

		// Write DTO to the response body.
		context.Response.ContentType = "application/json; charset=utf-8";
		await JsonSerializer.SerializeAsync(context.Response.Body, response, typeof(HealthResponse), _jsonContext).ConfigureAwait(false);
	}

	private static AppJsonSerializerContext CreateSerializerContext()
	{
		JsonSerializerOptions options = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		};

		options.Converters.Add(new JsonStringEnumConverter<HealthStatus>(options.PropertyNamingPolicy));
		options.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);

		return new AppJsonSerializerContext(options);
	}
}

/// <summary>
/// Response body for /health endpoint.
/// </summary>
/// <param name="Status">Overall status of the application.</param>
/// <param name="TotalDuration">Total time it took to complete the health check.</param>
/// <param name="Entries">List of subcomponent healthcheck reports.</param>
public record HealthResponse(
	HealthStatus Status,
	TimeSpan TotalDuration,
	IReadOnlyDictionary<string, HealthResponseEntry> Entries
);

/// <summary>
/// Healthcheck report for a subcomponent.
/// </summary>
/// <param name="Status">Status of the subcomponent.</param>
/// <param name="Description">Report remarks.</param>
/// <param name="Duration">Time it took to complete health check for this subcomponent.</param>
/// <param name="Data">Addtional report data.</param>
public record HealthResponseEntry(
	HealthStatus Status,
	string? Description,
	TimeSpan Duration,
	IReadOnlyDictionary<string, object> Data
);
