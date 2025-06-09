namespace HabitService.Kafka.Interface;

public interface IKafkaProducer
{
    public Task ProduceAsync<T>(string topic, T message);
}
