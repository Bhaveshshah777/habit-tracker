using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Kafka.Consumer;
using NotificationService;

try
{
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<KafkaUserRegisteredConsumer>();
            services.AddHostedService<NewHabitConsumer>();
            services.AddHealthChecks();
            services.AddHostedService<HealthCheckHttpServer>();
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        })
        .Build()
        .Run();
}
catch (Exception ex)
{
    Console.WriteLine("Fatal error starting host: " + ex);
}

