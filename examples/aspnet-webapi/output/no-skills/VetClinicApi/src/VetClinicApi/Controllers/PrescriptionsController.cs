using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _service;

    public PrescriptionsController(IPrescriptionService service)
    {
        _service = service;
    }

    /// <summary>Get prescription details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var prescription = await _service.GetByIdAsync(id);
        return prescription == null ? NotFound() : Ok(prescription);
    }

    /// <summary>Create prescription for a medical record</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] PrescriptionCreateDto dto)
    {
        var prescription = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
    }
}
