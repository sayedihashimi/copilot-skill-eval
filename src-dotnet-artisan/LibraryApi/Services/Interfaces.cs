using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PaginatedResponse<AuthorDto>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<AuthorDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<AuthorDto> CreateAsync(CreateAuthorRequest request, CancellationToken ct);
    Task<AuthorDto?> UpdateAsync(int id, UpdateAuthorRequest request, CancellationToken ct);
    Task<(bool Found, bool HasBooks)> DeleteAsync(int id, CancellationToken ct);
}

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct);
    Task<CategoryDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct);
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct);
    Task<(bool Found, bool HasBooks)> DeleteAsync(int id, CancellationToken ct);
}

public interface IBookService
{
    Task<PaginatedResponse<BookSummaryDto>> GetAllAsync(string? search, string? category, bool? available, string? sortBy, string? sortDir, int page, int pageSize, CancellationToken ct);
    Task<BookDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<BookDetailDto> CreateAsync(CreateBookRequest request, CancellationToken ct);
    Task<BookDetailDto?> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct);
    Task<(bool Found, bool HasActiveLoans)> DeleteAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanDto>> GetLoansAsync(int bookId, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(int bookId, CancellationToken ct);
}

public interface IPatronService
{
    Task<PaginatedResponse<PatronDto>> GetAllAsync(string? search, string? membershipType, int page, int pageSize, CancellationToken ct);
    Task<PatronDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PatronDto> CreateAsync(CreatePatronRequest request, CancellationToken ct);
    Task<PatronDto?> UpdateAsync(int id, UpdatePatronRequest request, CancellationToken ct);
    Task<(bool Found, bool HasActiveLoans)> DeactivateAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanDto>> GetLoansAsync(int patronId, string? status, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(int patronId, CancellationToken ct);
    Task<IReadOnlyList<FineDto>> GetFinesAsync(int patronId, string? status, CancellationToken ct);
}

public interface ILoanService
{
    Task<PaginatedResponse<LoanDto>> GetAllAsync(string? status, int page, int pageSize, CancellationToken ct);
    Task<LoanDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(LoanDto? Loan, string? Error)> CheckoutAsync(CreateLoanRequest request, CancellationToken ct);
    Task<(LoanDto? Loan, string? Error)> ReturnAsync(int id, CancellationToken ct);
    Task<(LoanDto? Loan, string? Error)> RenewAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<LoanDto>> GetOverdueAsync(CancellationToken ct);
}

public interface IReservationService
{
    Task<PaginatedResponse<ReservationDto>> GetAllAsync(string? status, int page, int pageSize, CancellationToken ct);
    Task<ReservationDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(ReservationDto? Reservation, string? Error)> CreateAsync(CreateReservationRequest request, CancellationToken ct);
    Task<(ReservationDto? Reservation, string? Error)> CancelAsync(int id, CancellationToken ct);
    Task<(LoanDto? Loan, string? Error)> FulfillAsync(int id, CancellationToken ct);
}

public interface IFineService
{
    Task<PaginatedResponse<FineDto>> GetAllAsync(string? status, int page, int pageSize, CancellationToken ct);
    Task<FineDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(FineDto? Fine, string? Error)> PayAsync(int id, CancellationToken ct);
    Task<(FineDto? Fine, string? Error)> WaiveAsync(int id, CancellationToken ct);
}
