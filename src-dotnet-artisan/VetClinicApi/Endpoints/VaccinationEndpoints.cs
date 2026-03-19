using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VaccinationEndpoints
{
    public static RouteGroupBuilder MapVaccinationEndpoints(this RouteGroupBuilder group)
    {
        var vaccinations = group.MapGroup("/vaccinations").WithTags("Vaccinations");

        vaccinations.MapGet("/{id:int}", GetVaccinationById).WithSummary("Get vaccination details");
        vaccinations.MapPost("/", CreateVaccination).WithSummary("Record a new vaccination");

        return group;
    }

    private static async Task<Results<Ok<VaccinationResponse>, NotFound>> GetVaccinationById(
        int id,
        IVaccinationService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<VaccinationResponse>, BadRequest<string>>> CreateVaccination(
        CreateVaccinationRequest request,
        IVaccinationService service,
        CancellationToken cancellationToken = default)
    {
        var (result, error) = await service.CreateAsync(request, cancellationToken);
        if (result is null)
        {
            return TypedResults.BadRequest(error!);
        }

        return TypedResults.Created($"/api/vaccinations/{result.Id}", result);
    }
}
