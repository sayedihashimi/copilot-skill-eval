using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController(ILoanService loanService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<LoanResponse>>> GetAll(
        [FromQuery] LoanStatus? status,
        [FromQuery] bool? overdue,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await loanService.GetAllAsync(status, overdue, fromDate, toDate, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanDetailResponse>> GetById(int id)
    {
        var loan = await loanService.GetByIdAsync(id);
        return loan is null ? NotFound() : Ok(loan);
    }

    [HttpPost]
    public async Task<ActionResult<LoanResponse>> Checkout(CreateLoanRequest request)
    {
        var (loan, error) = await loanService.CheckoutAsync(request);
        if (loan is null)
            return BadRequest(new ProblemDetails { Title = "Checkout failed", Detail = error, Status = 400 });
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    [HttpPost("{id:int}/return")]
    public async Task<ActionResult<LoanResponse>> Return(int id)
    {
        var (loan, error) = await loanService.ReturnAsync(id);
        if (loan is null)
            return BadRequest(new ProblemDetails { Title = "Return failed", Detail = error, Status = 400 });
        return Ok(loan);
    }

    [HttpPost("{id:int}/renew")]
    public async Task<ActionResult<LoanResponse>> Renew(int id)
    {
        var (loan, error) = await loanService.RenewAsync(id);
        if (loan is null)
            return BadRequest(new ProblemDetails { Title = "Renewal failed", Detail = error, Status = 400 });
        return Ok(loan);
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<List<LoanResponse>>> GetOverdue()
    {
        var loans = await loanService.GetOverdueAsync();
        return Ok(loans);
    }
}
