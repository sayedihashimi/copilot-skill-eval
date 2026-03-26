using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IRegistrationService
{
    Task<(Registration? Registration, string? Error)> RegisterAsync(int eventId, int attendeeId, int ticketTypeId, string? specialRequests);
    Task<Registration?> GetByIdAsync(int id);
    Task<Registration?> GetByConfirmationNumberAsync(string confirmationNumber);
    Task<(List<Registration> Items, int TotalCount)> GetEventRosterAsync(int eventId, string? search, int page, int pageSize);
    Task<List<Registration>> GetEventWaitlistAsync(int eventId);
    Task<string?> CancelRegistrationAsync(int id, string? reason);
    Task<List<Registration>> GetRecentRegistrationsAsync(int count);
}
