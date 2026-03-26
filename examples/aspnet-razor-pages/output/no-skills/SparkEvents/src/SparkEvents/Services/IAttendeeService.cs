using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IAttendeeService
{
    Task<(List<Attendee> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
    Task<List<Attendee>> GetAllAsync();
    Task<Attendee?> GetByIdAsync(int id);
    Task<Attendee?> GetByIdWithRegistrationsAsync(int id);
    Task<Attendee?> GetByEmailAsync(string email);
    Task CreateAsync(Attendee attendee);
    Task UpdateAsync(Attendee attendee);
}
