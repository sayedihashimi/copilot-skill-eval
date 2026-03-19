using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PagedResult<AuthorSummaryDto>> GetAuthorsAsync(string? search, int page, int pageSize);
    Task<AuthorDto?> GetAuthorByIdAsync(int id);
    Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto dto);
    Task<AuthorDto?> UpdateAuthorAsync(int id, UpdateAuthorDto dto);
    Task<(bool Success, string? Error)> DeleteAuthorAsync(int id);
}

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetCategoriesAsync(int page, int pageSize);
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task<(bool Success, string? Error)> DeleteCategoryAsync(int id);
}

public interface IBookService
{
    Task<PagedResult<BookDto>> GetBooksAsync(string? search, string? category, bool? available, string? sortBy, int page, int pageSize);
    Task<BookDto?> GetBookByIdAsync(int id);
    Task<BookDto> CreateBookAsync(CreateBookDto dto);
    Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto dto);
    Task<(bool Success, string? Error)> DeleteBookAsync(int id);
    Task<PagedResult<LoanDto>> GetBookLoansAsync(int bookId, int page, int pageSize);
    Task<PagedResult<ReservationDto>> GetBookReservationsAsync(int bookId, int page, int pageSize);
}

public interface IPatronService
{
    Task<PagedResult<PatronDto>> GetPatronsAsync(string? search, string? membershipType, int page, int pageSize);
    Task<PatronDto?> GetPatronByIdAsync(int id);
    Task<PatronDto> CreatePatronAsync(CreatePatronDto dto);
    Task<PatronDto?> UpdatePatronAsync(int id, UpdatePatronDto dto);
    Task<(bool Success, string? Error)> DeactivatePatronAsync(int id);
    Task<PagedResult<LoanDto>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize);
    Task<PagedResult<ReservationDto>> GetPatronReservationsAsync(int patronId, int page, int pageSize);
    Task<PagedResult<FineDto>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize);
}

public interface ILoanService
{
    Task<PagedResult<LoanDto>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<LoanDto?> GetLoanByIdAsync(int id);
    Task<(LoanDto? Loan, string? Error)> CheckoutBookAsync(CreateLoanDto dto);
    Task<(LoanDto? Loan, string? Error)> ReturnBookAsync(int loanId);
    Task<(LoanDto? Loan, string? Error)> RenewLoanAsync(int loanId);
    Task<PagedResult<LoanDto>> GetOverdueLoansAsync(int page, int pageSize);
}

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetReservationsAsync(string? status, int page, int pageSize);
    Task<ReservationDto?> GetReservationByIdAsync(int id);
    Task<(ReservationDto? Reservation, string? Error)> CreateReservationAsync(CreateReservationDto dto);
    Task<(ReservationDto? Reservation, string? Error)> CancelReservationAsync(int id);
    Task<(LoanDto? Loan, string? Error)> FulfillReservationAsync(int id);
}

public interface IFineService
{
    Task<PagedResult<FineDto>> GetFinesAsync(string? status, int page, int pageSize);
    Task<FineDto?> GetFineByIdAsync(int id);
    Task<(FineDto? Fine, string? Error)> PayFineAsync(int id);
    Task<(FineDto? Fine, string? Error)> WaiveFineAsync(int id);
}
