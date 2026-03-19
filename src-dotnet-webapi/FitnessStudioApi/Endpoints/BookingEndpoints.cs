using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings").WithTags("Bookings");

        group.MapPost("/", async Task<Created<BookingResponse>> (
            CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{result.Id}", result);
        })
        .WithName("CreateBooking")
        .WithSummary("Book a class")
        .WithDescription("Books a class for a member. Enforces all business rules: capacity management, membership requirements, tier access, weekly limits, booking window, and overlap checks.")
        .Produces<BookingResponse>(201);

        group.MapGet("/{id:int}", async Task<Results<Ok<BookingResponse>, NotFound>> (
            int id, IBookingService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetBooking")
        .WithSummary("Get booking details")
        .WithDescription("Returns details of a specific booking by ID.")
        .Produces<BookingResponse>(200)
        .Produces(404);

        group.MapPost("/{id:int}/cancel", async Task<Ok<BookingResponse>> (
            int id, CancelBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var result = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(result);
        })
        .WithName("CancelBooking")
        .WithSummary("Cancel a booking")
        .WithDescription("Cancels a booking. If a confirmed booking is cancelled, the first person on the waitlist is promoted. Late cancellation (less than 2 hours before class) is noted.")
        .Produces<BookingResponse>(200);

        group.MapPost("/{id:int}/check-in", async Task<Ok<BookingResponse>> (
            int id, IBookingService service, CancellationToken ct) =>
        {
            var result = await service.CheckInAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("CheckInBooking")
        .WithSummary("Check in for a class")
        .WithDescription("Checks in a member for a class. Available 15 minutes before to 15 minutes after class start time.")
        .Produces<BookingResponse>(200);

        group.MapPost("/{id:int}/no-show", async Task<Ok<BookingResponse>> (
            int id, IBookingService service, CancellationToken ct) =>
        {
            var result = await service.MarkNoShowAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("MarkNoShow")
        .WithSummary("Mark booking as no-show")
        .WithDescription("Marks a confirmed booking as no-show. Only available 15 minutes after class start time.")
        .Produces<BookingResponse>(200);
    }
}
