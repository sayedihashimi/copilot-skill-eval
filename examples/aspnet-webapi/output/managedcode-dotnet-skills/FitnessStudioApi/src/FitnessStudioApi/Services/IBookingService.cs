using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto, CancellationToken ct = default);
    Task<BookingDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BookingDto> CancelAsync(int id, CancelBookingDto dto, CancellationToken ct = default);
    Task<BookingDto> CheckInAsync(int id, CancellationToken ct = default);
    Task<BookingDto> MarkNoShowAsync(int id, CancellationToken ct = default);
}
