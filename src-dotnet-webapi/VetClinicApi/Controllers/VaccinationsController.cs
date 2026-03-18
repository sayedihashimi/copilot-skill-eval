using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VaccinationsController(IVaccinationService vaccinationService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<VaccinationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Record a vaccination")]
    [EndpointDescription("Records a new vaccination for a pet. Expiration date must be after date administered.")]
    public async Task<ActionResult<VaccinationResponse>> Create(
        [FromBody] CreateVaccinationRequest request,
        CancellationToken cancellationToken)
    {
        var vaccination = await vaccinationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = vaccination.Id }, vaccination);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<VaccinationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get vaccination by ID")]
    [EndpointDescription("Returns vaccination details including computed expired and due-soon status.")]
    public async Task<ActionResult<VaccinationResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var vaccination = await vaccinationService.GetByIdAsync(id, cancellationToken);
        return vaccination is null ? NotFound() : Ok(vaccination);
    }
}
