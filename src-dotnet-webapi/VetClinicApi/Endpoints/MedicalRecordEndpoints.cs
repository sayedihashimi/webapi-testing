using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class MedicalRecordEndpoints
{
    public static RouteGroupBuilder MapMedicalRecordEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/medical-records").WithTags("Medical Records");

        group.MapGet("/{id:int}", async Task<Results<Ok<MedicalRecordResponse>, NotFound>> (
            int id, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.GetByIdAsync(id, ct);
            return record is null ? TypedResults.NotFound() : TypedResults.Ok(record);
        })
        .WithName("GetMedicalRecordById")
        .WithSummary("Get medical record by ID")
        .WithDescription("Returns the medical record with its prescriptions.");

        group.MapPost("/", async Task<Results<Created<MedicalRecordResponse>, BadRequest>> (
            CreateMedicalRecordRequest request, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/medical-records/{record.Id}", record);
        })
        .WithName("CreateMedicalRecord")
        .WithSummary("Create a medical record")
        .WithDescription("Creates a medical record for a Completed or InProgress appointment. One per appointment.");

        group.MapPut("/{id:int}", async Task<Results<Ok<MedicalRecordResponse>, NotFound>> (
            int id, UpdateMedicalRecordRequest request, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.UpdateAsync(id, request, ct);
            return record is null ? TypedResults.NotFound() : TypedResults.Ok(record);
        })
        .WithName("UpdateMedicalRecord")
        .WithSummary("Update a medical record")
        .WithDescription("Updates the diagnosis, treatment, notes, and follow-up date.");

        return group;
    }
}
