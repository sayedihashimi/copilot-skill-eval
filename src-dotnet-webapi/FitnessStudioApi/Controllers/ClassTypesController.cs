using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/class-types")]
public class ClassTypesController(IClassTypeService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<ClassTypeResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List class types")]
    [EndpointDescription("Returns all active class types with optional difficulty and premium filters.")]
    public async Task<ActionResult<List<ClassTypeResponse>>> GetAll(
        [FromQuery] DifficultyLevel? difficulty,
        [FromQuery] bool? isPremium,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(difficulty, isPremium, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<ClassTypeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get class type details")]
    [EndpointDescription("Returns full details of a specific class type.")]
    public async Task<ActionResult<ClassTypeResponse>> GetById(int id, CancellationToken ct)
    {
        var classType = await service.GetByIdAsync(id, ct);
        return classType is null ? NotFound() : Ok(classType);
    }

    [HttpPost]
    [ProducesResponseType<ClassTypeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new class type")]
    [EndpointDescription("Creates a new class type offered by the studio.")]
    public async Task<ActionResult<ClassTypeResponse>> Create(
        CreateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = classType.Id }, classType);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<ClassTypeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a class type")]
    [EndpointDescription("Updates an existing class type's details.")]
    public async Task<ActionResult<ClassTypeResponse>> Update(
        int id, UpdateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await service.UpdateAsync(id, request, ct);
        return Ok(classType);
    }
}
