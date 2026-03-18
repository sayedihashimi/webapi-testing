using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class MedicalRecordEndpoints
{
    public static void MapMedicalRecordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/medical-records").WithTags("Medical Records");

        group.MapGet("/{id:int}", async (int id, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.GetByIdAsync(id, ct);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetMedicalRecordById")
        .WithSummary("Get medical record details")
        .WithDescription("Returns medical record details including prescriptions.")
        .Produces<MedicalRecordDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateMedicalRecordRequest request, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.CreateAsync(request, ct);
            return Results.Created($"/api/medical-records/{record.Id}", record);
        })
        .WithName("CreateMedicalRecord")
        .WithSummary("Create a medical record")
        .WithDescription("Creates a medical record for a completed or in-progress appointment. One record per appointment.")
        .Produces<MedicalRecordResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateMedicalRecordRequest request, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.UpdateAsync(id, request, ct);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("UpdateMedicalRecord")
        .WithSummary("Update a medical record")
        .WithDescription("Updates diagnosis, treatment, notes, and follow-up date.")
        .Produces<MedicalRecordResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
