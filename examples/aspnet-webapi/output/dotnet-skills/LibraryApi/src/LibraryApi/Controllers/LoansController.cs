using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoansController(ILoanService loanService) : ControllerBase
{
    /// <summary>List loans with filter by status, overdue flag, date range, and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LoanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoans(
        [FromQuery] string? status,
        [FromQuery] bool? overdue,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await loanService.GetLoansAsync(status, overdue, fromDate, toDate, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get loan details.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoan(int id)
    {
        var loan = await loanService.GetLoanByIdAsync(id);
        return loan is null ? NotFound() : Ok(loan);
    }

    /// <summary>Check out a book — creates a loan enforcing all checkout rules.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckoutBook([FromBody] CreateLoanRequest request)
    {
        var loan = await loanService.CheckoutBookAsync(request);
        return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
    }

    /// <summary>Return a book — enforces all return processing rules.</summary>
    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReturnBook(int id)
    {
        var loan = await loanService.ReturnBookAsync(id);
        return Ok(loan);
    }

    /// <summary>Renew a loan — enforces all renewal rules.</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RenewLoan(int id)
    {
        var loan = await loanService.RenewLoanAsync(id);
        return Ok(loan);
    }

    /// <summary>Get all currently overdue loans (also flags active overdue loans).</summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(List<LoanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdueLoans()
    {
        var loans = await loanService.GetOverdueLoansAsync();
        return Ok(loans);
    }
}
