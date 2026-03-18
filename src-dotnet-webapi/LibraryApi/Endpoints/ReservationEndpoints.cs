using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class ReservationEndpoints
{
    public static RouteGroupBuilder MapReservationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/reservations").WithTags("Reservations");

        group.MapGet("/", async (ReservationStatus? status, int? page, int? pageSize, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(status, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetReservations")
        .WithSummary("List reservations")
        .WithDescription("Returns a paginated list of reservations. Optionally filter by status.")
        .Produces<PaginatedResponse<ReservationResponse>>();

        group.MapGet("/{id:int}", async (int id, IReservationService service, CancellationToken ct) =>
        {
            var reservation = await service.GetByIdAsync(id, ct);
            return reservation is null ? Results.NotFound() : Results.Ok(reservation);
        })
        .WithName("GetReservationById")
        .WithSummary("Get reservation by ID")
        .WithDescription("Returns the details of a specific reservation.")
        .Produces<ReservationResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateReservationRequest request, IReservationService service, CancellationToken ct) =>
        {
            var reservation = await service.CreateAsync(request, ct);
            return Results.Created($"/api/reservations/{reservation.Id}", reservation);
        })
        .WithName("CreateReservation")
        .WithSummary("Create a reservation")
        .WithDescription("Creates a reservation for a book. Cannot reserve a book already on active loan by the same patron.")
        .Produces<ReservationResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/cancel", async (int id, IReservationService service, CancellationToken ct) =>
        {
            var reservation = await service.CancelAsync(id, ct);
            return Results.Ok(reservation);
        })
        .WithName("CancelReservation")
        .WithSummary("Cancel a reservation")
        .WithDescription("Cancels a pending or ready reservation.")
        .Produces<ReservationResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/fulfill", async (int id, IReservationService service, CancellationToken ct) =>
        {
            var loan = await service.FulfillAsync(id, ct);
            return Results.Created($"/api/loans/{loan.Id}", loan);
        })
        .WithName("FulfillReservation")
        .WithSummary("Fulfill a reservation")
        .WithDescription("Fulfills a Ready reservation by creating a loan for the patron.")
        .Produces<LoanResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        return group;
    }
}
