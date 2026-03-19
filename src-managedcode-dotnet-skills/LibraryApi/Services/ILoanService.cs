using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PagedResult<LoanDto>> GetAllAsync(int page, int pageSize);
    Task<LoanDto?> GetByIdAsync(int id);
    Task<LoanDto> CheckoutAsync(CreateLoanDto dto);
    Task<LoanDto> ReturnAsync(int id);
    Task<LoanDto> RenewAsync(int id);
    Task<PagedResult<LoanDto>> GetOverdueAsync(int page, int pageSize);
}
