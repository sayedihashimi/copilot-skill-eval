using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatronsController(IPatronService patronService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PatronDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatronDto>> GetById(int id)
    {
        var patron = await patronService.GetByIdAsync(id);
        return patron is null ? NotFound() : Ok(patron);
    }

    [HttpPost]
    public async Task<ActionResult<PatronDto>> Create(CreatePatronDto dto)
    {
        var patron = await patronService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = patron.Id }, patron);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PatronDto>> Update(int id, UpdatePatronDto dto)
    {
        var patron = await patronService.UpdateAsync(id, dto);
        return patron is null ? NotFound() : Ok(patron);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await patronService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/loans")]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetLoans(int id,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetPatronLoansAsync(id, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}/reservations")]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetReservations(int id,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetPatronReservationsAsync(id, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}/fines")]
    public async Task<ActionResult<PagedResult<FineDto>>> GetFines(int id,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetPatronFinesAsync(id, page, pageSize);
        return Ok(result);
    }
}
