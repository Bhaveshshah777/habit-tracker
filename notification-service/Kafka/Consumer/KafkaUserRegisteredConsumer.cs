using System.Text.Json;
using Confluent.Kafka;
using NotificationService.Kafka.Events;

namespace NotificationService.Kafka.Consumer;

public class KafkaUserRegisteredConsumer : BackgroundService
{
    private readonly ILogger<KafkaUserRegisteredConsumer> _logger;
    private readonly string _bootstrapServers = "kafka:9092";
    private readonly string _groupId = "notification-service";

    public KafkaUserRegisteredConsumer(ILogger<KafkaUserRegisteredConsumer> logger)
    {
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string userRegisterTopic = "user.registered";

        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(userRegisterTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(stoppingToken);
                _logger.LogInformation("Received message: {Message}", cr.Message.Value);

                // Optional: deserialize if needed
                var evt = JsonSerializer.Deserialize<UserRegisteredEvent>(cr.Message.Value);
                _logger.LogInformation("User Registered: {Email}", evt?.Email);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError("Kafka consume error: {Reason}", ex.Error.Reason);
            }
        }
    }
}
