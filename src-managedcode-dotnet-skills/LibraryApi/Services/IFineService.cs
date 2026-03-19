using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PagedResult<FineDto>> GetAllAsync(int page, int pageSize);
    Task<FineDto?> GetByIdAsync(int id);
    Task<FineDto> PayAsync(int id);
    Task<FineDto> WaiveAsync(int id);
}
