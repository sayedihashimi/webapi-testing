using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PrescriptionEndpoints
{
    public static RouteGroupBuilder MapPrescriptionEndpoints(this RouteGroupBuilder group)
    {
        var prescriptions = group.MapGroup("/prescriptions").WithTags("Prescriptions");

        prescriptions.MapGet("/{id:int}", GetPrescriptionById).WithSummary("Get prescription details");
        prescriptions.MapPost("/", CreatePrescription).WithSummary("Create a prescription for a medical record");

        return group;
    }

    private static async Task<Results<Ok<PrescriptionResponse>, NotFound>> GetPrescriptionById(
        int id,
        IPrescriptionService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<PrescriptionResponse>, BadRequest<string>>> CreatePrescription(
        CreatePrescriptionRequest request,
        IPrescriptionService service,
        CancellationToken cancellationToken = default)
    {
        var (result, error) = await service.CreateAsync(request, cancellationToken);
        if (result is null)
        {
            return TypedResults.BadRequest(error!);
        }

        return TypedResults.Created($"/api/prescriptions/{result.Id}", result);
    }
}
