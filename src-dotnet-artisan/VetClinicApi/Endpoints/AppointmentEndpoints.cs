using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static RouteGroupBuilder MapAppointmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments");

        group.MapGet("/", GetAll)
            .WithName("GetAppointments")
            .WithSummary("Get all appointments with optional filters and pagination");

        group.MapGet("/today", GetToday)
            .WithName("GetTodayAppointments")
            .WithSummary("Get all appointments for today");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetAppointmentById")
            .WithSummary("Get appointment by ID with full details");

        group.MapPost("/", Create)
            .WithName("CreateAppointment")
            .WithSummary("Create a new appointment (enforces conflict detection)");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateAppointment")
            .WithSummary("Update an existing appointment");

        group.MapPatch("/{id:int}/status", UpdateStatus)
            .WithName("UpdateAppointmentStatus")
            .WithSummary("Update appointment status (enforces workflow rules)");

        return group;
    }

    private static async Task<Ok<PagedResponse<AppointmentResponse>>> GetAll(
        AppointmentService service,
        DateTime? dateFrom = null, DateTime? dateTo = null, AppointmentStatus? status = null,
        int? vetId = null, int? petId = null, int page = 1, int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await service.GetAllAsync(dateFrom, dateTo, status, vetId, petId, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<List<AppointmentResponse>>> GetToday(
        AppointmentService service, CancellationToken ct)
    {
        var appointments = await service.GetTodayAsync(ct);
        return TypedResults.Ok(appointments);
    }

    private static async Task<Results<Ok<AppointmentDetailResponse>, NotFound>> GetById(
        int id, AppointmentService service, CancellationToken ct)
    {
        var appt = await service.GetByIdAsync(id, ct);
        return appt is not null ? TypedResults.Ok(appt) : TypedResults.NotFound();
    }

    private static async Task<Created<AppointmentResponse>> Create(
        CreateAppointmentRequest request, AppointmentService service, CancellationToken ct)
    {
        var appt = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/appointments/{appt.Id}", appt);
    }

    private static async Task<Results<Ok<AppointmentResponse>, NotFound>> Update(
        int id, UpdateAppointmentRequest request, AppointmentService service, CancellationToken ct)
    {
        var appt = await service.UpdateAsync(id, request, ct);
        return appt is not null ? TypedResults.Ok(appt) : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<AppointmentResponse>, NotFound>> UpdateStatus(
        int id, UpdateAppointmentStatusRequest request, AppointmentService service, CancellationToken ct)
    {
        var appt = await service.UpdateStatusAsync(id, request, ct);
        return appt is not null ? TypedResults.Ok(appt) : TypedResults.NotFound();
    }
}
