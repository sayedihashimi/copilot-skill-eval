using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinesController(IFineService fineService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<FineResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List fines")]
    [EndpointDescription("Returns a paginated list of fines, optionally filtered by status.")]
    public async Task<ActionResult<PagedResponse<FineResponse>>> GetAll(
        [FromQuery] FineStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await fineService.GetAllAsync(status, page, pageSize, cancellationToken));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<FineResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get fine by ID")]
    [EndpointDescription("Returns details of a specific fine.")]
    public async Task<ActionResult<FineResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var fine = await fineService.GetByIdAsync(id, cancellationToken);
        return fine is null ? NotFound() : Ok(fine);
    }

    [HttpPost("{id}/pay")]
    [ProducesResponseType<FineResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Pay a fine")]
    [EndpointDescription("Marks an unpaid fine as paid.")]
    public async Task<ActionResult<FineResponse>> Pay(int id, CancellationToken cancellationToken)
    {
        return Ok(await fineService.PayAsync(id, cancellationToken));
    }

    [HttpPost("{id}/waive")]
    [ProducesResponseType<FineResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Waive a fine")]
    [EndpointDescription("Waives an unpaid fine.")]
    public async Task<ActionResult<FineResponse>> Waive(int id, CancellationToken cancellationToken)
    {
        return Ok(await fineService.WaiveAsync(id, cancellationToken));
    }
}
