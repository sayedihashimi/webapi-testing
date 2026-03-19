using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VaccinationEndpoints
{
    public static RouteGroupBuilder MapVaccinationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/vaccinations").WithTags("Vaccinations");

        group.MapPost("/", async Task<Results<Created<VaccinationResponse>, BadRequest>> (
            CreateVaccinationRequest request, IVaccinationService service, CancellationToken ct) =>
        {
            var vaccination = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/vaccinations/{vaccination.Id}", vaccination);
        })
        .WithName("CreateVaccination")
        .WithSummary("Record a vaccination")
        .WithDescription("Records a new vaccination for a pet. Expiration date must be after date administered.");

        group.MapGet("/{id:int}", async Task<Results<Ok<VaccinationResponse>, NotFound>> (
            int id, IVaccinationService service, CancellationToken ct) =>
        {
            var vaccination = await service.GetByIdAsync(id, ct);
            return vaccination is null ? TypedResults.NotFound() : TypedResults.Ok(vaccination);
        })
        .WithName("GetVaccinationById")
        .WithSummary("Get vaccination by ID")
        .WithDescription("Returns vaccination details including expired and due-soon status.");

        return group;
    }
}
