using FitnessStudioApi.DTOs.ClassType;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/class-types")]
[Produces("application/json")]
public sealed class ClassTypesController : ControllerBase
{
    private readonly IClassTypeService _service;

    public ClassTypesController(IClassTypeService service)
    {
        _service = service;
    }

    /// <summary>List class types with optional filters</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClassTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? difficulty,
        [FromQuery] bool? isPremium)
    {
        var types = await _service.GetAllAsync(difficulty, isPremium);
        return Ok(types);
    }

    /// <summary>Get class type details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var classType = await _service.GetByIdAsync(id);
        return Ok(classType);
    }

    /// <summary>Create a new class type</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateClassTypeDto dto)
    {
        var classType = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = classType.Id }, classType);
    }

    /// <summary>Update a class type</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassTypeDto dto)
    {
        var classType = await _service.UpdateAsync(id, dto);
        return Ok(classType);
    }
}
