using Confluent.Kafka;
using Confluent.Kafka.Admin;

var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "kafka:9092";

var config = new AdminClientConfig { BootstrapServers = bootstrapServers };

using var adminClient = new AdminClientBuilder(config).Build();

try
{
    await adminClient.CreateTopicsAsync(new[]
    {
        new TopicSpecification
        {
            Name = "habit.created",
            NumPartitions = 1,
            ReplicationFactor = 1
        },
        new TopicSpecification
        {
            Name = "user.registered",
            NumPartitions = 1,
            ReplicationFactor = 1
        }
    });

    Console.WriteLine("✅ Kafka topics created.");
}
catch (CreateTopicsException e)
{
    foreach (var result in e.Results)
    {
        if (result.Error.Code != ErrorCode.TopicAlreadyExists)
            Console.WriteLine($"❌ Failed to create topic {result.Topic}: {result.Error.Reason}");
        else
            Console.WriteLine($"ℹ️ Topic already exists: {result.Topic}");
    }
}
