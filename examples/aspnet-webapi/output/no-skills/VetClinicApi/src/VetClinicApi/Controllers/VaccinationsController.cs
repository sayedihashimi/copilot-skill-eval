using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VaccinationsController : ControllerBase
{
    private readonly IVaccinationService _service;

    public VaccinationsController(IVaccinationService service)
    {
        _service = service;
    }

    /// <summary>Record a new vaccination</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VaccinationResponseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] VaccinationCreateDto dto)
    {
        var vaccination = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = vaccination.Id }, vaccination);
    }

    /// <summary>Get vaccination details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VaccinationResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var vaccination = await _service.GetByIdAsync(id);
        return vaccination == null ? NotFound() : Ok(vaccination);
    }
}
