using HabitService.Interface;
using HabitService.Kafka.Interface;
using HabitService.Kafka.Producer;

namespace HabitService.Services;

public static class ServiceDependancies
{
    public static IServiceCollection ServiceInjector(this IServiceCollection service)
    {
        service.AddSingleton<IKafkaProducer>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var bootstrapServers = config["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092";
            return new KafkaProducer(bootstrapServers);
        });
        service.AddScoped<IHabitService, HabitService>();
        return service;
    }
}
