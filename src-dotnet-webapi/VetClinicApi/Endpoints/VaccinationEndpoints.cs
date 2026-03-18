using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VaccinationEndpoints
{
    public static void MapVaccinationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vaccinations").WithTags("Vaccinations");

        group.MapGet("/{id:int}", async (int id, IVaccinationService service, CancellationToken ct) =>
        {
            var vaccination = await service.GetByIdAsync(id, ct);
            return vaccination is null ? Results.NotFound() : Results.Ok(vaccination);
        })
        .WithName("GetVaccinationById")
        .WithSummary("Get vaccination details")
        .WithDescription("Returns vaccination details including expiration and due-soon status.")
        .Produces<VaccinationResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateVaccinationRequest request, IVaccinationService service, CancellationToken ct) =>
        {
            var vaccination = await service.CreateAsync(request, ct);
            return Results.Created($"/api/vaccinations/{vaccination.Id}", vaccination);
        })
        .WithName("CreateVaccination")
        .WithSummary("Record a vaccination")
        .WithDescription("Records a new vaccination for a pet. Expiration date must be after administration date.")
        .Produces<VaccinationResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
