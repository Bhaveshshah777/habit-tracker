using System;
using Dapper;
using HabitService.Interface;
using HabitService.Kafka.Events;
using HabitService.Kafka.Interface;
using HabitService.Models;
using Npgsql;

namespace HabitService.Services;

public class HabitService : IHabitService
{
    private readonly string cs;
    private readonly IKafkaProducer _producer;

    public HabitService(IConfiguration config, IKafkaProducer producer)
    {
        string? cs = config["POSTGRES_CONNECTION"];
        if (string.IsNullOrEmpty(cs))
            throw new Exception("Habit:Connection string missing.");

        this.cs = cs;
        _producer = producer;
    }

    public async Task<IList<Habit>> Get(Guid user_id)
    {
        string query = @"
        SELECT * FROM habits
        WHERE user_id = @user_id";

        await using var conn = new NpgsqlConnection(cs);
        return (await conn.QueryAsync<Habit>(query, new { user_id })).ToList();
    }

    public async Task<Habit?> GetById(Guid user_id, Guid habit_id)
    {
        string query = @"
        SELECT * FROM habits
        WHERE id = @habit_id and user_id = @user_id";

        await using var conn = new NpgsqlConnection(cs);
        return await conn.QuerySingleOrDefaultAsync<Habit>(query, new { habit_id, user_id });
    }

    public async Task Create(Habit habit)
    {
        if (habit is null)
            throw new ArgumentNullException(nameof(habit), "Habit cannot be null");
        Console.WriteLine($"-------------Creating habit for user: {habit.UserId}");
        string query = @"
            INSERT INTO habits (user_id, title, description, frequency, start_date, reminder_time, category) 
            VALUES (@UserId, @Title, @Description, @Frequency, @StartDate, @ReminderTime, @Category)";
        await using NpgsqlConnection conn = new NpgsqlConnection(cs);
        int rowsAffected = await conn.ExecuteAsync(query, habit);

        if (rowsAffected == 1)
        {
            // Create NewHabitCreatedEvent
            var newHabitEvent = new NewHabitCreatedEvent
            {
                Name = habit.Title,
                Description = habit.Description ?? string.Empty,
                Email = habit.Email,
                ReminderTime = habit.ReminderTime,
                CreatedAt = DateTime.UtcNow
            };
            await _producer.ProduceAsync("habit.created", newHabitEvent);
        }
    }

    public async Task Update(Habit habit)
    {
        if (habit is null)
            throw new ArgumentNullException(nameof(habit), "Habit cannot be null");

        string query = @"
        UPDATE habits
        SET title = @Title, description = @Description, frequency = @Frequency, 
            start_date = @StartDate, reminder_time = @ReminderTime, category = @Category,
            updated_at = NOW()
        WHERE id = @Id";

        await using var conn = new NpgsqlConnection(cs);
        await conn.ExecuteAsync(query, habit);
    }

    public async Task Delete(List<Guid> habit_ids)
    {
        string query = @"
        DELETE FROM habits
        WHERE id = ANY(@habit_ids)";
        await using var conn = new NpgsqlConnection(cs);
        await conn.ExecuteAsync(query, new { habit_ids });
    }
}

