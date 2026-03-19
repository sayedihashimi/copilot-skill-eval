using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController(ILoanService loanService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await loanService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanDto>> GetById(int id)
    {
        var loan = await loanService.GetByIdAsync(id);
        return loan is null ? NotFound() : Ok(loan);
    }

    [HttpPost]
    public async Task<ActionResult<LoanDto>> Checkout(CreateLoanDto dto)
    {
        var loan = await loanService.CheckoutAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    [HttpPost("{id:int}/return")]
    public async Task<ActionResult<LoanDto>> Return(int id)
    {
        var loan = await loanService.ReturnAsync(id);
        return Ok(loan);
    }

    [HttpPost("{id:int}/renew")]
    public async Task<ActionResult<LoanDto>> Renew(int id)
    {
        var loan = await loanService.RenewAsync(id);
        return Ok(loan);
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetOverdue(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await loanService.GetOverdueAsync(page, pageSize);
        return Ok(result);
    }
}
