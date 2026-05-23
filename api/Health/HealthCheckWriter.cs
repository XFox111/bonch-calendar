using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BonchCalendar.Health;

public static class HealthCheckWriter
{
	private static readonly byte[] _emptyResponse = [ (byte)'{', (byte)'}' ];
	private static JsonSerializerContext? _jsonContext = null;

	public static JsonSerializerContext JsonContext => _jsonContext ??= CreateSerializerContext();

	public static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
	{
		if (report is null)
		{
			await context.Response.BodyWriter.WriteAsync(_emptyResponse).ConfigureAwait(false);
			return;
		}

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

		context.Response.ContentType = "application/json; charset=utf-8";

		await JsonSerializer.SerializeAsync(context.Response.Body, response, typeof(HealthResponse), JsonContext).ConfigureAwait(false);
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

public record HealthResponse(
	HealthStatus Status,
	TimeSpan TotalDuration,
	IReadOnlyDictionary<string, HealthResponseEntry> Entries
);

public record HealthResponseEntry(
	HealthStatus Status,
	string? Description,
	TimeSpan Duration,
	IReadOnlyDictionary<string, object> Data
);
