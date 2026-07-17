using M01.OrderPaymentSystem.OrderServiceApi.Repositories;
using M01.OrderPaymentSystem.OrderServiceApi.Services;
using M01.RepositoryPattern.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderServiceApi.Exceptions;
using OrderServiceApi.HealthChecks;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHealthChecks();

builder.Host.UseSerilog((context, loggerconfiguration) =>
{
    loggerconfiguration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(res=>res.AddService("orderserviceapi"))
    .WithTracing(tracing=>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddOtlpExporter();
        metrics.AddPrometheusExporter();
    });


builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = (context) =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
    };
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source = app.db");
}).AddHealthChecks().AddCheck<DbHealthCheck>("database");

builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PaymentService:BaseUrl"]!);
});

var app = builder.Build();


app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };
        await ctx.Response.WriteAsJsonAsync(payload);
    }
}); app.UseExceptionHandler();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseStatusCodePages();
app.UseSerilogRequestLogging();



app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.Run();