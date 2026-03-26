using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PaginatedResponse<LoanResponse>> GetAllAsync(
        string? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct);
    Task<LoanResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<LoanResponse> CheckoutAsync(CreateLoanRequest request, CancellationToken ct);
    Task<LoanResponse> ReturnAsync(int id, CancellationToken ct);
    Task<LoanResponse> RenewAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetOverdueAsync(int page, int pageSize, CancellationToken ct);
}
