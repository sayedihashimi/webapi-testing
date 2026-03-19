using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class ReservationEndpoints
{
    public static RouteGroupBuilder MapReservationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/reservations")
            .WithTags("Reservations");

        group.MapGet("/", GetReservationsAsync);
        group.MapGet("/{id:int}", GetReservationByIdAsync);
        group.MapPost("/", CreateReservationAsync);
        group.MapPost("/{id:int}/cancel", CancelReservationAsync);
        group.MapPost("/{id:int}/fulfill", FulfillReservationAsync);

        return group;
    }

    private static async Task<IResult> GetReservationsAsync(
        IReservationService service,
        string? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetReservationsAsync(status, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetReservationByIdAsync(
        int id,
        IReservationService service,
        CancellationToken ct = default)
    {
        var reservation = await service.GetReservationByIdAsync(id, ct);
        return reservation is not null
            ? TypedResults.Ok(reservation)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateReservationAsync(
        CreateReservationDto dto,
        IReservationService service,
        CancellationToken ct = default)
    {
        var (reservation, error) = await service.CreateReservationAsync(dto, ct);

        if (error is not null)
        {
            return TypedResults.Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Created($"/api/reservations/{reservation!.Id}", reservation);
    }

    private static async Task<IResult> CancelReservationAsync(
        int id,
        IReservationService service,
        CancellationToken ct = default)
    {
        var (reservation, error) = await service.CancelReservationAsync(id, ct);

        if (error is not null)
        {
            return TypedResults.Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(reservation);
    }

    private static async Task<IResult> FulfillReservationAsync(
        int id,
        IReservationService service,
        CancellationToken ct = default)
    {
        var (loan, error) = await service.FulfillReservationAsync(id, ct);

        if (error is not null)
        {
            return TypedResults.Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Created($"/api/loans/{loan!.Id}", loan);
    }
}
