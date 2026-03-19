using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Endpoints;

public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapPost("/", async (CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
        })
        .WithName("CreateBooking")
        .WithSummary("Book a class");

        group.MapGet("/{id:int}", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.GetByIdAsync(id, ct);
            return booking is not null ? Results.Ok(booking) : Results.NotFound();
        })
        .WithName("GetBooking")
        .WithSummary("Get booking details");

        group.MapPost("/{id:int}/cancel", async (int id, CancelBookingRequest? request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("CancelBooking")
        .WithSummary("Cancel a booking");

        group.MapPost("/{id:int}/check-in", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CheckInAsync(id, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("CheckInBooking")
        .WithSummary("Check in to a class");

        group.MapPost("/{id:int}/no-show", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.MarkNoShowAsync(id, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("MarkNoShow")
        .WithSummary("Mark booking as no-show");

        return group;
    }
}
