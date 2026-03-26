using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservationsController(IReservationService reservationService, IValidator<CreateReservationRequest> createValidator) : ControllerBase
{
    /// <summary>List reservations with filter by status and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ReservationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservations([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await reservationService.GetReservationsAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get reservation details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservation(int id)
    {
        var result = await reservationService.GetReservationByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a reservation enforcing all reservation rules.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await reservationService.CreateReservationAsync(request);
        return CreatedAtAction(nameof(GetReservation), new { id = result.Id }, result);
    }

    /// <summary>Cancel a reservation.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var result = await reservationService.CancelReservationAsync(id);
        return Ok(result);
    }

    /// <summary>Fulfill a "Ready" reservation (creates a loan for the patron).</summary>
    [HttpPost("{id:int}/fulfill")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> FulfillReservation(int id)
    {
        var result = await reservationService.FulfillReservationAsync(id);
        return Ok(result);
    }
}
