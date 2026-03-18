using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PrescriptionEndpoints
{
    public static RouteGroupBuilder MapPrescriptionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/prescriptions").WithTags("Prescriptions");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetPrescriptionById")
            .WithSummary("Get prescription by ID");

        group.MapPost("/", Create)
            .WithName("CreatePrescription")
            .WithSummary("Create a prescription for a medical record");

        return group;
    }

    private static async Task<Results<Ok<PrescriptionResponse>, NotFound>> GetById(
        int id, PrescriptionService service, CancellationToken ct)
    {
        var rx = await service.GetByIdAsync(id, ct);
        return rx is not null ? TypedResults.Ok(rx) : TypedResults.NotFound();
    }

    private static async Task<Created<PrescriptionResponse>> Create(
        CreatePrescriptionRequest request, PrescriptionService service, CancellationToken ct)
    {
        var rx = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/prescriptions/{rx.Id}", rx);
    }
}
