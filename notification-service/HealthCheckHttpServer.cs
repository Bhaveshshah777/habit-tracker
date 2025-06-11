using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace NotificationService;

public sealed class HealthCheckHttpServer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthCheckHttpServer> _logger;

    public HealthCheckHttpServer(IServiceProvider serviceProvider, ILogger<HealthCheckHttpServer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://*:80/health/");
        listener.Start();
        _logger.LogInformation("Health check server started on /health");

        while (!stoppingToken.IsCancellationRequested)
        {
            var context = await listener.GetContextAsync();

            if (context.Request.Url?.AbsolutePath == "/health")
            {
                using var scope = _serviceProvider.CreateScope();
                var healthChecks = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
                var result = await healthChecks.CheckHealthAsync(stoppingToken);

                context.Response.StatusCode = result.Status == HealthStatus.Healthy ? 200 : 503;
                context.Response.ContentType = "application/json";
                var json = JsonSerializer.Serialize(new
                {
                    status = result.Status.ToString(),
                    results = result.Entries.Select(e => new { key = e.Key, value = e.Value.Status.ToString() })
                });
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                await context.Response.OutputStream.WriteAsync(buffer);
                context.Response.Close();
            }
        }

        listener.Close();
    }
}
