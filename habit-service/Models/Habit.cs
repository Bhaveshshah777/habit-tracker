using System.ComponentModel.DataAnnotations;

namespace HabitService.Models;

public class Habit
{
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = "";
    [Required]
    public Guid UserId { get; set; }

    public string? Description { get; set; }
    public int Frequency { get; set; } = 1;
    public DateTime StartDate { get; set; } = DateTime.Now;
    public TimeSpan ReminderTime { get; set; } = new TimeSpan(10, 0, 0);
    public string? Category { get; set; }
}