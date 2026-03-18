using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/bookings").WithTags("Bookings");

        group.MapPost("/", async (CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
        })
        .WithName("CreateBooking")
        .WithSummary("Book a class")
        .WithDescription("Books a class for a member. Enforces membership requirements, booking windows, weekly limits, premium access, and capacity. Auto-waitlists if class is full.")
        .Produces<BookingResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.GetByIdAsync(id, ct);
            return booking is null ? Results.NotFound() : Results.Ok(booking);
        })
        .WithName("GetBookingById")
        .WithSummary("Get booking details")
        .WithDescription("Returns the details of a specific booking.")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/cancel", async (int id, CancelBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CancelAsync(id, request, ct);
            return Results.Ok(booking);
        })
        .WithName("CancelBooking")
        .WithSummary("Cancel a booking")
        .WithDescription("Cancels a booking. Free cancellation 2+ hours before class. Late cancellation within 2 hours. Promotes first waitlisted member when a confirmed booking is cancelled.")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/check-in", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CheckInAsync(id, ct);
            return Results.Ok(booking);
        })
        .WithName("CheckInBooking")
        .WithSummary("Check in for a class")
        .WithDescription("Checks in a member for their class. Available 15 minutes before to 15 minutes after class start time.")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/no-show", async (int id, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.MarkNoShowAsync(id, ct);
            return Results.Ok(booking);
        })
        .WithName("MarkNoShow")
        .WithSummary("Mark booking as no-show")
        .WithDescription("Marks a confirmed booking as no-show. Only available 15 minutes after class start time.")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
