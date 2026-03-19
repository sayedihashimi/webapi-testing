using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class ReservationEndpoints
{
    public static void MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reservations").WithTags("Reservations");

        group.MapGet("/", async Task<Ok<PaginatedResponse<ReservationResponse>>> (
            ReservationStatus? status, int? page, int? pageSize,
            IReservationService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(status, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetReservations")
        .WithSummary("List reservations")
        .WithDescription("Returns a paginated list of reservations, optionally filtered by status.")
        .Produces<PaginatedResponse<ReservationResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<ReservationResponse>, NotFound>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetReservationById")
        .WithSummary("Get reservation by ID")
        .WithDescription("Returns reservation details.")
        .Produces<ReservationResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<ReservationResponse>> (
            CreateReservationRequest request, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/reservations/{result.Id}", result);
        })
        .WithName("CreateReservation")
        .WithSummary("Create a reservation")
        .WithDescription("Creates a new reservation. Cannot reserve a book you currently have on loan.")
        .Produces<ReservationResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/cancel", async Task<Ok<ReservationResponse>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.CancelAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("CancelReservation")
        .WithSummary("Cancel a reservation")
        .WithDescription("Cancels a pending or ready reservation.")
        .Produces<ReservationResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/fulfill", async Task<Ok<LoanResponse>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.FulfillAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("FulfillReservation")
        .WithSummary("Fulfill a reservation")
        .WithDescription("Fulfills a 'Ready' reservation by creating a loan.")
        .Produces<LoanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
