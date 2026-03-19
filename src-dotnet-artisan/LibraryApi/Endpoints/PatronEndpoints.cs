using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class PatronEndpoints
{
    public static RouteGroupBuilder MapPatronEndpoints(this RouteGroupBuilder group)
    {
        var patrons = group.MapGroup("/patrons").WithTags("Patrons");

        patrons.MapGet("/", GetPatronsAsync)
            .WithSummary("List patrons with search, membership type filter, and pagination");

        patrons.MapGet("/{id:int}", GetPatronByIdAsync)
            .WithSummary("Get patron details with active loans count and unpaid fines");

        patrons.MapPost("/", CreatePatronAsync)
            .WithSummary("Create a new patron");

        patrons.MapPut("/{id:int}", UpdatePatronAsync)
            .WithSummary("Update an existing patron");

        patrons.MapDelete("/{id:int}", DeactivatePatronAsync)
            .WithSummary("Deactivate a patron (fails if patron has active loans)");

        patrons.MapGet("/{id:int}/loans", GetPatronLoansAsync)
            .WithSummary("Get patron's loans with optional status filter");

        patrons.MapGet("/{id:int}/reservations", GetPatronReservationsAsync)
            .WithSummary("Get patron's reservations");

        patrons.MapGet("/{id:int}/fines", GetPatronFinesAsync)
            .WithSummary("Get patron's fines with optional status filter");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<PatronResponse>>> GetPatronsAsync(
        IPatronService service, string? search, MembershipType? membershipType,
        int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetPatronsAsync(search, membershipType, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PatronDetailResponse>, NotFound>> GetPatronByIdAsync(
        int id, IPatronService service, CancellationToken ct = default)
    {
        var result = await service.GetPatronByIdAsync(id, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Created<PatronResponse>> CreatePatronAsync(
        CreatePatronRequest request, IPatronService service, CancellationToken ct = default)
    {
        var result = await service.CreatePatronAsync(request, ct);
        return TypedResults.Created($"/api/patrons/{result.Id}", result);
    }

    private static async Task<Results<Ok<PatronResponse>, NotFound>> UpdatePatronAsync(
        int id, UpdatePatronRequest request, IPatronService service, CancellationToken ct = default)
    {
        var result = await service.UpdatePatronAsync(id, request, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>>> DeactivatePatronAsync(
        int id, IPatronService service, CancellationToken ct = default)
    {
        var (found, hasActiveLoans) = await service.DeactivatePatronAsync(id, ct);
        if (!found)
        {
            return TypedResults.NotFound();
        }

        return hasActiveLoans
            ? TypedResults.Conflict("Cannot deactivate patron because they have active loans.")
            : TypedResults.NoContent();
    }

    private static async Task<Ok<PaginatedResponse<LoanResponse>>> GetPatronLoansAsync(
        int id, IPatronService service, string? status, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetPatronLoansAsync(id, status, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<PaginatedResponse<ReservationResponse>>> GetPatronReservationsAsync(
        int id, IPatronService service, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetPatronReservationsAsync(id, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<PaginatedResponse<FineResponse>>> GetPatronFinesAsync(
        int id, IPatronService service, string? status, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetPatronFinesAsync(id, status, page, pageSize, ct);
        return TypedResults.Ok(result);
    }
}
