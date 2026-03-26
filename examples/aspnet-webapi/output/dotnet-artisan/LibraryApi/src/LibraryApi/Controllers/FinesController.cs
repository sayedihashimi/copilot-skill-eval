using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class FinesController(IFineService fineService) : ControllerBase
{
    /// <summary>List fines with filter by status and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<FineResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFines([FromQuery] FineStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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
        var fine = await fineService.GetFineByIdAsync(id);
        return fine is not null ? Ok(fine) : NotFound();
    }

    /// <summary>Pay a fine (set PaidDate, update status to Paid).</summary>
    [HttpPost("{id:int}/pay")]
    [ProducesResponseType(typeof(FineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PayFine(int id)
    {
        var fine = await fineService.PayFineAsync(id);
        return Ok(fine);
    }

    /// <summary>Waive a fine (update status to Waived).</summary>
    [HttpPost("{id:int}/waive")]
    [ProducesResponseType(typeof(FineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> WaiveFine(int id)
    {
        var fine = await fineService.WaiveFineAsync(id);
        return Ok(fine);
    }
}
