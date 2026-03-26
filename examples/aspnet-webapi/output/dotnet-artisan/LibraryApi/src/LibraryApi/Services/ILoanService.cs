using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PaginatedResponse<LoanResponse>> GetLoansAsync(LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<LoanResponse?> GetLoanByIdAsync(int id);
    Task<LoanResponse> CheckoutBookAsync(CreateLoanRequest request);
    Task<LoanResponse> ReturnBookAsync(int loanId);
    Task<LoanResponse> RenewLoanAsync(int loanId);
    Task<PaginatedResponse<LoanResponse>> GetOverdueLoansAsync(int page, int pageSize);
}
