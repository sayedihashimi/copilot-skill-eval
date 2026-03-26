using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapPost("/", async (CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
        }).WithSummary("Book a class (enforces all booking rules)");

        group.MapGet("/{id:int}", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.GetByIdAsync(id, ct);
            return booking is not null ? Results.Ok(booking) : Results.NotFound();
        }).WithSummary("Get booking details");

        group.MapPost("/{id:int}/cancel", async (int id, CancelBookingRequest request, IBookingService service, CancellationToken ct) =>
            TypedResults.Ok(await service.CancelAsync(id, request, ct)))
            .WithSummary("Cancel a booking (promotes from waitlist)");

        group.MapPost("/{id:int}/check-in", async (int id, IBookingService service, CancellationToken ct) =>
            TypedResults.Ok(await service.CheckInAsync(id, ct)))
            .WithSummary("Check in for a class");

        group.MapPost("/{id:int}/no-show", async (int id, IBookingService service, CancellationToken ct) =>
            TypedResults.Ok(await service.MarkNoShowAsync(id, ct)))
            .WithSummary("Mark booking as no-show");

        return group;
    }
}
