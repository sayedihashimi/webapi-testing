using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PetEndpoints
{
    public static RouteGroupBuilder MapPetEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/pets")
            .WithTags("Pets");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", SoftDeleteAsync);
        group.MapGet("/{id:int}/medical-records", GetMedicalRecordsAsync);
        group.MapGet("/{id:int}/vaccinations", GetVaccinationsAsync);
        group.MapGet("/{id:int}/vaccinations/upcoming", GetUpcomingVaccinationsAsync);
        group.MapGet("/{id:int}/prescriptions/active", GetActivePrescriptionsAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        IPetService service,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(search, page, pageSize, includeInactive, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync(
        int id,
        IPetService service,
        CancellationToken ct = default)
    {
        var pet = await service.GetByIdAsync(id, ct);
        return pet is not null
            ? TypedResults.Ok(pet)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreatePetDto dto,
        IPetService service,
        CancellationToken ct = default)
    {
        var pet = await service.CreateAsync(dto, ct);
        return TypedResults.Created($"/api/pets/{pet.Id}", pet);
    }

    private static async Task<IResult> UpdateAsync(
        int id,
        UpdatePetDto dto,
        IPetService service,
        CancellationToken ct = default)
    {
        var pet = await service.UpdateAsync(id, dto, ct);
        return pet is not null
            ? TypedResults.Ok(pet)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> SoftDeleteAsync(
        int id,
        IPetService service,
        CancellationToken ct = default)
    {
        var deleted = await service.SoftDeleteAsync(id, ct);
        return deleted
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<IResult> GetMedicalRecordsAsync(
        int id,
        IPetService service,
        CancellationToken ct = default)
    {
        if (!await service.ExistsAsync(id, ct))
        {
            return TypedResults.NotFound();
        }

        var records = await service.GetMedicalRecordsAsync(id, ct);
        return TypedResults.Ok(records);
    }

    private static async Task<IResult> GetVaccinationsAsync(
        int id,
        IPetService service,
        CancellationToken ct = default)
    {
        if (!await service.ExistsAsync(id, ct))
        {
            return TypedResults.NotFound();
        }

        var vaccinations = await service.GetVaccinationsAsync(id, ct);
        return TypedResults.Ok(vaccinations);
    }

    private static async Task<IResult> GetUpcomingVaccinationsAsync(
        int id,
        IPetService service,
        CancellationToken ct = default)
    {
        if (!await service.ExistsAsync(id, ct))
        {
            return TypedResults.NotFound();
        }

        var vaccinations = await service.GetUpcomingVaccinationsAsync(id, ct);
        return TypedResults.Ok(vaccinations);
    }

    private static async Task<IResult> GetActivePrescriptionsAsync(
        int id,
        IPetService service,
        CancellationToken ct = default)
    {
        if (!await service.ExistsAsync(id, ct))
        {
            return TypedResults.NotFound();
        }

        var prescriptions = await service.GetActivePrescriptionsAsync(id, ct);
        return TypedResults.Ok(prescriptions);
    }
}
