using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class ReservationEndpoints
{
    public static RouteGroupBuilder MapReservationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reservations").WithTags("Reservations");

        group.MapGet("/", async Task<Ok<PaginatedResponse<ReservationResponse>>> (
            ReservationStatus? status, int? page, int? pageSize,
            IReservationService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(status, page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetReservations")
        .WithSummary("List reservations")
        .WithDescription("Returns a paginated list of reservations. Optionally filter by status.");

        group.MapGet("/{id:int}", async Task<Results<Ok<ReservationResponse>, NotFound>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetReservationById")
        .WithSummary("Get reservation by ID");

        group.MapPost("/", async Task<Created<ReservationResponse>> (
            CreateReservationRequest request, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/reservations/{result.Id}", result);
        })
        .WithName("CreateReservation")
        .WithSummary("Create a reservation")
        .WithDescription("Places a book on hold. Cannot reserve a book already on active loan by the same patron.");

        group.MapPost("/{id:int}/cancel", async Task<Ok<ReservationResponse>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.CancelAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("CancelReservation")
        .WithSummary("Cancel a reservation");

        group.MapPost("/{id:int}/fulfill", async Task<Ok<ReservationResponse>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.FulfillAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("FulfillReservation")
        .WithSummary("Fulfill a reservation")
        .WithDescription("Marks a 'Ready' reservation as fulfilled.");

        return group;
    }
}
