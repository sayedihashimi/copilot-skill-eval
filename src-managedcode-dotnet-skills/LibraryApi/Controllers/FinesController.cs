using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinesController(IFineService fineService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<FineDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await fineService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FineDto>> GetById(int id)
    {
        var fine = await fineService.GetByIdAsync(id);
        return fine is null ? NotFound() : Ok(fine);
    }

    [HttpPost("{id:int}/pay")]
    public async Task<ActionResult<FineDto>> Pay(int id)
    {
        var fine = await fineService.PayAsync(id);
        return Ok(fine);
    }

    [HttpPost("{id:int}/waive")]
    public async Task<ActionResult<FineDto>> Waive(int id)
    {
        var fine = await fineService.WaiveAsync(id);
        return Ok(fine);
    }
}
