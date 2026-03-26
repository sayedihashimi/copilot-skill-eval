using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PagedResult<AuthorDto>> GetAllAsync(string? search, int page, int pageSize);
    Task<AuthorDetailDto?> GetByIdAsync(int id);
    Task<AuthorDto> CreateAsync(AuthorCreateDto dto);
    Task<AuthorDto?> UpdateAsync(int id, AuthorUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDetailDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CategoryCreateDto dto);
    Task<CategoryDto?> UpdateAsync(int id, CategoryUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IBookService
{
    Task<PagedResult<BookDto>> GetAllAsync(string? search, bool? available, string? sortBy, string? sortDir, int page, int pageSize);
    Task<BookDto?> GetByIdAsync(int id);
    Task<BookDto> CreateAsync(BookCreateDto dto);
    Task<BookDto?> UpdateAsync(int id, BookUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<LoanDto>> GetBookLoansAsync(int bookId, int page, int pageSize);
    Task<List<ReservationDto>> GetBookReservationsAsync(int bookId);
}

public interface IPatronService
{
    Task<PagedResult<PatronDto>> GetAllAsync(string? search, string? membershipType, int page, int pageSize);
    Task<PatronDetailDto?> GetByIdAsync(int id);
    Task<PatronDto> CreateAsync(PatronCreateDto dto);
    Task<PatronDto?> UpdateAsync(int id, PatronUpdateDto dto);
    Task<bool> DeactivateAsync(int id);
    Task<PagedResult<LoanDto>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize);
    Task<PagedResult<ReservationDto>> GetPatronReservationsAsync(int patronId, int page, int pageSize);
    Task<PagedResult<FineDto>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize);
}

public interface ILoanService
{
    Task<PagedResult<LoanDto>> GetAllAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<LoanDto?> GetByIdAsync(int id);
    Task<LoanDto> CheckoutAsync(LoanCreateDto dto);
    Task<LoanDto> ReturnAsync(int id);
    Task<LoanDto> RenewAsync(int id);
    Task<PagedResult<LoanDto>> GetOverdueAsync(int page, int pageSize);
}

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetAllAsync(string? status, int page, int pageSize);
    Task<ReservationDto?> GetByIdAsync(int id);
    Task<ReservationDto> CreateAsync(ReservationCreateDto dto);
    Task<ReservationDto> CancelAsync(int id);
    Task<LoanDto> FulfillAsync(int id);
}

public interface IFineService
{
    Task<PagedResult<FineDto>> GetAllAsync(string? status, int page, int pageSize);
    Task<FineDto?> GetByIdAsync(int id);
    Task<FineDto> PayAsync(int id);
    Task<FineDto> WaiveAsync(int id);
}
