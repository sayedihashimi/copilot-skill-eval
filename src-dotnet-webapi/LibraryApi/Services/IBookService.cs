using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PaginatedResponse<BookResponse>> GetBooksAsync(string? search, bool? available, string? sortBy, string? sortOrder, int page, int pageSize, CancellationToken ct);
    Task<BookDetailResponse?> GetBookByIdAsync(int id, CancellationToken ct);
    Task<BookResponse> CreateBookAsync(CreateBookRequest request, CancellationToken ct);
    Task<BookResponse?> UpdateBookAsync(int id, UpdateBookRequest request, CancellationToken ct);
    Task<(bool Found, bool HasActiveLoans)> DeleteBookAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<LoanResponse>?> GetBookLoansAsync(int bookId, CancellationToken ct);
    Task<IReadOnlyList<ReservationResponse>?> GetBookReservationsAsync(int bookId, CancellationToken ct);
}
