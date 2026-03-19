using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class PatronEndpoints
{
    public static RouteGroupBuilder MapPatronEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/patrons").WithTags("Patrons");

        group.MapGet("/", async Task<Ok<PaginatedResponse<PatronResponse>>> (
            string? search, MembershipType? membershipType,
            int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(
                search, membershipType, page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatrons")
        .WithSummary("List patrons")
        .WithDescription("Returns a paginated list of patrons. Filter by name/email search or membership type.");

        group.MapGet("/{id:int}", async Task<Results<Ok<PatronDetailResponse>, NotFound>> (
            int id, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetPatronById")
        .WithSummary("Get patron by ID")
        .WithDescription("Returns patron details with active loan count and unpaid fines total.");

        group.MapPost("/", async Task<Created<PatronResponse>> (
            CreatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/patrons/{result.Id}", result);
        })
        .WithName("CreatePatron")
        .WithSummary("Create a new patron");

        group.MapPut("/{id:int}", async Task<Results<Ok<PatronResponse>, NotFound>> (
            int id, UpdatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(result);
        })
        .WithName("UpdatePatron")
        .WithSummary("Update a patron");

        group.MapDelete("/{id:int}", async Task<NoContent> (
            int id, IPatronService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeletePatron")
        .WithSummary("Deactivate a patron")
        .WithDescription("Deactivates the patron. Fails if patron has active loans.");

        group.MapGet("/{id:int}/loans", async Task<Ok<PaginatedResponse<LoanResponse>>> (
            int id, LoanStatus? status, int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetLoansAsync(id, status, page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatronLoans")
        .WithSummary("Get patron's loans")
        .WithDescription("Returns loans for a patron. Optionally filter by status.");

        group.MapGet("/{id:int}/reservations", async Task<Ok<PaginatedResponse<ReservationResponse>>> (
            int id, int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetReservationsAsync(id, page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatronReservations")
        .WithSummary("Get patron's reservations");

        group.MapGet("/{id:int}/fines", async Task<Ok<PaginatedResponse<FineResponse>>> (
            int id, FineStatus? status, int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetFinesAsync(id, status, page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatronFines")
        .WithSummary("Get patron's fines")
        .WithDescription("Returns fines for a patron. Optionally filter by status.");

        return group;
    }
}
