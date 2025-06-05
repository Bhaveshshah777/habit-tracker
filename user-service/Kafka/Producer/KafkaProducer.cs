using System.Text.Json;
using Confluent.Kafka;
using UserService.Kafka.Interface;

namespace user_service.Kafka.Producer;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    public KafkaProducer(string bootstrapServer)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServer };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }

    public async Task ProduceAsync<T>(string topic, T message)
    {
        var json = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = json
        });
    }
}
