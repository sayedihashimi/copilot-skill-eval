using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PagedResult<BookDto>> GetAllAsync(int page, int pageSize);
    Task<BookDto?> GetByIdAsync(int id);
    Task<BookDto> CreateAsync(CreateBookDto dto);
    Task<BookDto?> UpdateAsync(int id, UpdateBookDto dto);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<LoanDto>> GetBookLoansAsync(int bookId, int page, int pageSize);
    Task<PagedResult<ReservationDto>> GetBookReservationsAsync(int bookId, int page, int pageSize);
}
