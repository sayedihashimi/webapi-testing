using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class PatronEndpoints
{
    public static RouteGroupBuilder MapPatronEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/patrons").WithTags("Patrons");

        group.MapGet("/", async (string? search, MembershipType? membershipType, int? page, int? pageSize, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, membershipType, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetPatrons")
        .WithSummary("List patrons")
        .WithDescription("Returns a paginated list of library patrons. Supports searching by name/email and filtering by membership type.")
        .Produces<PagedResponse<PatronResponse>>();

        group.MapGet("/{id:int}", async (int id, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPatronById")
        .WithSummary("Get patron by ID")
        .WithDescription("Returns patron details including active loan count, unpaid fines total, and reservation count.")
        .Produces<PatronDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/patrons/{result.Id}", result);
        })
        .WithName("CreatePatron")
        .WithSummary("Create a new patron")
        .WithDescription("Creates a new library patron. Email must be unique.")
        .Produces<PatronResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async (int id, UpdatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("UpdatePatron")
        .WithSummary("Update a patron")
        .WithDescription("Updates an existing patron's information.")
        .Produces<PatronResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:int}", async (int id, IPatronService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeactivatePatron")
        .WithSummary("Deactivate a patron")
        .WithDescription("Deactivates a patron (soft delete). Fails if the patron has active loans.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/loans", async (int id, LoanStatus? status, int? page, int? pageSize, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetLoansAsync(id, status, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetPatronLoans")
        .WithSummary("Get patron's loans")
        .WithDescription("Returns a patron's loans, optionally filtered by status.")
        .Produces<PagedResponse<LoanResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/reservations", async (int id, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetReservationsAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("GetPatronReservations")
        .WithSummary("Get patron's reservations")
        .WithDescription("Returns a patron's active reservations.")
        .Produces<List<ReservationResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/fines", async (int id, FineStatus? status, int? page, int? pageSize, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetFinesAsync(id, status, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetPatronFines")
        .WithSummary("Get patron's fines")
        .WithDescription("Returns a patron's fines, optionally filtered by status.")
        .Produces<PagedResponse<FineResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
