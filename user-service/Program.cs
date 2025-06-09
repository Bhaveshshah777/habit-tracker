using System.Data;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using user_service.Kafka.Producer;
using UserService.Kafka.Interface;
using UserService.Repositories;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration["POSTGRES_CONNECTION"];
if (string.IsNullOrEmpty(connectionString))
    throw new Exception("User:Connection string missing.");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(connectionString));
builder.Services.AddScoped<UserRepo>();
builder.Services.AddControllers();

builder.Services.AddSingleton<IKafkaProducer>(new KafkaProducer(
    bootstrapServer: builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "kafka:9092"
));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://accounts.google.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        options.MapInboundClaims = false;
    });


builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();