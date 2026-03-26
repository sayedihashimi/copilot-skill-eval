using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PaginatedResponse<AuthorResponse>> GetAuthorsAsync(string? search, int page, int pageSize);
    Task<AuthorResponse?> GetAuthorByIdAsync(int id);
    Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request);
    Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request);
    Task<bool> DeleteAuthorAsync(int id);
}

public interface ICategoryService
{
    Task<PaginatedResponse<CategoryResponse>> GetCategoriesAsync(int page, int pageSize);
    Task<CategoryResponse?> GetCategoryByIdAsync(int id);
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(int id);
}

public interface IBookService
{
    Task<PaginatedResponse<BookResponse>> GetBooksAsync(string? search, string? category, bool? available, string? sortBy, string? sortOrder, int page, int pageSize);
    Task<BookResponse?> GetBookByIdAsync(int id);
    Task<BookResponse> CreateBookAsync(CreateBookRequest request);
    Task<BookResponse?> UpdateBookAsync(int id, UpdateBookRequest request);
    Task<bool> DeleteBookAsync(int id);
    Task<PaginatedResponse<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize);
    Task<PaginatedResponse<ReservationResponse>> GetBookReservationsAsync(int bookId, int page, int pageSize);
}

public interface IPatronService
{
    Task<PaginatedResponse<PatronResponse>> GetPatronsAsync(string? search, string? membershipType, int page, int pageSize);
    Task<PatronResponse?> GetPatronByIdAsync(int id);
    Task<PatronResponse> CreatePatronAsync(CreatePatronRequest request);
    Task<PatronResponse?> UpdatePatronAsync(int id, UpdatePatronRequest request);
    Task<bool> DeactivatePatronAsync(int id);
    Task<PaginatedResponse<LoanResponse>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize);
    Task<PaginatedResponse<ReservationResponse>> GetPatronReservationsAsync(int patronId, int page, int pageSize);
    Task<PaginatedResponse<FineResponse>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize);
}

public interface ILoanService
{
    Task<PaginatedResponse<LoanResponse>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<LoanResponse?> GetLoanByIdAsync(int id);
    Task<LoanResponse> CheckoutBookAsync(CreateLoanRequest request);
    Task<LoanResponse> ReturnBookAsync(int id);
    Task<LoanResponse> RenewLoanAsync(int id);
    Task<PaginatedResponse<LoanResponse>> GetOverdueLoansAsync(int page, int pageSize);
}

public interface IReservationService
{
    Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(string? status, int page, int pageSize);
    Task<ReservationResponse?> GetReservationByIdAsync(int id);
    Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request);
    Task<ReservationResponse> CancelReservationAsync(int id);
    Task<LoanResponse> FulfillReservationAsync(int id);
}

public interface IFineService
{
    Task<PaginatedResponse<FineResponse>> GetFinesAsync(string? status, int page, int pageSize);
    Task<FineResponse?> GetFineByIdAsync(int id);
    Task<FineResponse> PayFineAsync(int id);
    Task<FineResponse> WaiveFineAsync(int id);
}
