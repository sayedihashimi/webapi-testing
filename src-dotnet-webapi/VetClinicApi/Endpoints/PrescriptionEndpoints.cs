using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PrescriptionEndpoints
{
    public static RouteGroupBuilder MapPrescriptionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/prescriptions").WithTags("Prescriptions");

        group.MapGet("/{id:int}", async Task<Results<Ok<PrescriptionResponse>, NotFound>> (
            int id, IPrescriptionService service, CancellationToken ct) =>
        {
            var prescription = await service.GetByIdAsync(id, ct);
            return prescription is null ? TypedResults.NotFound() : TypedResults.Ok(prescription);
        })
        .WithName("GetPrescriptionById")
        .WithSummary("Get prescription by ID")
        .WithDescription("Returns prescription details including active status.");

        group.MapPost("/", async Task<Results<Created<PrescriptionResponse>, BadRequest>> (
            CreatePrescriptionRequest request, IPrescriptionService service, CancellationToken ct) =>
        {
            var prescription = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/prescriptions/{prescription.Id}", prescription);
        })
        .WithName("CreatePrescription")
        .WithSummary("Create a prescription")
        .WithDescription("Creates a prescription for an existing medical record. EndDate is computed from StartDate + DurationDays.");

        return group;
    }
}
