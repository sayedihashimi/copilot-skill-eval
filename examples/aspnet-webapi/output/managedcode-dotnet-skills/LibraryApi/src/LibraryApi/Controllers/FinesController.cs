using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinesController(IFineService fineService) : ControllerBase
{
    /// <summary>List fines with filter by status and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<FineResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFines([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await fineService.GetFinesAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get fine details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFine(int id)
    {
        var result = await fineService.GetFineByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Pay a fine (set PaidDate, update status to Paid).</summary>
    [HttpPost("{id:int}/pay")]
    [ProducesResponseType(typeof(FineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PayFine(int id)
    {
        var result = await fineService.PayFineAsync(id);
        return Ok(result);
    }

    /// <summary>Waive a fine (update status to Waived).</summary>
    [HttpPost("{id:int}/waive")]
    [ProducesResponseType(typeof(FineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> WaiveFine(int id)
    {
        var result = await fineService.WaiveFineAsync(id);
        return Ok(result);
    }
}
