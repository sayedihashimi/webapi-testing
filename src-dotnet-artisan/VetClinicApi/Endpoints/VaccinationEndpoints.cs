using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VaccinationEndpoints
{
    public static RouteGroupBuilder MapVaccinationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/vaccinations")
            .WithTags("Vaccinations");

        group.MapPost("/", CreateAsync);
        group.MapGet("/{id:int}", GetByIdAsync);

        return group;
    }

    private static async Task<IResult> GetByIdAsync(
        int id,
        IVaccinationService service,
        CancellationToken ct = default)
    {
        var vaccination = await service.GetByIdAsync(id, ct);
        return vaccination is not null
            ? TypedResults.Ok(vaccination)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreateVaccinationDto dto,
        IVaccinationService service,
        IPetService petService,
        IVeterinarianService vetService,
        CancellationToken ct = default)
    {
        if (!await petService.ExistsAsync(dto.PetId, ct))
        {
            return TypedResults.Problem(
                detail: "Pet not found.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        if (!await vetService.ExistsAsync(dto.AdministeredByVetId, ct))
        {
            return TypedResults.Problem(
                detail: "Veterinarian not found.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        if (dto.ExpirationDate <= dto.DateAdministered)
        {
            return TypedResults.Problem(
                detail: "Expiration date must be after the date administered.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        var vaccination = await service.CreateAsync(dto, ct);
        return TypedResults.Created($"/api/vaccinations/{vaccination.Id}", vaccination);
    }
}
