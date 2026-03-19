using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class PatronEndpoints
{
    public static RouteGroupBuilder MapPatronEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/patrons")
            .WithTags("Patrons");

        group.MapGet("/", GetPatronsAsync);
        group.MapGet("/{id:int}", GetPatronByIdAsync);
        group.MapPost("/", CreatePatronAsync);
        group.MapPut("/{id:int}", UpdatePatronAsync);
        group.MapDelete("/{id:int}", DeactivatePatronAsync);
        group.MapGet("/{id:int}/loans", GetPatronLoansAsync);
        group.MapGet("/{id:int}/reservations", GetPatronReservationsAsync);
        group.MapGet("/{id:int}/fines", GetPatronFinesAsync);

        return group;
    }

    private static async Task<IResult> GetPatronsAsync(
        IPatronService service,
        string? search = null,
        MembershipType? membershipType = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetPatronsAsync(search, membershipType, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetPatronByIdAsync(
        int id,
        IPatronService service,
        CancellationToken ct = default)
    {
        var patron = await service.GetPatronByIdAsync(id, ct);
        return patron is not null
            ? TypedResults.Ok(patron)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreatePatronAsync(
        CreatePatronDto dto,
        IPatronService service,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName) || string.IsNullOrWhiteSpace(dto.Email))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { [""] = ["FirstName, LastName, and Email are required."] });
        }

        var patron = await service.CreatePatronAsync(dto, ct);
        return TypedResults.Created($"/api/patrons/{patron.Id}", patron);
    }

    private static async Task<IResult> UpdatePatronAsync(
        int id,
        UpdatePatronDto dto,
        IPatronService service,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName) || string.IsNullOrWhiteSpace(dto.Email))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { [""] = ["FirstName, LastName, and Email are required."] });
        }

        var patron = await service.UpdatePatronAsync(id, dto, ct);
        return patron is not null
            ? TypedResults.Ok(patron)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> DeactivatePatronAsync(
        int id,
        IPatronService service,
        CancellationToken ct = default)
    {
        var (found, hasActiveLoans) = await service.DeactivatePatronAsync(id, ct);

        if (!found)
        {
            return TypedResults.NotFound();
        }

        if (hasActiveLoans)
        {
            return TypedResults.Problem(
                detail: "Cannot deactivate patron with active loans.",
                statusCode: StatusCodes.Status409Conflict);
        }

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetPatronLoansAsync(
        int id,
        IPatronService service,
        string? status = null,
        CancellationToken ct = default)
    {
        var loans = await service.GetPatronLoansAsync(id, status, ct);
        return TypedResults.Ok(loans);
    }

    private static async Task<IResult> GetPatronReservationsAsync(
        int id,
        IPatronService service,
        CancellationToken ct = default)
    {
        var reservations = await service.GetPatronReservationsAsync(id, ct);
        return TypedResults.Ok(reservations);
    }

    private static async Task<IResult> GetPatronFinesAsync(
        int id,
        IPatronService service,
        string? status = null,
        CancellationToken ct = default)
    {
        var fines = await service.GetPatronFinesAsync(id, status, ct);
        return TypedResults.Ok(fines);
    }
}
