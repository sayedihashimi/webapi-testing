using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class MedicalRecordEndpoints
{
    public static RouteGroupBuilder MapMedicalRecordEndpoints(this RouteGroupBuilder group)
    {
        var records = group.MapGroup("/medical-records").WithTags("Medical Records");

        records.MapGet("/{id:int}", GetMedicalRecordById).WithSummary("Get medical record with prescriptions");
        records.MapPost("/", CreateMedicalRecord).WithSummary("Create medical record for a completed/in-progress appointment");
        records.MapPut("/{id:int}", UpdateMedicalRecord).WithSummary("Update a medical record");

        return group;
    }

    private static async Task<Results<Ok<MedicalRecordDetailResponse>, NotFound>> GetMedicalRecordById(
        int id,
        IMedicalRecordService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<MedicalRecordResponse>, BadRequest<string>>> CreateMedicalRecord(
        CreateMedicalRecordRequest request,
        IMedicalRecordService service,
        CancellationToken cancellationToken = default)
    {
        var (result, error) = await service.CreateAsync(request, cancellationToken);
        if (result is null)
        {
            return TypedResults.BadRequest(error!);
        }

        return TypedResults.Created($"/api/medical-records/{result.Id}", result);
    }

    private static async Task<Results<Ok<MedicalRecordResponse>, NotFound, BadRequest<string>>> UpdateMedicalRecord(
        int id,
        UpdateMedicalRecordRequest request,
        IMedicalRecordService service,
        CancellationToken cancellationToken = default)
    {
        var (result, error, notFound) = await service.UpdateAsync(id, request, cancellationToken);
        if (notFound)
        {
            return TypedResults.NotFound();
        }

        if (result is null)
        {
            return TypedResults.BadRequest(error!);
        }

        return TypedResults.Ok(result);
    }
}
