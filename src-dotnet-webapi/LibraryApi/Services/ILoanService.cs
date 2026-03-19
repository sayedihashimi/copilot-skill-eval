using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PaginatedResponse<LoanResponse>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct);
    Task<LoanResponse?> GetLoanByIdAsync(int id, CancellationToken ct);
    Task<(LoanResponse? Loan, string? Error)> CheckoutBookAsync(CreateLoanRequest request, CancellationToken ct);
    Task<(LoanResponse? Loan, string? Error, bool NotFound)> ReturnBookAsync(int loanId, CancellationToken ct);
    Task<(LoanResponse? Loan, string? Error, bool NotFound)> RenewLoanAsync(int loanId, CancellationToken ct);
    Task<IReadOnlyList<LoanResponse>> GetOverdueLoansAsync(CancellationToken ct);
}
