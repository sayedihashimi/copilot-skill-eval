using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoansController(ILoanService loanService, IValidator<CreateLoanRequest> createValidator) : ControllerBase
{
    /// <summary>List loans with filter by status, overdue flag, date range, and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<LoanResponse>), StatusCodes.Status200OK)]
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
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoan(int id)
    {
        var result = await loanService.GetLoanByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Check out a book — create a loan enforcing all checkout rules.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckoutBook([FromBody] CreateLoanRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await loanService.CheckoutBookAsync(request);
        return CreatedAtAction(nameof(GetLoan), new { id = result.Id }, result);
    }

    /// <summary>Return a book — enforce all return processing rules.</summary>
    [HttpPost("{id:int}/return")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReturnBook(int id)
    {
        var result = await loanService.ReturnBookAsync(id);
        return Ok(result);
    }

    /// <summary>Renew a loan — enforce all renewal rules.</summary>
    [HttpPost("{id:int}/renew")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RenewLoan(int id)
    {
        var result = await loanService.RenewLoanAsync(id);
        return Ok(result);
    }

    /// <summary>Get all currently overdue loans.</summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(PaginatedResponse<LoanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdueLoans([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await loanService.GetOverdueLoansAsync(page, pageSize);
        return Ok(result);
    }
}
