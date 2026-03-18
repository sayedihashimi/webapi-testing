using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class ReservationEndpoints
{
    public static RouteGroupBuilder MapReservationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reservations").WithTags("Reservations");

        group.MapGet("/", async (ReservationStatus? status, int? page, int? pageSize, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(status, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetReservations")
        .WithSummary("List reservations")
        .WithDescription("Returns a paginated list of reservations. Supports filtering by status.")
        .Produces<PagedResponse<ReservationResponse>>();

        group.MapGet("/{id:int}", async (int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetReservationById")
        .WithSummary("Get reservation by ID")
        .WithDescription("Returns reservation details.")
        .Produces<ReservationResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateReservationRequest request, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/reservations/{result.Id}", result);
        })
        .WithName("CreateReservation")
        .WithSummary("Create a reservation")
        .WithDescription("Creates a new reservation in the queue. Cannot reserve a book already on active loan to same patron.")
        .Produces<ReservationResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/cancel", async (int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.CancelAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("CancelReservation")
        .WithSummary("Cancel a reservation")
        .WithDescription("Cancels a pending or ready reservation. If ready, releases the held copy.")
        .Produces<ReservationResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/fulfill", async (int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.FulfillAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("FulfillReservation")
        .WithSummary("Fulfill a ready reservation")
        .WithDescription("Fulfills a reservation in 'Ready' status, creating a loan for the patron.")
        .Produces<ReservationResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
