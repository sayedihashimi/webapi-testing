using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class OwnerEndpoints
{
    public static RouteGroupBuilder MapOwnerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/owners")
            .WithTags("Owners");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        group.MapGet("/{id:int}/pets", GetPetsAsync);
        group.MapGet("/{id:int}/appointments", GetAppointmentsAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        IOwnerService service,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(search, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync(
        int id,
        IOwnerService service,
        CancellationToken ct = default)
    {
        var owner = await service.GetByIdAsync(id, ct);
        return owner is not null
            ? TypedResults.Ok(owner)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreateOwnerDto dto,
        IOwnerService service,
        CancellationToken ct = default)
    {
        var owner = await service.CreateAsync(dto, ct);
        return TypedResults.Created($"/api/owners/{owner.Id}", owner);
    }

    private static async Task<IResult> UpdateAsync(
        int id,
        UpdateOwnerDto dto,
        IOwnerService service,
        CancellationToken ct = default)
    {
        var owner = await service.UpdateAsync(id, dto, ct);
        return owner is not null
            ? TypedResults.Ok(owner)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> DeleteAsync(
        int id,
        IOwnerService service,
        CancellationToken ct = default)
    {
        if (await service.HasActivePetsAsync(id, ct))
        {
            return TypedResults.Problem(
                detail: "Cannot delete owner with active pets. Deactivate or transfer pets first.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict");
        }

        var deleted = await service.DeleteAsync(id, ct);
        return deleted
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<IResult> GetPetsAsync(
        int id,
        IOwnerService service,
        CancellationToken ct = default)
    {
        var owner = await service.GetByIdAsync(id, ct);
        if (owner is null)
        {
            return TypedResults.NotFound();
        }

        var pets = await service.GetPetsAsync(id, ct);
        return TypedResults.Ok(pets);
    }

    private static async Task<IResult> GetAppointmentsAsync(
        int id,
        IOwnerService service,
        CancellationToken ct = default)
    {
        var owner = await service.GetByIdAsync(id, ct);
        if (owner is null)
        {
            return TypedResults.NotFound();
        }

        var appointments = await service.GetAppointmentsAsync(id, ct);
        return TypedResults.Ok(appointments);
    }
}
