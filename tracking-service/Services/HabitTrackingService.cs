using Dapper;
using HabitTracker.Models;
using Npgsql;

namespace HabitTracker.Services;

public class HabitTrackingService
{

    private readonly string cs;
    public HabitTrackingService(IConfiguration config)
    {
        string? cs = config["POSTGRES_CONNECTION"];
        if (string.IsNullOrEmpty(cs))
            throw new Exception("Habit:Connection string missing.");

        this.cs = cs;
    }

    public async Task<bool> CanTrack(HabitTracking model)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model), "HabitTracking cannot be null");

        string query = @"
            SELECT start_date, reminder_time 
            FROM habits 
            WHERE user_id = @UserId AND id = @Habit_Id;
        ";

        await using var conn = new NpgsqlConnection(cs);
        var habit = await conn.QueryFirstOrDefaultAsync(query, new { model.UserId, model.Habit_Id });

        if (habit is null)
            throw new Exception("Habit not found for the given user.");

        DateTime startDate = habit.start_date;
        TimeSpan? reminderTime = habit.reminder_time;

        // Combine start_date and reminder_time (if exists), else treat reminder time as 00:00
        DateTime scheduledDateTime = startDate.Date.Add(reminderTime ?? TimeSpan.Zero);

        return DateTime.UtcNow >= scheduledDateTime;
    }

    public async Task<bool> TrackHabit(HabitTracking model)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model), "HabitTracking cannot be null");

        string query = @"
            INSERT INTO habit_trackings (user_id, habit_id, is_completed, tracked_date, note) 
            VALUES (@UserId, @Habit_Id, @IsCompleted, NOW(), @Note)";

        try
        {
            await using NpgsqlConnection conn = new NpgsqlConnection(cs);
            await conn.ExecuteAsync(query, model);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            // Handle unique constraint violation (e.g., duplicate entry)            
            return false;
        }
        return true;
    }
}
