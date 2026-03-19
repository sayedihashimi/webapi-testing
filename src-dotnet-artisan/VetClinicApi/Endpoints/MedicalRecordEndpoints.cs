using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class MedicalRecordEndpoints
{
    public static RouteGroupBuilder MapMedicalRecordEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/medical-records")
            .WithTags("Medical Records");

        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);

        return group;
    }

    private static async Task<IResult> GetByIdAsync(
        int id,
        IMedicalRecordService service,
        CancellationToken ct = default)
    {
        var record = await service.GetByIdAsync(id, ct);
        return record is not null
            ? TypedResults.Ok(record)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreateMedicalRecordDto dto,
        IMedicalRecordService service,
        CancellationToken ct = default)
    {
        var validationError = await service.ValidateAppointmentForRecordAsync(dto.AppointmentId, ct);
        if (validationError is not null)
        {
            return TypedResults.Problem(
                detail: validationError,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        var record = await service.CreateAsync(dto, ct);
        return TypedResults.Created($"/api/medical-records/{record.Id}", record);
    }

    private static async Task<IResult> UpdateAsync(
        int id,
        UpdateMedicalRecordDto dto,
        IMedicalRecordService service,
        CancellationToken ct = default)
    {
        var record = await service.UpdateAsync(id, dto, ct);
        return record is not null
            ? TypedResults.Ok(record)
            : TypedResults.NotFound();
    }
}
