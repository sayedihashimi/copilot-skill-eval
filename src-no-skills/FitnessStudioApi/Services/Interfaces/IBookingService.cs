using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);
    Task<BookingDto?> GetByIdAsync(int id);
    Task<BookingDto> CancelAsync(int id, CancelBookingDto dto);
    Task<BookingDto> CheckInAsync(int id);
    Task<BookingDto> NoShowAsync(int id);
}
