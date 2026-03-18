using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class FineEndpoints
{
    public static RouteGroupBuilder MapFineEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/fines").WithTags("Fines");

        group.MapGet("/", async (FineStatus? status, int? page, int? pageSize, IFineService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(status, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetFines")
        .WithSummary("List fines")
        .WithDescription("Returns a paginated list of fines. Supports filtering by status.")
        .Produces<PagedResponse<FineResponse>>();

        group.MapGet("/{id:int}", async (int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetFineById")
        .WithSummary("Get fine by ID")
        .WithDescription("Returns fine details including patron and loan information.")
        .Produces<FineResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/pay", async (int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.PayAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("PayFine")
        .WithSummary("Pay a fine")
        .WithDescription("Marks an unpaid fine as paid.")
        .Produces<FineResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/waive", async (int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.WaiveAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("WaiveFine")
        .WithSummary("Waive a fine")
        .WithDescription("Waives an unpaid fine.")
        .Produces<FineResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
