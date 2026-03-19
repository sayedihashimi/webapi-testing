using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PrescriptionEndpoints
{
    public static void MapPrescriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/prescriptions").WithTags("Prescriptions");

        group.MapGet("/{id:int}", async Task<Results<Ok<PrescriptionResponse>, NotFound>> (
            int id, IPrescriptionService service, CancellationToken ct) =>
        {
            var prescription = await service.GetByIdAsync(id, ct);
            return prescription is null ? TypedResults.NotFound() : TypedResults.Ok(prescription);
        })
        .WithName("GetPrescriptionById")
        .WithSummary("Get a prescription by ID")
        .WithDescription("Returns prescription details including active status.")
        .Produces<PrescriptionResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<PrescriptionResponse>> (
            CreatePrescriptionRequest request, IPrescriptionService service, CancellationToken ct) =>
        {
            var prescription = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/prescriptions/{prescription.Id}", prescription);
        })
        .WithName("CreatePrescription")
        .WithSummary("Create a prescription")
        .WithDescription("Creates a prescription for a medical record. EndDate is computed from StartDate + DurationDays.")
        .Produces<PrescriptionResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
