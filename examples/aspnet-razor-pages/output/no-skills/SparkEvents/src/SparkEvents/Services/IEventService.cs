using SparkEvents.Models;

namespace SparkEvents.Services;

public class EventFilterModel
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public EventStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public interface IEventService
{
    Task<(List<Event> Items, int TotalCount)> GetFilteredAsync(EventFilterModel filter);
    Task<Event?> GetByIdAsync(int id);
    Task<Event?> GetByIdWithDetailsAsync(int id);
    Task CreateAsync(Event evt);
    Task UpdateAsync(Event evt);
    Task<string?> PublishAsync(int id);
    Task<string?> CancelEventAsync(int id, string reason);
    Task<string?> CompleteEventAsync(int id);
    Task<List<TicketType>> GetTicketTypesAsync(int eventId);
    Task<TicketType?> GetTicketTypeByIdAsync(int id);
    Task CreateTicketTypeAsync(TicketType ticketType);
    Task UpdateTicketTypeAsync(TicketType ticketType);
    Task<List<Event>> GetUpcomingEventsAsync(int days);
    Task<List<Event>> GetTodaysEventsAsync();
    Task<int> GetTotalEventsCountAsync();
    Task<int> GetEventsThisMonthCountAsync();
    Task<int> GetTotalRegistrationsCountAsync();
}
