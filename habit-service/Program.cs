using System.Data;
using System.Text.Json;
using HabitService.Interface;
using HabitService.Services;
using HabitService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")));
builder.Services.AddControllers();
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

builder.Services.ServiceInjector();
builder.Services.AddAuthorization();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

//adding messages
var messages = JsonSerializer.Deserialize<Dictionary<string, string>>(
    File.ReadAllText("messages.json"));

if (messages is null)
    throw new Exception("Messages file is empty or not found.");

builder.Services.AddSingleton(messages);

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

string? cs = builder.Configuration["POSTGRES_CONNECTION"];
if (string.IsNullOrEmpty(cs))
    throw new Exception("Habit:Connection string missing.");

app.Run();
