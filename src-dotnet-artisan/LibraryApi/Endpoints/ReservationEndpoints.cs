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

        group.MapGet("/", GetReservations).WithName("GetReservations");
        group.MapGet("/{id:int}", GetReservation).WithName("GetReservation");
        group.MapPost("/", CreateReservation).WithName("CreateReservation");
        group.MapPost("/{id:int}/cancel", CancelReservation).WithName("CancelReservation");
        group.MapPost("/{id:int}/fulfill", FulfillReservation).WithName("FulfillReservation");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<ReservationResponse>>> GetReservations(
        LibraryService service, ReservationStatus? status = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetReservationsAsync(status, page, pageSize);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ReservationResponse>, NotFound>> GetReservation(
        LibraryService service, int id)
    {
        var result = await service.GetReservationByIdAsync(id);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<ReservationResponse>, BadRequest<string>>> CreateReservation(
        LibraryService service, CreateReservationRequest request)
    {
        var (result, error) = await service.CreateReservationAsync(request);
        return result is not null
            ? TypedResults.Created($"/api/reservations/{result.Id}", result)
            : TypedResults.BadRequest(error!);
    }

    private static async Task<Results<Ok<ReservationResponse>, NotFound, BadRequest<string>>> CancelReservation(
        LibraryService service, int id)
    {
        var (result, error) = await service.CancelReservationAsync(id);
        if (error == "Reservation not found") { return TypedResults.NotFound(); }
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(error!);
    }

    private static async Task<Results<Created<LoanResponse>, NotFound, BadRequest<string>>> FulfillReservation(
        LibraryService service, int id)
    {
        var (result, error) = await service.FulfillReservationAsync(id);
        if (error == "Reservation not found") { return TypedResults.NotFound(); }
        return result is not null
            ? TypedResults.Created($"/api/loans/{result.Id}", result)
            : TypedResults.BadRequest(error!);
    }
}
