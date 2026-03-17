using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FinesController(IFineService fineService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<FineDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await fineService.GetAllAsync(status, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FineDto>> GetById(int id, CancellationToken ct = default)
    {
        var fine = await fineService.GetByIdAsync(id, ct);
        return fine is not null ? Ok(fine) : NotFound();
    }

    [HttpPost("{id:int}/pay")]
    public async Task<ActionResult<FineDto>> Pay(int id, CancellationToken ct = default)
    {
        var (fine, error) = await fineService.PayAsync(id, ct);
        if (error is not null)
        {
            if (error.Contains("not found")) return NotFound();
            return BadRequest(new ProblemDetails { Title = "Payment Failed", Detail = error });
        }
        return Ok(fine);
    }

    [HttpPost("{id:int}/waive")]
    public async Task<ActionResult<FineDto>> Waive(int id, CancellationToken ct = default)
    {
        var (fine, error) = await fineService.WaiveAsync(id, ct);
        if (error is not null)
        {
            if (error.Contains("not found")) return NotFound();
            return BadRequest(new ProblemDetails { Title = "Waive Failed", Detail = error });
        }
        return Ok(fine);
    }
}
