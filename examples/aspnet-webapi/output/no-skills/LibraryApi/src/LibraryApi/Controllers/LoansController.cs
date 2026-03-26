using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _service;

    public LoansController(ILoanService service) => _service = service;

    /// <summary>List loans with filter by status, overdue flag, date range; pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] bool? overdue, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(status, overdue, fromDate, toDate, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get loan details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LoanDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Check out a book — create a loan enforcing all checkout rules</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Checkout([FromBody] LoanCreateDto dto)
    {
        var result = await _service.CheckoutAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Return a book — enforce all return processing rules</summary>
    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(LoanDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Return(int id)
    {
        var result = await _service.ReturnAsync(id);
        return Ok(result);
    }

    /// <summary>Renew a loan — enforce all renewal rules</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(typeof(LoanDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Renew(int id)
    {
        var result = await _service.RenewAsync(id);
        return Ok(result);
    }

    /// <summary>Get all currently overdue loans</summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), 200)]
    public async Task<IActionResult> GetOverdue([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetOverdueAsync(page, pageSize);
        return Ok(result);
    }
}
