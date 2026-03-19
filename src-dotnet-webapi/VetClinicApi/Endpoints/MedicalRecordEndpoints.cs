using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class MedicalRecordEndpoints
{
    public static void MapMedicalRecordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/medical-records").WithTags("Medical Records");

        group.MapGet("/{id:int}", async Task<Results<Ok<MedicalRecordDetailResponse>, NotFound>> (
            int id, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.GetByIdAsync(id, ct);
            return record is null ? TypedResults.NotFound() : TypedResults.Ok(record);
        })
        .WithName("GetMedicalRecordById")
        .WithSummary("Get a medical record by ID")
        .WithDescription("Returns medical record details including prescriptions.")
        .Produces<MedicalRecordDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<MedicalRecordResponse>> (
            CreateMedicalRecordRequest request, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/medical-records/{record.Id}", record);
        })
        .WithName("CreateMedicalRecord")
        .WithSummary("Create a medical record")
        .WithDescription("Creates a medical record for a completed or in-progress appointment. Only one record per appointment.")
        .Produces<MedicalRecordResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<MedicalRecordResponse>, NotFound>> (
            int id, UpdateMedicalRecordRequest request, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.UpdateAsync(id, request, ct);
            return record is null ? TypedResults.NotFound() : TypedResults.Ok(record);
        })
        .WithName("UpdateMedicalRecord")
        .WithSummary("Update a medical record")
        .WithDescription("Updates diagnosis, treatment, notes, and follow-up date.")
        .Produces<MedicalRecordResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
