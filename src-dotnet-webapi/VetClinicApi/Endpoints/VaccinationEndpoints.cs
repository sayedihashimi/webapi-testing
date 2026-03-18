using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VaccinationEndpoints
{
    public static RouteGroupBuilder MapVaccinationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/vaccinations").WithTags("Vaccinations");

        group.MapGet("/{id:int}", async (int id, IVaccinationService service, CancellationToken ct) =>
        {
            var vaccination = await service.GetByIdAsync(id, ct);
            return vaccination is null ? Results.NotFound() : Results.Ok(vaccination);
        })
        .WithName("GetVaccinationById")
        .WithSummary("Get a vaccination by ID")
        .WithDescription("Returns vaccination details including expiration status and administering veterinarian.")
        .Produces<VaccinationResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateVaccinationRequest request, IVaccinationService service, CancellationToken ct) =>
        {
            var vaccination = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/vaccinations/{vaccination.Id}", vaccination);
        })
        .WithName("CreateVaccination")
        .WithSummary("Record a new vaccination")
        .WithDescription("Records a new vaccination. Expiration date must be after date administered.")
        .Produces<VaccinationResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
