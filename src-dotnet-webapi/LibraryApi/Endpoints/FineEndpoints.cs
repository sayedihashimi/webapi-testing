using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class FineEndpoints
{
    public static RouteGroupBuilder MapFineEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/fines").WithTags("Fines");

        group.MapGet("/", async Task<Ok<PaginatedResponse<FineResponse>>> (
            FineStatus? status, int? page, int? pageSize,
            IFineService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(status, page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetFines")
        .WithSummary("List fines")
        .WithDescription("Returns a paginated list of fines. Optionally filter by status.");

        group.MapGet("/{id:int}", async Task<Results<Ok<FineResponse>, NotFound>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetFineById")
        .WithSummary("Get fine by ID");

        group.MapPost("/{id:int}/pay", async Task<Ok<FineResponse>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.PayAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("PayFine")
        .WithSummary("Pay a fine");

        group.MapPost("/{id:int}/waive", async Task<Ok<FineResponse>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.WaiveAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("WaiveFine")
        .WithSummary("Waive a fine");

        return group;
    }
}
