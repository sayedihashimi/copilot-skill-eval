using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PaginatedResponse<PatronResponse>> GetAllAsync(
        string? search, MembershipType? membershipType, int page, int pageSize, CancellationToken ct);
    Task<PatronDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<PatronResponse> CreateAsync(CreatePatronRequest request, CancellationToken ct);
    Task<PatronResponse?> UpdateAsync(int id, UpdatePatronRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<ReservationResponse>> GetPatronReservationsAsync(int patronId, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<FineResponse>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize, CancellationToken ct);
}
