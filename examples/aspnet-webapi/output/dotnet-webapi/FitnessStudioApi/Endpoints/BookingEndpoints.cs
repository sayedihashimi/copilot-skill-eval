using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapPost("/", async (CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
        })
        .WithName("CreateBooking")
        .WithSummary("Book a class")
        .WithDescription("Book a class for a member. Enforces all booking rules: capacity, membership tier, weekly limit, overlap, and booking window.")
        .Produces<BookingResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}", async Task<Results<Ok<BookingResponse>, NotFound>> (
            int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.GetByIdAsync(id, ct);
            return booking is null ? TypedResults.NotFound() : TypedResults.Ok(booking);
        })
        .WithName("GetBookingById")
        .WithSummary("Get booking details")
        .WithDescription("Returns the full details of a specific booking.")
        .Produces<BookingResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/cancel", async (int id, CancelBookingRequest request,
            IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("CancelBooking")
        .WithSummary("Cancel a booking")
        .WithDescription("Cancel a booking. If confirmed, the first waitlisted member is automatically promoted. Late cancellations (< 2 hours before class) are noted.")
        .Produces<BookingResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/check-in", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CheckInAsync(id, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("CheckInBooking")
        .WithSummary("Check in for a class")
        .WithDescription("Check in for a class. Available from 15 minutes before to 15 minutes after class start time.")
        .Produces<BookingResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/no-show", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.MarkNoShowAsync(id, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("MarkNoShow")
        .WithSummary("Mark booking as no-show")
        .WithDescription("Mark a confirmed booking as no-show. Only available 15 minutes after class start time.")
        .Produces<BookingResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
