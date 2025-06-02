namespace HabitTracker.Models;

public class HabitTracking
{
    public Guid UserId { get; set; }
    public Guid Habit_Id { get; set; }
    public bool IsCompleted { get; set; }
    public string? Note { get; set; }
}