using HabitTracker.Models;
using HabitTracker.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<HabitTrackingService>();
var app = builder.Build();
app.UseHttpsRedirection();
app.MapGet("/", () => "Welcome to tracking service.");
app.MapPost("/", async ([FromBody] HabitTracking model, [FromServices] HabitTrackingService service) =>
{
    if (model == null || model.UserId == Guid.Empty || model.Habit_Id == Guid.Empty)
        return Results.BadRequest("Invalid habit tracking data.");

    bool canTrack = await service.CanTrack(model);
    if (!canTrack)
        return Results.BadRequest("Habit cannot be tracked as the scheduled time hasn't come yet.");

    bool result = await service.TrackHabit(model);
    if (result == false)
        return Results.BadRequest("Habit tracking already exists.");

    return Results.Ok("Habit tracking recorded successfully.");
});
app.Run();
