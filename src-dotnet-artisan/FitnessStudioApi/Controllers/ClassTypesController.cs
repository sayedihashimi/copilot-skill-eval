using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/class-types")]
public sealed class ClassTypesController(IClassTypeService service) : ControllerBase
{
    private readonly IClassTypeService _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClassTypeResponse>>> GetAll(
        [FromQuery] DifficultyLevel? difficulty,
        [FromQuery] bool? isPremium,
        CancellationToken ct)
    {
        var classTypes = await _service.GetAllAsync(difficulty, isPremium, ct);
        return Ok(classTypes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClassTypeResponse>> GetById(int id, CancellationToken ct)
    {
        var classType = await _service.GetByIdAsync(id, ct);
        return Ok(classType);
    }

    [HttpPost]
    public async Task<ActionResult<ClassTypeResponse>> Create(CreateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = classType.Id }, classType);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClassTypeResponse>> Update(int id, UpdateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await _service.UpdateAsync(id, request, ct);
        return Ok(classType);
    }
}
