using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static RouteGroupBuilder MapAppointmentEndpoints(this RouteGroupBuilder group)
    {
        var appointments = group.MapGroup("/appointments").WithTags("Appointments");

        appointments.MapGet("/", GetAppointments).WithSummary("List appointments with filters and pagination");
        appointments.MapGet("/today", GetTodayAppointments).WithSummary("Get all of today's appointments");
        appointments.MapGet("/{id:int}", GetAppointmentById).WithSummary("Get appointment details including pet, vet, and medical record");
        appointments.MapPost("/", CreateAppointment).WithSummary("Schedule a new appointment with conflict detection");
        appointments.MapPut("/{id:int}", UpdateAppointment).WithSummary("Update appointment details with conflict re-check");
        appointments.MapPatch("/{id:int}/status", UpdateAppointmentStatus).WithSummary("Update appointment status with workflow validation");

        return group;
    }

    private static async Task<Ok<PagedResult<AppointmentResponse>>> GetAppointments(
        IAppointmentService service,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? status = null,
        int? vetId = null,
        int? petId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetAllAsync(fromDate, toDate, status, vetId, petId, page, pageSize, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<AppointmentDetailResponse>, NotFound>> GetAppointmentById(
        int id,
        IAppointmentService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<AppointmentResponse>, BadRequest<string>>> CreateAppointment(
        CreateAppointmentRequest request,
        IAppointmentService service,
        CancellationToken cancellationToken = default)
    {
        var (result, error) = await service.CreateAsync(request, cancellationToken);
        if (result is null)
        {
            return TypedResults.BadRequest(error!);
        }

        return TypedResults.Created($"/api/appointments/{result.Id}", result);
    }

    private static async Task<Results<Ok<AppointmentResponse>, NotFound, BadRequest<string>>> UpdateAppointment(
        int id,
        UpdateAppointmentRequest request,
        IAppointmentService service,
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

    private static async Task<Results<Ok, NotFound, BadRequest<string>>> UpdateAppointmentStatus(
        int id,
        UpdateAppointmentStatusRequest request,
        IAppointmentService service,
        CancellationToken cancellationToken = default)
    {
        var (success, error, notFound) = await service.UpdateStatusAsync(id, request, cancellationToken);
        if (notFound)
        {
            return TypedResults.NotFound();
        }

        if (!success)
        {
            return TypedResults.BadRequest(error!);
        }

        return TypedResults.Ok();
    }

    private static async Task<Ok<IReadOnlyList<AppointmentResponse>>> GetTodayAppointments(
        IAppointmentService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetTodayAsync(cancellationToken);
        return TypedResults.Ok(result);
    }
}
