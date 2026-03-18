using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class PatronEndpoints
{
    public static RouteGroupBuilder MapPatronEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/patrons").WithTags("Patrons");

        group.MapGet("/", async (string? search, MembershipType? membershipType, int? page, int? pageSize, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, membershipType, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetPatrons")
        .WithSummary("List patrons")
        .WithDescription("Returns a paginated list of patrons. Filter by name/email and membership type.")
        .Produces<PaginatedResponse<PatronResponse>>();

        group.MapGet("/{id:int}", async (int id, IPatronService service, CancellationToken ct) =>
        {
            var patron = await service.GetByIdAsync(id, ct);
            return patron is null ? Results.NotFound() : Results.Ok(patron);
        })
        .WithName("GetPatronById")
        .WithSummary("Get patron by ID")
        .WithDescription("Returns patron details including active loan count and unpaid fines balance.")
        .Produces<PatronDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var patron = await service.CreateAsync(request, ct);
            return Results.Created($"/api/patrons/{patron.Id}", patron);
        })
        .WithName("CreatePatron")
        .WithSummary("Create a patron")
        .WithDescription("Creates a new library patron. Email must be unique.")
        .Produces<PatronResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var patron = await service.UpdateAsync(id, request, ct);
            return patron is null ? Results.NotFound() : Results.Ok(patron);
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
        .WithDescription("Deactivates a patron account. Fails if the patron has active loans.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/loans", async (int id, LoanStatus? status, int? page, int? pageSize, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetPatronLoansAsync(id, status, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetPatronLoans")
        .WithSummary("Get patron loans")
        .WithDescription("Returns the loans for a specific patron. Optionally filter by loan status.")
        .Produces<PaginatedResponse<LoanResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/reservations", async (int id, int? page, int? pageSize, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetPatronReservationsAsync(id, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetPatronReservations")
        .WithSummary("Get patron reservations")
        .WithDescription("Returns the reservations for a specific patron.")
        .Produces<PaginatedResponse<ReservationResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/fines", async (int id, FineStatus? status, int? page, int? pageSize, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetPatronFinesAsync(id, status, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetPatronFines")
        .WithSummary("Get patron fines")
        .WithDescription("Returns the fines for a specific patron. Optionally filter by fine status.")
        .Produces<PaginatedResponse<FineResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
