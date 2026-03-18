using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VaccinationEndpoints
{
    public static RouteGroupBuilder MapVaccinationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/vaccinations").WithTags("Vaccinations");

        group.MapPost("/", Create)
            .WithName("CreateVaccination")
            .WithSummary("Record a new vaccination");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetVaccinationById")
            .WithSummary("Get vaccination by ID");

        return group;
    }

    private static async Task<Created<VaccinationResponse>> Create(
        CreateVaccinationRequest request, VaccinationService service, CancellationToken ct)
    {
        var vax = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/vaccinations/{vax.Id}", vax);
    }

    private static async Task<Results<Ok<VaccinationResponse>, NotFound>> GetById(
        int id, VaccinationService service, CancellationToken ct)
    {
        var vax = await service.GetByIdAsync(id, ct);
        return vax is not null ? TypedResults.Ok(vax) : TypedResults.NotFound();
    }
}
