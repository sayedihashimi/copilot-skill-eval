using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PaginatedResponse<FineResponse>> GetFinesAsync(FineStatus? status, int page, int pageSize);
    Task<FineResponse?> GetFineByIdAsync(int id);
    Task<FineResponse> PayFineAsync(int id);
    Task<FineResponse> WaiveFineAsync(int id);
}
