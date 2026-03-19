using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct = default);
    Task<BookingResponse> CancelAsync(int id, CancelBookingRequest? request, CancellationToken ct = default);
    Task<BookingResponse> CheckInAsync(int id, CancellationToken ct = default);
    Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct = default);
}
