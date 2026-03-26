using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinesController : ControllerBase
{
    private readonly IFineService _service;

    public FinesController(IFineService service) => _service = service;

    /// <summary>List fines with filter by status and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FineDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get fine details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FineDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Pay a fine (set PaidDate, update status to Paid)</summary>
    [HttpPost("{id}/pay")]
    [ProducesResponseType(typeof(FineDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Pay(int id)
    {
        var result = await _service.PayAsync(id);
        return Ok(result);
    }

    /// <summary>Waive a fine (update status to Waived)</summary>
    [HttpPost("{id}/waive")]
    [ProducesResponseType(typeof(FineDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Waive(int id)
    {
        var result = await _service.WaiveAsync(id);
        return Ok(result);
    }
}
