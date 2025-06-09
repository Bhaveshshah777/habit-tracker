using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Kafka.Event;

namespace NotificationService.Kafka.Consumer;

public class NewHabitConsumer : BackgroundService
{
    private readonly ILogger<NewHabitConsumer> _logger;
    private readonly string _topic = "habit.created";
    private readonly string _bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "kafka:9092";
    private readonly string _groupId = "notification-habit-service";

    private IConsumer<string, string>? _consumer;

    public NewHabitConsumer(ILogger<NewHabitConsumer> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();

        _consumer.Subscribe(_topic);
        _logger.LogInformation("Subscribed to Kafka topic: {Topic}", _topic);

        return Task.Run(() =>
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = _consumer.Consume(stoppingToken);
                    _logger.LogInformation("Received: {Message}", cr.Message.Value);

                    var habitCreatedEvent = JsonSerializer.Deserialize<NewHabitEvent>(cr.Message.Value);
                    _logger.LogInformation("Habit name: {Name}", habitCreatedEvent?.Name);

                    _logger.LogWarning($"Sending congratulations email to {habitCreatedEvent?.Email}...");
                }
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                _logger.LogWarning("Topic not found during consume: {Reason}", ex.Error.Reason);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("NewHabitConsumer is stopping...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in NewHabitConsumer");
            }
            finally
            {
                _consumer?.Close();
                _consumer?.Dispose();
            }
        }, stoppingToken);
    }
}
