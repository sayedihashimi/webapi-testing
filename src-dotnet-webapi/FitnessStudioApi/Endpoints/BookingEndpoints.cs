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
            var booking = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
        })
        .WithName("CreateBooking")
        .WithSummary("Book a class")
        .WithDescription("Books a class for a member. Enforces membership, capacity, premium access, weekly limits, and scheduling rules.")
        .Produces<BookingResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}", async Task<Results<Ok<BookingResponse>, NotFound>> (
            int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.GetByIdAsync(id, ct);
            return booking is null ? TypedResults.NotFound() : TypedResults.Ok(booking);
        })
        .WithName("GetBookingById")
        .WithSummary("Get a booking by ID")
        .WithDescription("Returns the full details of a specific booking.")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/cancel", async Task<Results<Ok<BookingResponse>, NotFound>> (
            int id, CancelBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("CancelBooking")
        .WithSummary("Cancel a booking")
        .WithDescription("Cancels a booking. Promotes the first waitlisted member if the cancelled booking was confirmed.")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/check-in", async Task<Results<Ok<BookingResponse>, NotFound>> (
            int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CheckInAsync(id, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("CheckInBooking")
        .WithSummary("Check in to a class")
        .WithDescription("Checks a member into a class. Must be within 15 minutes before or after class start time.")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/no-show", async Task<Results<Ok<BookingResponse>, NotFound>> (
            int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.MarkNoShowAsync(id, ct);
            return TypedResults.Ok(booking);
        })
        .WithName("MarkNoShow")
        .WithSummary("Mark booking as no-show")
        .WithDescription("Marks a confirmed booking as a no-show. Can only be done 15 minutes after class start.")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
