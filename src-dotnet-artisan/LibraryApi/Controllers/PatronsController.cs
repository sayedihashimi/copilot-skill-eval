using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatronsController(IPatronService patronService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PatronResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] MembershipType? membershipType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetAllAsync(search, membershipType, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatronDetailResponse>> GetById(int id)
    {
        var patron = await patronService.GetByIdAsync(id);
        return patron is null ? NotFound() : Ok(patron);
    }

    [HttpPost]
    public async Task<ActionResult<PatronResponse>> Create(CreatePatronRequest request)
    {
        var patron = await patronService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = patron.Id }, patron);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PatronResponse>> Update(int id, UpdatePatronRequest request)
    {
        var patron = await patronService.UpdateAsync(id, request);
        return patron is null ? NotFound() : Ok(patron);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await patronService.DeleteAsync(id);
        if (!success)
        {
            return error == "Patron not found."
                ? NotFound(new ProblemDetails { Title = error, Status = 404 })
                : Conflict(new ProblemDetails { Title = error, Status = 409 });
        }
        return NoContent();
    }

    [HttpGet("{id:int}/loans")]
    public async Task<ActionResult<List<LoanResponse>>> GetPatronLoans(int id, [FromQuery] LoanStatus? status)
    {
        var loans = await patronService.GetPatronLoansAsync(id, status);
        return Ok(loans);
    }

    [HttpGet("{id:int}/reservations")]
    public async Task<ActionResult<List<ReservationResponse>>> GetPatronReservations(int id)
    {
        var reservations = await patronService.GetPatronReservationsAsync(id);
        return Ok(reservations);
    }

    [HttpGet("{id:int}/fines")]
    public async Task<ActionResult<List<FineResponse>>> GetPatronFines(int id, [FromQuery] FineStatus? status)
    {
        var fines = await patronService.GetPatronFinesAsync(id, status);
        return Ok(fines);
    }
}
