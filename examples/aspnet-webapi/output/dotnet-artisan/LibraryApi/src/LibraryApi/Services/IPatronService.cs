using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PaginatedResponse<PatronResponse>> GetPatronsAsync(string? search, MembershipType? membershipType, int page, int pageSize);
    Task<PatronResponse?> GetPatronByIdAsync(int id);
    Task<PatronResponse> CreatePatronAsync(CreatePatronRequest request);
    Task<PatronResponse?> UpdatePatronAsync(int id, UpdatePatronRequest request);
    Task<bool> DeactivatePatronAsync(int id);
    Task<PaginatedResponse<LoanResponse>> GetPatronLoansAsync(int patronId, LoanStatus? status, int page, int pageSize);
    Task<PaginatedResponse<ReservationResponse>> GetPatronReservationsAsync(int patronId, int page, int pageSize);
    Task<PaginatedResponse<FineResponse>> GetPatronFinesAsync(int patronId, FineStatus? status, int page, int pageSize);
}
