using BonchCalendar;
using BonchCalendar.Endpoints;
using BonchCalendar.Health;
using BonchCalendar.Services;
using BonchCalendar.Utils;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

// Adding static JSON serializer since we're running in Native AOT
builder.Services.ConfigureHttpJsonOptions(options =>
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default)
);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();      // OpenAPI specification generator
builder.Services.AddValidation();   // Request validation

// Customizing non-200 responses to include trace identifier and a no-as-a-service reason
builder.Services.AddProblemDetails(configure =>
{
	configure.CustomizeProblemDetails = context =>
	{
		context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
		context.ProblemDetails.Extensions["naas_reason"] = new NaasReasons().GetReason();
	};
});

builder.Services
	.AddSingleton<IssueTrackingService>()       // Service for tracking latest API request results
	.AddSingleton<StatsService>()               // Service for tracking unique IDs of requests
	.AddScoped<TimetableService>()              // Service for generating a timetable
	.AddScoped<ApiService>()                    // Service for making API calls to sut.ru API
	.AddScoped<ParsingService>();               // Service for parsing timetable from sut.ru

builder.Services.AddHealthChecks()
	.AddCheck<ApiHealthCheck>("timetable_website");     // Healthcheck service

// Get ORIGIN_DOMAIN environmental variable
string? corsDomain = Environment.GetEnvironmentVariable("ORIGIN_DOMAIN");

// Configure defautl CORS policy
builder.Services.AddCors(options =>
	options.AddDefaultPolicy(policy =>
	{
		// Allow only GET requests with any headers
		policy
			.WithMethods(["GET"])
			.AllowAnyHeader();

		// If ORIGIN_DOMAIN environmental variable is set, allow request only from this domain,
		// otherwise allow request from any domains.
		if (string.IsNullOrWhiteSpace(corsDomain))
			policy.AllowAnyOrigin();
		else
			policy.WithOrigins(corsDomain);
	})
);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors();                  // Enable CORS
app.UseStatusCodePages();       // Enable default JSON response body for non-200 responses.
app.MapOpenApi(pattern: "/openapi.yaml");               // Map OpenAPI sepcification. Available at /openapi.yaml
app.MapScalarApiReference(options =>
	options
		.WithOpenApiRoutePattern("/openapi.yaml")
		.WithYamlDocumentDownload()
		.HideDeveloperTools()
		.DisableMcp()
);

// Map healthcheck endpoint with custom response writer
// Remark: /health and /openapi/v1.json endpoints are not present in OpenAPI specification.
app.MapHealthChecks("/health", new HealthCheckOptions
{
	ResponseWriter = HealthCheckWriter.WriteHealthCheckResponse
});

// Statistics endpoint. Shows number of active users.
app.MapGet("/stats", (StatsService statsService) =>
	Results.Ok(new StatsResponse(statsService.GetUniqueIdCount()))
)
	.WithName("GetStats")
	.WithDescription("Get basic usage statistics.")
	.Produces<StatsResponse>(StatusCodes.Status200OK);

app.MapGetFaculties();
app.MapGetGroups();
app.MapGetTimetable();

// Start the application.
app.Run();
