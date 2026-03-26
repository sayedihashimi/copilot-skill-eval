using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PaginatedResponse<BookResponse>> GetAllAsync(
        string? search, bool? available, string? sortBy, string? sortDirection,
        int page, int pageSize, CancellationToken ct);
    Task<BookResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken ct);
    Task<BookResponse?> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<ReservationResponse>> GetBookReservationsAsync(int bookId, int page, int pageSize, CancellationToken ct);
}
