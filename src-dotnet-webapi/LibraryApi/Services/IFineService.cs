using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PaginatedResponse<FineResponse>> GetFinesAsync(string? status, int page, int pageSize, CancellationToken ct);
    Task<FineResponse?> GetFineByIdAsync(int id, CancellationToken ct);
    Task<(FineResponse? Fine, string? Error, bool NotFound)> PayFineAsync(int id, CancellationToken ct);
    Task<(FineResponse? Fine, string? Error, bool NotFound)> WaiveFineAsync(int id, CancellationToken ct);
}
