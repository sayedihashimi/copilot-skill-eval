using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetAllAsync(int page, int pageSize);
    Task<ReservationDto?> GetByIdAsync(int id);
    Task<ReservationDto> CreateAsync(CreateReservationDto dto);
    Task<ReservationDto> CancelAsync(int id);
    Task<ReservationDto> FulfillAsync(int id);
}
