using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PagedResponse<BookResponse>> GetAllAsync(string? search, string? category, bool? available, string? sortBy, string? sortDirection, int page, int pageSize, CancellationToken ct);
    Task<BookDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken ct);
    Task<BookResponse?> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<PagedResponse<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize, CancellationToken ct);
    Task<List<ReservationResponse>> GetBookReservationsAsync(int bookId, CancellationToken ct);
}
