using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PagedResult<PatronDto>> GetAllAsync(int page, int pageSize);
    Task<PatronDto?> GetByIdAsync(int id);
    Task<PatronDto> CreateAsync(CreatePatronDto dto);
    Task<PatronDto?> UpdateAsync(int id, UpdatePatronDto dto);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<LoanDto>> GetPatronLoansAsync(int patronId, int page, int pageSize);
    Task<PagedResult<ReservationDto>> GetPatronReservationsAsync(int patronId, int page, int pageSize);
    Task<PagedResult<FineDto>> GetPatronFinesAsync(int patronId, int page, int pageSize);
}
