using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/class-types")]
[Produces("application/json")]
public class ClassTypesController : ControllerBase
{
    private readonly IClassTypeService _service;

    public ClassTypesController(IClassTypeService service) => _service = service;

    /// <summary>List class types with filters</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ClassTypeDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? difficulty = null, [FromQuery] bool? isPremium = null)
        => Ok(await _service.GetAllAsync(page, pageSize, difficulty, isPremium));

    /// <summary>Get class type details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassTypeDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var ct = await _service.GetByIdAsync(id);
        return ct == null ? NotFound() : Ok(ct);
    }

    /// <summary>Create a new class type</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassTypeDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateClassTypeDto dto)
    {
        var ct = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = ct.Id }, ct);
    }

    /// <summary>Update a class type</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassTypeDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassTypeDto dto)
        => Ok(await _service.UpdateAsync(id, dto));
}
