using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static RouteGroupBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/appointments")
            .WithTags("Appointments");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/today", GetTodayAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapPatch("/{id:int}/status", UpdateStatusAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        IAppointmentService service,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? status = null,
        int? vetId = null,
        int? petId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(fromDate, toDate, status, vetId, petId, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetTodayAsync(
        IAppointmentService service,
        CancellationToken ct = default)
    {
        var appointments = await service.GetTodayAsync(ct);
        return TypedResults.Ok(appointments);
    }

    private static async Task<IResult> GetByIdAsync(
        int id,
        IAppointmentService service,
        CancellationToken ct = default)
    {
        var appointment = await service.GetByIdAsync(id, ct);
        return appointment is not null
            ? TypedResults.Ok(appointment)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreateAppointmentDto dto,
        IAppointmentService service,
        IPetService petService,
        IVeterinarianService vetService,
        CancellationToken ct = default)
    {
        if (!await petService.ExistsAsync(dto.PetId, ct))
        {
            return TypedResults.Problem(
                detail: "Pet not found.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        if (!await vetService.ExistsAsync(dto.VeterinarianId, ct))
        {
            return TypedResults.Problem(
                detail: "Veterinarian not found.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        if (dto.AppointmentDate <= DateTime.UtcNow)
        {
            return TypedResults.Problem(
                detail: "Appointment date must be in the future.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        if (await service.HasConflictAsync(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, ct: ct))
        {
            return TypedResults.Problem(
                detail: "The veterinarian has a conflicting appointment at the requested time.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Scheduling Conflict");
        }

        var appointment = await service.CreateAsync(dto, ct);
        return TypedResults.Created($"/api/appointments/{appointment.Id}", appointment);
    }

    private static async Task<IResult> UpdateAsync(
        int id,
        UpdateAppointmentDto dto,
        IAppointmentService service,
        IPetService petService,
        IVeterinarianService vetService,
        CancellationToken ct = default)
    {
        if (!await petService.ExistsAsync(dto.PetId, ct))
        {
            return TypedResults.Problem(
                detail: "Pet not found.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        if (!await vetService.ExistsAsync(dto.VeterinarianId, ct))
        {
            return TypedResults.Problem(
                detail: "Veterinarian not found.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        if (await service.HasConflictAsync(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id, ct))
        {
            return TypedResults.Problem(
                detail: "The veterinarian has a conflicting appointment at the requested time.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Scheduling Conflict");
        }

        var appointment = await service.UpdateAsync(id, dto, ct);
        return appointment is not null
            ? TypedResults.Ok(appointment)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> UpdateStatusAsync(
        int id,
        UpdateAppointmentStatusDto dto,
        IAppointmentService service,
        CancellationToken ct = default)
    {
        if (dto.NewStatus == AppointmentStatus.Cancelled && string.IsNullOrWhiteSpace(dto.CancellationReason))
        {
            return TypedResults.Problem(
                detail: "Cancellation reason is required when cancelling an appointment.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        var validationError = await service.ValidateStatusTransitionAsync(id, dto.NewStatus, ct);
        if (validationError is not null)
        {
            return TypedResults.Problem(
                detail: validationError,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Status Transition");
        }

        var appointment = await service.UpdateStatusAsync(id, dto, ct);
        return appointment is not null
            ? TypedResults.Ok(appointment)
            : TypedResults.NotFound();
    }
}
