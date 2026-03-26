using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PaginatedResponse<BookResponse>> GetBooksAsync(string? search, bool? available, string? sortBy, int page, int pageSize);
    Task<BookResponse?> GetBookByIdAsync(int id);
    Task<BookResponse> CreateBookAsync(CreateBookRequest request);
    Task<BookResponse?> UpdateBookAsync(int id, UpdateBookRequest request);
    Task<bool> DeleteBookAsync(int id);
    Task<PaginatedResponse<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize);
    Task<PaginatedResponse<ReservationResponse>> GetBookReservationsAsync(int bookId, int page, int pageSize);
}
