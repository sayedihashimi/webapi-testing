using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class FineEndpoints
{
    public static RouteGroupBuilder MapFineEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/fines").WithTags("Fines");

        group.MapGet("/", async (FineStatus? status, int? page, int? pageSize, IFineService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(status, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetFines")
        .WithSummary("List fines")
        .WithDescription("Returns a paginated list of fines. Optionally filter by status.")
        .Produces<PaginatedResponse<FineResponse>>();

        group.MapGet("/{id:int}", async (int id, IFineService service, CancellationToken ct) =>
        {
            var fine = await service.GetByIdAsync(id, ct);
            return fine is null ? Results.NotFound() : Results.Ok(fine);
        })
        .WithName("GetFineById")
        .WithSummary("Get fine by ID")
        .WithDescription("Returns the details of a specific fine.")
        .Produces<FineResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/pay", async (int id, IFineService service, CancellationToken ct) =>
        {
            var fine = await service.PayAsync(id, ct);
            return Results.Ok(fine);
        })
        .WithName("PayFine")
        .WithSummary("Pay a fine")
        .WithDescription("Marks an unpaid fine as paid.")
        .Produces<FineResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/waive", async (int id, IFineService service, CancellationToken ct) =>
        {
            var fine = await service.WaiveAsync(id, ct);
            return Results.Ok(fine);
        })
        .WithName("WaiveFine")
        .WithSummary("Waive a fine")
        .WithDescription("Waives an unpaid fine.")
        .Produces<FineResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        return group;
    }
}
