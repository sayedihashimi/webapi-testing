using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class FineEndpoints
{
    public static void MapFineEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/fines").WithTags("Fines");

        group.MapGet("/", async Task<Ok<PaginatedResponse<FineResponse>>> (
            FineStatus? status, int? page, int? pageSize,
            IFineService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(status, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetFines")
        .WithSummary("List fines")
        .WithDescription("Returns a paginated list of fines, optionally filtered by status.")
        .Produces<PaginatedResponse<FineResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<FineResponse>, NotFound>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetFineById")
        .WithSummary("Get fine by ID")
        .WithDescription("Returns fine details.")
        .Produces<FineResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/pay", async Task<Ok<FineResponse>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.PayAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("PayFine")
        .WithSummary("Pay a fine")
        .WithDescription("Marks an unpaid fine as paid.")
        .Produces<FineResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/waive", async Task<Ok<FineResponse>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.WaiveAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("WaiveFine")
        .WithSummary("Waive a fine")
        .WithDescription("Waives an unpaid fine.")
        .Produces<FineResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
