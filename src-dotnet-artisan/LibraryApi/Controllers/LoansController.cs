using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LoansController(ILoanService loanService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<LoanDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await loanService.GetAllAsync(status, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanDto>> GetById(int id, CancellationToken ct = default)
    {
        var loan = await loanService.GetByIdAsync(id, ct);
        return loan is not null ? Ok(loan) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<LoanDto>> Checkout([FromBody] CreateLoanRequest request, CancellationToken ct = default)
    {
        var (loan, error) = await loanService.CheckoutAsync(request, ct);
        if (error is not null)
            return BadRequest(new ProblemDetails { Title = "Checkout Failed", Detail = error });
        return CreatedAtAction(nameof(GetById), new { id = loan!.Id }, loan);
    }

    [HttpPost("{id:int}/return")]
    public async Task<ActionResult<LoanDto>> Return(int id, CancellationToken ct = default)
    {
        var (loan, error) = await loanService.ReturnAsync(id, ct);
        if (error is not null)
        {
            if (error.Contains("not found")) return NotFound();
            return BadRequest(new ProblemDetails { Title = "Return Failed", Detail = error });
        }
        return Ok(loan);
    }

    [HttpPost("{id:int}/renew")]
    public async Task<ActionResult<LoanDto>> Renew(int id, CancellationToken ct = default)
    {
        var (loan, error) = await loanService.RenewAsync(id, ct);
        if (error is not null)
        {
            if (error.Contains("not found")) return NotFound();
            return BadRequest(new ProblemDetails { Title = "Renewal Failed", Detail = error });
        }
        return Ok(loan);
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<IReadOnlyList<LoanDto>>> GetOverdue(CancellationToken ct = default)
    {
        var result = await loanService.GetOverdueAsync(ct);
        return Ok(result);
    }
}
