using SparkEvents.Models;

namespace SparkEvents.Services;

public interface ICheckInService
{
    Task<(CheckIn? CheckIn, string? Error)> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes);
    Task<(List<Registration> Items, int TotalCheckedIn, int TotalConfirmed)> GetCheckInDashboardAsync(int eventId, string? search);
    Task<Registration?> LookupForCheckInAsync(int eventId, string searchTerm);
}
