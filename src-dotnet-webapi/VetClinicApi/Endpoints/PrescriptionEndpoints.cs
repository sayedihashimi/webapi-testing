using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PrescriptionEndpoints
{
    public static void MapPrescriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/prescriptions").WithTags("Prescriptions");

        group.MapGet("/{id:int}", async (int id, IPrescriptionService service, CancellationToken ct) =>
        {
            var prescription = await service.GetByIdAsync(id, ct);
            return prescription is null ? Results.NotFound() : Results.Ok(prescription);
        })
        .WithName("GetPrescriptionById")
        .WithSummary("Get prescription details")
        .WithDescription("Returns details for a specific prescription including active status.")
        .Produces<PrescriptionResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreatePrescriptionRequest request, IPrescriptionService service, CancellationToken ct) =>
        {
            var prescription = await service.CreateAsync(request, ct);
            return Results.Created($"/api/prescriptions/{prescription.Id}", prescription);
        })
        .WithName("CreatePrescription")
        .WithSummary("Create a prescription")
        .WithDescription("Creates a new prescription for a medical record. End date is calculated from start date and duration.")
        .Produces<PrescriptionResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
