using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this RouteGroupBuilder group)
    {
        var bookings = group.MapGroup("/bookings").WithTags("Bookings");

        bookings.MapPost("/", CreateAsync)
            .WithSummary("Book a class (enforces all booking rules)");

        bookings.MapGet("/{id:int}", GetByIdAsync)
            .WithSummary("Get booking details");

        bookings.MapPost("/{id:int}/cancel", CancelAsync)
            .WithSummary("Cancel a booking (enforces cancellation policy)");

        bookings.MapPost("/{id:int}/check-in", CheckInAsync)
            .WithSummary("Check in for a class");

        bookings.MapPost("/{id:int}/no-show", NoShowAsync)
            .WithSummary("Mark booking as no-show");

        return group;
    }

    private static async Task<Results<Created<BookingResponse>, BadRequest<string>>> CreateAsync(
        CreateBookingRequest request, IBookingService service, CancellationToken ct)
    {
        var (result, error) = await service.CreateAsync(request, ct);
        return result is not null
            ? TypedResults.Created($"/api/bookings/{result.Id}", result)
            : TypedResults.BadRequest(error);
    }

    private static async Task<Results<Ok<BookingResponse>, NotFound>> GetByIdAsync(
        int id, IBookingService service, CancellationToken ct)
    {
        var booking = await service.GetByIdAsync(id, ct);
        return booking is not null
            ? TypedResults.Ok(booking)
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, BadRequest<string>>> CancelAsync(
        int id, CancelBookingRequest? request, IBookingService service, CancellationToken ct)
    {
        var (success, error) = await service.CancelAsync(id, request, ct);
        return success
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(error);
    }

    private static async Task<Results<NoContent, BadRequest<string>>> CheckInAsync(
        int id, IBookingService service, CancellationToken ct)
    {
        var (success, error) = await service.CheckInAsync(id, ct);
        return success
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(error);
    }

    private static async Task<Results<NoContent, BadRequest<string>>> NoShowAsync(
        int id, IBookingService service, CancellationToken ct)
    {
        var (success, error) = await service.MarkNoShowAsync(id, ct);
        return success
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(error);
    }
}
