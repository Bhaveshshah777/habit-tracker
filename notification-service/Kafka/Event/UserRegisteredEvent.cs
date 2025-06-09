namespace NotificationService.Kafka.Event;

public class UserRegisteredEvent
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime RegisteredAt { get; set; }
}
