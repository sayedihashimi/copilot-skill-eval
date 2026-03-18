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
    [ProducesResponseType<PagedResponse<LoanResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List loans")]
    [EndpointDescription("Returns a paginated list of loans with optional status, overdue, and date range filters.")]
    public async Task<ActionResult<PagedResponse<LoanResponse>>> GetAll(
        [FromQuery] LoanStatus? status,
        [FromQuery] bool? overdue,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await loanService.GetAllAsync(status, overdue, fromDate, toDate, page, pageSize, cancellationToken));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<LoanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get loan by ID")]
    [EndpointDescription("Returns details of a specific loan.")]
    public async Task<ActionResult<LoanResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var loan = await loanService.GetByIdAsync(id, cancellationToken);
        return loan is null ? NotFound() : Ok(loan);
    }

    [HttpPost]
    [ProducesResponseType<LoanResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Checkout a book")]
    [EndpointDescription("Creates a new loan (checkout). Enforces availability, borrowing limits, fine thresholds, and active membership.")]
    public async Task<ActionResult<LoanResponse>> Checkout(CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var loan = await loanService.CheckoutAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    [HttpPost("{id}/return")]
    [ProducesResponseType<ReturnLoanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Return a book")]
    [EndpointDescription("Processes a book return. Auto-generates fines for overdue books and promotes pending reservations.")]
    public async Task<ActionResult<ReturnLoanResponse>> Return(int id, CancellationToken cancellationToken)
    {
        return Ok(await loanService.ReturnAsync(id, cancellationToken));
    }

    [HttpPost("{id}/renew")]
    [ProducesResponseType<LoanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Renew a loan")]
    [EndpointDescription("Renews an active loan. Max 2 renewals. Cannot renew if overdue, has pending reservations, or patron has $10+ unpaid fines.")]
    public async Task<ActionResult<LoanResponse>> Renew(int id, CancellationToken cancellationToken)
    {
        return Ok(await loanService.RenewAsync(id, cancellationToken));
    }

    [HttpGet("overdue")]
    [ProducesResponseType<List<LoanResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("Get overdue loans")]
    [EndpointDescription("Returns all currently overdue loans and updates their status.")]
    public async Task<ActionResult<List<LoanResponse>>> GetOverdue(CancellationToken cancellationToken)
    {
        return Ok(await loanService.GetOverdueAsync(cancellationToken));
    }
}
