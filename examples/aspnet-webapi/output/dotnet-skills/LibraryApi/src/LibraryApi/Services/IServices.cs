using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PagedResult<AuthorResponse>> GetAuthorsAsync(string? search, int page, int pageSize);
    Task<AuthorDetailResponse?> GetAuthorByIdAsync(int id);
    Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request);
    Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request);
    Task<bool> DeleteAuthorAsync(int id);
}

public interface ICategoryService
{
    Task<PagedResult<CategoryResponse>> GetCategoriesAsync(int page, int pageSize);
    Task<CategoryDetailResponse?> GetCategoryByIdAsync(int id);
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(int id);
}

public interface IBookService
{
    Task<PagedResult<BookListResponse>> GetBooksAsync(string? search, bool? available, string? sortBy, int page, int pageSize);
    Task<BookResponse?> GetBookByIdAsync(int id);
    Task<BookResponse> CreateBookAsync(CreateBookRequest request);
    Task<BookResponse?> UpdateBookAsync(int id, UpdateBookRequest request);
    Task<bool> DeleteBookAsync(int id);
    Task<PagedResult<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize);
    Task<PagedResult<ReservationResponse>> GetBookReservationsAsync(int bookId, int page, int pageSize);
}

public interface IPatronService
{
    Task<PagedResult<PatronResponse>> GetPatronsAsync(string? search, string? membershipType, int page, int pageSize);
    Task<PatronDetailResponse?> GetPatronByIdAsync(int id);
    Task<PatronResponse> CreatePatronAsync(CreatePatronRequest request);
    Task<PatronResponse?> UpdatePatronAsync(int id, UpdatePatronRequest request);
    Task<bool> DeactivatePatronAsync(int id);
    Task<PagedResult<LoanResponse>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize);
    Task<PagedResult<ReservationResponse>> GetPatronReservationsAsync(int patronId, int page, int pageSize);
    Task<PagedResult<FineResponse>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize);
}

public interface ILoanService
{
    Task<PagedResult<LoanResponse>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<LoanResponse?> GetLoanByIdAsync(int id);
    Task<LoanResponse> CheckoutBookAsync(CreateLoanRequest request);
    Task<LoanResponse> ReturnBookAsync(int loanId);
    Task<LoanResponse> RenewLoanAsync(int loanId);
    Task<List<LoanResponse>> GetOverdueLoansAsync();
}

public interface IReservationService
{
    Task<PagedResult<ReservationResponse>> GetReservationsAsync(string? status, int page, int pageSize);
    Task<ReservationResponse?> GetReservationByIdAsync(int id);
    Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request);
    Task<ReservationResponse> CancelReservationAsync(int id);
    Task<LoanResponse> FulfillReservationAsync(int id);
}

public interface IFineService
{
    Task<PagedResult<FineResponse>> GetFinesAsync(string? status, int page, int pageSize);
    Task<FineResponse?> GetFineByIdAsync(int id);
    Task<FineResponse> PayFineAsync(int id);
    Task<FineResponse> WaiveFineAsync(int id);
}
