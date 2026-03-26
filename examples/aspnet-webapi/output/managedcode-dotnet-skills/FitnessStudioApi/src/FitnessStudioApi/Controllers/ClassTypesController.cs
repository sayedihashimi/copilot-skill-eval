using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/class-types")]
[Produces("application/json")]
public class ClassTypesController(IClassTypeService service) : ControllerBase
{
    /// <summary>List class types with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClassTypeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? difficulty,
        [FromQuery] bool? isPremium,
        CancellationToken ct)
    {
        var types = await service.GetAllAsync(difficulty, isPremium, ct);
        return Ok(types);
    }

    /// <summary>Get class type details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ClassTypeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var classType = await service.GetByIdAsync(id, ct);
        return classType is null ? NotFound() : Ok(classType);
    }

    /// <summary>Create a new class type.</summary>
    [HttpPost]
    [ProducesResponseType<ClassTypeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClassTypeDto dto, CancellationToken ct)
    {
        var classType = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = classType.Id }, classType);
    }

    /// <summary>Update an existing class type.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<ClassTypeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassTypeDto dto, CancellationToken ct)
    {
        var classType = await service.UpdateAsync(id, dto, ct);
        return Ok(classType);
    }
}
