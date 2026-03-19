using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapPost("/", async (CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
        })
        .WithName("CreateBooking")
        .WithSummary("Book a class (enforces all business rules)");

        group.MapGet("/{id:int}", async Task<Results<Ok<BookingResponse>, NotFound>> (
            int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.GetByIdAsync(id, ct);
            return booking is null ? TypedResults.NotFound() : TypedResults.Ok(booking);
        })
        .WithName("GetBookingById")
        .WithSummary("Get booking details");

        group.MapPost("/{id:int}/cancel", async (int id, CancelBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("CancelBooking")
        .WithSummary("Cancel a booking (promotes waitlisted member if applicable)");

        group.MapPost("/{id:int}/check-in", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CheckInAsync(id, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("CheckInBooking")
        .WithSummary("Check in to a class (15 min window around class start)");

        group.MapPost("/{id:int}/no-show", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.MarkNoShowAsync(id, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("MarkNoShow")
        .WithSummary("Mark a confirmed booking as no-show");
    }
}
