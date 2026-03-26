using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OwnersController : ControllerBase
{
    private readonly IOwnerService _service;

    public OwnersController(IOwnerService service)
    {
        _service = service;
    }

    /// <summary>List all owners with optional search and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OwnerSummaryDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(search, new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get owner by ID including their pets</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OwnerResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var owner = await _service.GetByIdAsync(id);
        return owner == null ? NotFound() : Ok(owner);
    }

    /// <summary>Create a new owner</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OwnerResponseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] OwnerCreateDto dto)
    {
        var owner = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = owner.Id }, owner);
    }

    /// <summary>Update an existing owner</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OwnerResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] OwnerUpdateDto dto)
    {
        var owner = await _service.UpdateAsync(id, dto);
        return owner == null ? NotFound() : Ok(owner);
    }

    /// <summary>Delete owner (fails if owner has active pets)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }

    /// <summary>Get all pets for an owner</summary>
    [HttpGet("{id}/pets")]
    [ProducesResponseType(typeof(List<PetResponseDto>), 200)]
    public async Task<IActionResult> GetPets(int id)
    {
        var pets = await _service.GetPetsAsync(id);
        return Ok(pets);
    }

    /// <summary>Get appointment history for all of an owner's pets</summary>
    [HttpGet("{id}/appointments")]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetAppointments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAppointmentsAsync(id, new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }
}
