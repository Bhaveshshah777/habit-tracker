using NotificationService.Kafka.Consumer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<KafkaUserRegisteredConsumer>();

var app = builder.Build();

app.UseHttpsRedirection();
app.Run();
