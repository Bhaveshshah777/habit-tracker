using System.Data;
using System.Text.Json;
using habit_service.Interface;
using habit_service.Services;
using HabitService.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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

#region Endpoints
// Get all habits
app.MapGet("/{user_id:guid}", async (
    [FromServices] IHabitService habitService,
    Guid user_id) =>
{
    try
    {
        var habits = await habitService.Get(user_id);
        return Results.Ok(habits);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error fetching habits: {ex.Message}");
    }
});

// Get habit by ID
app.MapGet("/{user_id:guid}/{habit_id:guid}", async (
    [FromServices] IHabitService habitService,
    Guid user_id,
    Guid habit_id) =>
{
    try
    {
        var habit = await habitService.GetById(user_id, habit_id);
        if (habit is null)
            return Results.NotFound($"Habit with ID '{habit_id}' not found.");
        return Results.Ok(habit);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error fetching habit: {ex.Message}");
    }
});

// Create habit
app.MapPost("/", async (
    [FromServices] IHabitService habitService,
    [FromBody] Habit habit) =>
{
    if (habit is null)
        return Results.BadRequest("Habit cannot be null");

    try
    {
        await habitService.Create(habit);
        return Results.Ok($"Habit '{habit.Title}' created successfully.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error creating habit: {ex.Message}");
    }
});

// Update habit
app.MapPut("/", async (
    [FromServices] IHabitService habitService,
    [FromBody] Habit habit) =>
{
    if (habit is null)
        return Results.BadRequest("Habit cannot be null");

    try
    {
        await habitService.Update(habit);
        return Results.Ok($"Habit '{habit.Title}' updated successfully.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error updating habit: {ex.Message}");
    }
});

// Delete habits
app.MapDelete("/", async (
    [FromServices] IHabitService habitService,
    [FromBody] List<Guid> habit_ids) =>
{
    if (habit_ids is null)
        return Results.BadRequest("Habit IDs cannot be null");

    if (habit_ids.Count == 0)
        return Results.Ok("Nothing to delete, no habit IDs provided.");

    try
    {
        await habitService.Delete(habit_ids);
        return Results.Ok($"Deleted {habit_ids.Count} habit(s) successfully.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error deleting habits: {ex.Message}");
    }
});

#endregion

app.Run();
