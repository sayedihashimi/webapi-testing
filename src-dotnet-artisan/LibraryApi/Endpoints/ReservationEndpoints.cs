using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Endpoints;

public static class ReservationEndpoints
{
    public static RouteGroupBuilder MapReservationEndpoints(this RouteGroupBuilder group)
    {
        var reservations = group.MapGroup("/reservations").WithTags("Reservations");

        reservations.MapGet("/", GetReservationsAsync)
            .WithSummary("List reservations with status filter and pagination");

        reservations.MapGet("/{id:int}", GetReservationByIdAsync)
            .WithSummary("Get reservation details");

        reservations.MapPost("/", CreateReservationAsync)
            .WithSummary("Create a reservation enforcing all reservation rules");

        reservations.MapPost("/{id:int}/cancel", CancelReservationAsync)
            .WithSummary("Cancel a reservation");

        reservations.MapPost("/{id:int}/fulfill", FulfillReservationAsync)
            .WithSummary("Fulfill a Ready reservation — creates a loan for the patron");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<ReservationResponse>>> GetReservationsAsync(
        IReservationService service, string? status, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetReservationsAsync(status, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ReservationDetailResponse>, NotFound>> GetReservationByIdAsync(
        int id, IReservationService service, CancellationToken ct = default)
    {
        var result = await service.GetReservationByIdAsync(id, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Results<Created<ReservationDetailResponse>, NotFound, BadRequest<ProblemDetails>, Conflict<ProblemDetails>>> CreateReservationAsync(
        CreateReservationRequest request, IReservationService service, CancellationToken ct = default)
    {
        var result = await service.CreateReservationAsync(request, ct);
        if (result.IsSuccess)
        {
            return TypedResults.Created($"/api/reservations/{result.Value!.Id}", result.Value);
        }

        if (result.StatusCode == 404)
        {
            return TypedResults.NotFound();
        }

        if (result.StatusCode == 409)
        {
            return TypedResults.Conflict(new ProblemDetails
            {
                Title = "Reservation conflict",
                Detail = result.Error,
                Status = 409
            });
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Reservation denied",
            Detail = result.Error,
            Status = 400
        });
    }

    private static async Task<Results<Ok<ReservationDetailResponse>, NotFound, BadRequest<ProblemDetails>>> CancelReservationAsync(
        int id, IReservationService service, CancellationToken ct = default)
    {
        var result = await service.CancelReservationAsync(id, ct);
        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value!);
        }

        if (result.StatusCode == 404)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Cancel failed",
            Detail = result.Error,
            Status = 400
        });
    }

    private static async Task<Results<Created<LoanDetailResponse>, NotFound, BadRequest<ProblemDetails>>> FulfillReservationAsync(
        int id, IReservationService service, CancellationToken ct = default)
    {
        var result = await service.FulfillReservationAsync(id, ct);
        if (result.IsSuccess)
        {
            return TypedResults.Created($"/api/loans/{result.Value!.Id}", result.Value);
        }

        if (result.StatusCode == 404)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Fulfill failed",
            Detail = result.Error,
            Status = 400
        });
    }
}
