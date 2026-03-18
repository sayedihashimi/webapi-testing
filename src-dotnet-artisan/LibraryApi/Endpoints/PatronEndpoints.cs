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

        group.MapGet("/", GetPatrons).WithName("GetPatrons");
        group.MapGet("/{id:int}", GetPatron).WithName("GetPatron");
        group.MapPost("/", CreatePatron).WithName("CreatePatron");
        group.MapPut("/{id:int}", UpdatePatron).WithName("UpdatePatron");
        group.MapDelete("/{id:int}", DeletePatron).WithName("DeletePatron");
        group.MapGet("/{id:int}/loans", GetPatronLoans).WithName("GetPatronLoans");
        group.MapGet("/{id:int}/reservations", GetPatronReservations).WithName("GetPatronReservations");
        group.MapGet("/{id:int}/fines", GetPatronFines).WithName("GetPatronFines");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<PatronResponse>>> GetPatrons(
        LibraryService service,
        string? search = null, MembershipType? membershipType = null,
        int page = 1, int pageSize = 10)
    {
        var result = await service.GetPatronsAsync(search, membershipType, page, pageSize);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PatronDetailResponse>, NotFound>> GetPatron(
        LibraryService service, int id)
    {
        var result = await service.GetPatronByIdAsync(id);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<PatronResponse>, Conflict<string>>> CreatePatron(
        LibraryService service, CreatePatronRequest request)
    {
        var (result, error) = await service.CreatePatronAsync(request);
        return result is not null
            ? TypedResults.Created($"/api/patrons/{result.Id}", result)
            : TypedResults.Conflict(error!);
    }

    private static async Task<Results<Ok<PatronResponse>, NotFound, Conflict<string>>> UpdatePatron(
        LibraryService service, int id, UpdatePatronRequest request)
    {
        var (result, error) = await service.UpdatePatronAsync(id, request);
        if (error == "Patron not found") { return TypedResults.NotFound(); }
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.Conflict(error!);
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>>> DeletePatron(
        LibraryService service, int id)
    {
        var (success, error) = await service.DeletePatronAsync(id);
        if (error == "Patron not found") { return TypedResults.NotFound(); }
        if (!success) { return TypedResults.Conflict(error!); }
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<LoanResponse>>> GetPatronLoans(
        LibraryService service, int id, LoanStatus? status = null)
    {
        var result = await service.GetPatronLoansAsync(id, status);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<List<ReservationResponse>>> GetPatronReservations(
        LibraryService service, int id)
    {
        var result = await service.GetPatronReservationsAsync(id);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<List<FineResponse>>> GetPatronFines(
        LibraryService service, int id, FineStatus? status = null)
    {
        var result = await service.GetPatronFinesAsync(id, status);
        return TypedResults.Ok(result);
    }
}
