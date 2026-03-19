using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PrescriptionEndpoints
{
    public static RouteGroupBuilder MapPrescriptionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/prescriptions")
            .WithTags("Prescriptions");

        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);

        return group;
    }

    private static async Task<IResult> GetByIdAsync(
        int id,
        IPrescriptionService service,
        CancellationToken ct = default)
    {
        var prescription = await service.GetByIdAsync(id, ct);
        return prescription is not null
            ? TypedResults.Ok(prescription)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreatePrescriptionDto dto,
        IPrescriptionService service,
        CancellationToken ct = default)
    {
        var prescription = await service.CreateAsync(dto, ct);
        return TypedResults.Created($"/api/prescriptions/{prescription.Id}", prescription);
    }
}
