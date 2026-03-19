using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/class-types")]
[Produces("application/json")]
public class ClassTypesController(IClassTypeService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClassTypeResponse>>(200)]
    public async Task<IActionResult> GetAll([FromQuery] string? difficulty, [FromQuery] bool? isPremium)
        => Ok(await service.GetAllAsync(difficulty, isPremium));

    [HttpGet("{id:int}")]
    [ProducesResponseType<ClassTypeResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<ClassTypeResponse>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(CreateClassTypeRequest request)
    {
        var result = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<ClassTypeResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, UpdateClassTypeRequest request)
        => Ok(await service.UpdateAsync(id, request));
}
