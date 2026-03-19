using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VaccinationEndpoints
{
    public static void MapVaccinationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vaccinations").WithTags("Vaccinations");

        group.MapGet("/{id:int}", async Task<Results<Ok<VaccinationResponse>, NotFound>> (
            int id, IVaccinationService service, CancellationToken ct) =>
        {
            var vaccination = await service.GetByIdAsync(id, ct);
            return vaccination is null ? TypedResults.NotFound() : TypedResults.Ok(vaccination);
        })
        .WithName("GetVaccinationById")
        .WithSummary("Get a vaccination by ID")
        .WithDescription("Returns vaccination details including expired and due-soon status.")
        .Produces<VaccinationResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<VaccinationResponse>> (
            CreateVaccinationRequest request, IVaccinationService service, CancellationToken ct) =>
        {
            var vaccination = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/vaccinations/{vaccination.Id}", vaccination);
        })
        .WithName("CreateVaccination")
        .WithSummary("Record a new vaccination")
        .WithDescription("Records a new vaccination for a pet. Expiration date must be after date administered.")
        .Produces<VaccinationResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
