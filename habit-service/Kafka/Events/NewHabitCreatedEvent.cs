namespace HabitService.Kafka.Events;

public class NewHabitCreatedEvent
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public required string Email { get; set; }
    public TimeSpan ReminderTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
