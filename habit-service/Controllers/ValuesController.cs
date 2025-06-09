using HabitService.Interface;
using HabitService.Models; // Replace with your actual namespace
using HabitService.Models.DTO;
using HabitService.Services; // Replace with your actual namespace
using Microsoft.AspNetCore.Mvc;

namespace HabitService.Controllers;

[ApiController]
public class ValuesController : ControllerBase
{
    private readonly IHabitService _habitService;

    public ValuesController(IHabitService habitService)
    {
        _habitService = habitService;
    }

    [HttpGet("{user_id:guid}")]
    public async Task<IActionResult> GetAll(Guid user_id)
    {
        try
        {
            var habits = await _habitService.Get(user_id);
            return Ok(habits);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error fetching habits: {ex.Message}");
        }
    }
    
    [HttpGet("{user_id:guid}/{habit_id:guid}")]
    public async Task<IActionResult> GetById(Guid user_id, Guid habit_id)
    {
        try
        {
            var habit = await _habitService.GetById(user_id, habit_id);
            if (habit is null)
                return NotFound($"Habit with ID '{habit_id}' not found.");
            return Ok(habit);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error fetching habit: {ex.Message}");
        }
    }

    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] HabitDTO model)
    {        
        try
        {
            Habit habit = new Habit
            {
                Title = model.Title,
                UserId = model.UserId,
                Category = model.Category
            };

            if (!string.IsNullOrEmpty(model.Description))
                habit.Description = model.Description;

            if (model.Frequency is not null)
                habit.Frequency = model.Frequency.Value;

            if (model.StartDate is not null)
                habit.StartDate = model.StartDate.Value;

            if (model.ReminderTime is not null)
                habit.ReminderTime = model.ReminderTime.Value;

            habit.Email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
            await _habitService.Create(habit);
            return Ok($"Habit '{habit.Title}' created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating habit: {ex.Message}");
        }
    }

    [HttpPut("")]
    public async Task<IActionResult> Update([FromBody] Habit habit)
    {
        if (habit is null)
            return BadRequest("Habit cannot be null");

        try
        {
            await _habitService.Update(habit);
            return Ok($"Habit '{habit.Title}' updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error updating habit: {ex.Message}");
        }
    }

    [HttpDelete("")]
    public async Task<IActionResult> Delete([FromBody] List<Guid> habit_ids)
    {
        if (habit_ids is null)
            return BadRequest("Habit IDs cannot be null");

        if (habit_ids.Count == 0)
            return Ok("Nothing to delete, no habit IDs provided.");

        try
        {
            await _habitService.Delete(habit_ids);
            return Ok($"Deleted {habit_ids.Count} habit(s) successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error deleting habits: {ex.Message}");
        }
    }
}
