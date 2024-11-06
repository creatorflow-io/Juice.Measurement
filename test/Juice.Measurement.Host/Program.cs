using Juice.Measurement;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args
});

// Add services to the container.
builder.Services.AddExecutionTimeMeasurement();
builder.Services.AddMeasurementEFStores(builder.Configuration, options =>
{
    options.DatabaseProvider = "SqlServer";
});
builder.Services.AddGrpc(o => o.EnableDetailedErrors = true);

var app = builder.Build();
app.UseMiddleware<ExecutionTimeMiddleware>();

app.MapMeasurementGrpcServices();
app.MapGet("/health", async context =>
{
    await context.Response.WriteAsync("Healthy");
});
app.Run();


public partial class Program { }

internal class ExecutionTimeMiddleware
{
    private readonly RequestDelegate _next;

    public ExecutionTimeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tracker = context.RequestServices.GetRequiredService<ITimeTracker>();
        var logger = context.RequestServices.GetRequiredService<ILogger<ExecutionTimeMiddleware>>();
        using var _ = tracker.BeginScope("Request", "timetracker.middleware");
        await _next(context);
        logger.LogInformation(tracker.ToString());
    }
}
