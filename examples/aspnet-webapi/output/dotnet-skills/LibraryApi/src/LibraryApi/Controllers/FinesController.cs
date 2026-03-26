using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinesController(IFineService fineService) : ControllerBase
{
    /// <summary>List fines with filter by status and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FineResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFines([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await fineService.GetFinesAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get fine details.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFine(int id)
    {
        var fine = await fineService.GetFineByIdAsync(id);
        return fine is null ? NotFound() : Ok(fine);
    }

    /// <summary>Pay a fine (set PaidDate, update status to Paid).</summary>
    [HttpPost("{id}/pay")]
    [ProducesResponseType(typeof(FineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PayFine(int id)
    {
        var fine = await fineService.PayFineAsync(id);
        return Ok(fine);
    }

    /// <summary>Waive a fine (update status to Waived).</summary>
    [HttpPost("{id}/waive")]
    [ProducesResponseType(typeof(FineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> WaiveFine(int id)
    {
        var fine = await fineService.WaiveFineAsync(id);
        return Ok(fine);
    }
}
