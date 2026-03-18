using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PrescriptionEndpoints
{
    public static RouteGroupBuilder MapPrescriptionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/prescriptions").WithTags("Prescriptions");

        group.MapGet("/{id:int}", async (int id, IPrescriptionService service, CancellationToken ct) =>
        {
            var prescription = await service.GetByIdAsync(id, ct);
            return prescription is null ? Results.NotFound() : Results.Ok(prescription);
        })
        .WithName("GetPrescriptionById")
        .WithSummary("Get a prescription by ID")
        .WithDescription("Returns detailed information about a specific prescription including active status.")
        .Produces<PrescriptionResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreatePrescriptionRequest request, IPrescriptionService service, CancellationToken ct) =>
        {
            var prescription = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/prescriptions/{prescription.Id}", prescription);
        })
        .WithName("CreatePrescription")
        .WithSummary("Create a prescription")
        .WithDescription("Creates a new prescription for an existing medical record. EndDate is computed from StartDate + DurationDays.")
        .Produces<PrescriptionResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
