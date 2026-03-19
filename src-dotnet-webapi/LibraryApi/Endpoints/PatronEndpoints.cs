using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class PatronEndpoints
{
    public static void MapPatronEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patrons").WithTags("Patrons");

        group.MapGet("/", async Task<Ok<PaginatedResponse<PatronResponse>>> (
            string? search, MembershipType? membershipType, bool? isActive,
            int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(search, membershipType, isActive, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatrons")
        .WithSummary("List patrons")
        .WithDescription("Returns a paginated list of patrons with optional search and filtering.")
        .Produces<PaginatedResponse<PatronResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<PatronDetailResponse>, NotFound>> (
            int id, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetPatronById")
        .WithSummary("Get patron by ID")
        .WithDescription("Returns patron details including active loans count and unpaid fines total.")
        .Produces<PatronDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<PatronResponse>> (
            CreatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/patrons/{result.Id}", result);
        })
        .WithName("CreatePatron")
        .WithSummary("Create a patron")
        .WithDescription("Creates a new library patron.")
        .Produces<PatronResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<PatronResponse>, NotFound>> (
            int id, UpdatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("UpdatePatron")
        .WithSummary("Update a patron")
        .WithDescription("Updates an existing patron by ID.")
        .Produces<PatronResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IPatronService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeletePatron")
        .WithSummary("Deactivate a patron")
        .WithDescription("Deactivates a patron. Fails if they have active loans.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/loans", async Task<Ok<PaginatedResponse<LoanResponse>>> (
            int id, LoanStatus? status, int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetLoansAsync(id, status, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatronLoans")
        .WithSummary("Get patron's loans")
        .WithDescription("Returns a patron's loans, optionally filtered by status.")
        .Produces<PaginatedResponse<LoanResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/reservations", async Task<Ok<IReadOnlyList<ReservationResponse>>> (
            int id, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetReservationsAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatronReservations")
        .WithSummary("Get patron's reservations")
        .WithDescription("Returns a patron's active reservations.")
        .Produces<IReadOnlyList<ReservationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/fines", async Task<Ok<PaginatedResponse<FineResponse>>> (
            int id, FineStatus? status, int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetFinesAsync(id, status, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatronFines")
        .WithSummary("Get patron's fines")
        .WithDescription("Returns a patron's fines, optionally filtered by status.")
        .Produces<PaginatedResponse<FineResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
