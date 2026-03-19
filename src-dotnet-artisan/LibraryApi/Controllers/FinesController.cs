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
    public async Task<ActionResult<PagedResult<FineResponse>>> GetAll(
        [FromQuery] FineStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await fineService.GetAllAsync(status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FineResponse>> GetById(int id)
    {
        var fine = await fineService.GetByIdAsync(id);
        return fine is null ? NotFound() : Ok(fine);
    }

    [HttpPost("{id:int}/pay")]
    public async Task<ActionResult<FineResponse>> Pay(int id)
    {
        var (fine, error) = await fineService.PayAsync(id);
        if (fine is null)
            return BadRequest(new ProblemDetails { Title = "Payment failed", Detail = error, Status = 400 });
        return Ok(fine);
    }

    [HttpPost("{id:int}/waive")]
    public async Task<ActionResult<FineResponse>> Waive(int id)
    {
        var (fine, error) = await fineService.WaiveAsync(id);
        if (fine is null)
            return BadRequest(new ProblemDetails { Title = "Waive failed", Detail = error, Status = 400 });
        return Ok(fine);
    }
}
