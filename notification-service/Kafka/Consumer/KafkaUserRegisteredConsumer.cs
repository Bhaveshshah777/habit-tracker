using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Kafka.Event;

namespace NotificationService.Kafka.Consumer;

public class KafkaUserRegisteredConsumer : BackgroundService
{
    private readonly ILogger<KafkaUserRegisteredConsumer> _logger;
    private readonly string _topic = "user.registered";
    private readonly string _bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "kafka:9092";
    private readonly string _groupId = "notification-user-service";
    private IConsumer<string, string>? _consumer;

    public KafkaUserRegisteredConsumer(ILogger<KafkaUserRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();

        _consumer.Subscribe(_topic);
        _logger.LogInformation("Subscribed to Kafka topic: {Topic}", _topic);

        return Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _consumer.Consume(stoppingToken);
                    _logger.LogInformation("Received: {Message}", cr.Message.Value);

                    var userRegisteredEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(cr.Message.Value);
                    _logger.LogInformation("User Registered: {Email}", userRegisteredEvent?.Email);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message");
                }
            }

            _consumer?.Close(); // graceful shutdown
        });
    }
}
