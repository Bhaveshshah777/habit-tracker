using System.ComponentModel.DataAnnotations;

namespace HabitService.Models.DTO;

public class HabitDTO
{
    [Required]
    public required string Title { get; set; }
    [Required]
    public Guid UserId { get; set; }    
    public string? Description { get; set; }
    public int? Frequency { get; set; }
    public DateTime? StartDate { get; set; }
    public TimeSpan? ReminderTime { get; set; }
    public string? Category { get; set; }
}
