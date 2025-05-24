using System;
using HabitService.Models;

namespace habit_service.Interface;

public interface IHabitService
{
    public Task<IList<Habit>> Get(Guid user_id);
    public Task<Habit?> GetById(Guid user_id, Guid habit_id);
    public Task Create(Habit habit);
    public Task Update(Habit habit);
    public Task Delete(List<Guid> habit_ids);
}
