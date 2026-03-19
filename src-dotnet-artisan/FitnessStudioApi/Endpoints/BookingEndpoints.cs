using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapPost("/", CreateAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/{id:int}/cancel", CancelAsync);
        group.MapPost("/{id:int}/check-in", CheckInAsync);
        group.MapPost("/{id:int}/no-show", NoShowAsync);

        return group;
    }

    private static async Task<IResult> CreateAsync(
        CreateBookingRequest request, IBookingService service, CancellationToken ct)
    {
        try
        {
            var booking = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetByIdAsync(
        int id, IBookingService service, CancellationToken ct)
    {
        var booking = await service.GetByIdAsync(id, ct);
        return booking is not null
            ? TypedResults.Ok(booking)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CancelAsync(
        int id, CancelBookingRequest request, IBookingService service, CancellationToken ct)
    {
        try
        {
            var booking = await service.CancelAsync(id, request.Reason, ct);
            return TypedResults.Ok(booking);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CheckInAsync(
        int id, IBookingService service, CancellationToken ct)
    {
        try
        {
            var booking = await service.CheckInAsync(id, ct);
            return TypedResults.Ok(booking);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> NoShowAsync(
        int id, IBookingService service, CancellationToken ct)
    {
        try
        {
            var booking = await service.MarkNoShowAsync(id, ct);
            return TypedResults.Ok(booking);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }
}
