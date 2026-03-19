using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static RouteGroupBuilder MapAppointmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<AppointmentResponse>>, BadRequest>> (
            DateTime? dateFrom, DateTime? dateTo, string? status, int? vetId, int? petId,
            int? page, int? pageSize,
            IAppointmentService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(dateFrom, dateTo, status, vetId, petId, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetAppointments")
        .WithSummary("List all appointments")
        .WithDescription("Returns paginated appointments. Filter by date range, status, vet, or pet.");

        group.MapGet("/today", async Task<Ok<IReadOnlyList<AppointmentResponse>>> (
            IAppointmentService service, CancellationToken ct) =>
        {
            var result = await service.GetTodayAsync(ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetTodayAppointments")
        .WithSummary("Get today's appointments")
        .WithDescription("Returns all appointments scheduled for today.");

        group.MapGet("/{id:int}", async Task<Results<Ok<AppointmentDetailResponse>, NotFound>> (
            int id, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.GetByIdAsync(id, ct);
            return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
        })
        .WithName("GetAppointmentById")
        .WithSummary("Get appointment by ID")
        .WithDescription("Returns full appointment details including pet, veterinarian, and medical record.");

        group.MapPost("/", async Task<Results<Created<AppointmentResponse>, BadRequest>> (
            CreateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/appointments/{appointment.Id}", appointment);
        })
        .WithName("CreateAppointment")
        .WithSummary("Schedule a new appointment")
        .WithDescription("Schedules a new appointment. Enforces scheduling conflict detection.");

        group.MapPut("/{id:int}", async Task<Results<Ok<AppointmentResponse>, NotFound>> (
            int id, UpdateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateAsync(id, request, ct);
            return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
        })
        .WithName("UpdateAppointment")
        .WithSummary("Update an appointment")
        .WithDescription("Updates appointment details. Re-checks conflicts if time or vet changed.");

        group.MapPatch("/{id:int}/status", async Task<Results<Ok<AppointmentResponse>, NotFound>> (
            int id, UpdateAppointmentStatusRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateStatusAsync(id, request, ct);
            return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
        })
        .WithName("UpdateAppointmentStatus")
        .WithSummary("Update appointment status")
        .WithDescription("Updates the appointment status following the allowed workflow transitions.");

        return group;
    }
}
