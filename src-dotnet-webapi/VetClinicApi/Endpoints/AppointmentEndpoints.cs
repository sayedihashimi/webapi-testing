using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments");

        group.MapGet("/", async Task<Ok<PaginatedResponse<AppointmentResponse>>> (
            IAppointmentService service,
            DateTime? dateFrom,
            DateTime? dateTo,
            AppointmentStatus? status,
            int? vetId,
            int? petId,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var result = await service.GetAllAsync(dateFrom, dateTo, status, vetId, petId, page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetAppointments")
        .WithSummary("List all appointments")
        .WithDescription("Returns a paginated list of appointments. Supports filter by date range, status, vet, and pet.")
        .Produces<PaginatedResponse<AppointmentResponse>>(StatusCodes.Status200OK);

        group.MapGet("/today", async Task<Ok<IReadOnlyList<AppointmentResponse>>> (
            IAppointmentService service, CancellationToken ct) =>
        {
            var result = await service.GetTodayAsync(ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetTodayAppointments")
        .WithSummary("Get today's appointments")
        .WithDescription("Returns all appointments scheduled for today, ordered by time.")
        .Produces<IReadOnlyList<AppointmentResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<AppointmentDetailResponse>, NotFound>> (
            int id, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.GetByIdAsync(id, ct);
            return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
        })
        .WithName("GetAppointmentById")
        .WithSummary("Get an appointment by ID")
        .WithDescription("Returns appointment details including pet, veterinarian, and medical record.")
        .Produces<AppointmentDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<AppointmentResponse>> (
            CreateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/appointments/{appointment.Id}", appointment);
        })
        .WithName("CreateAppointment")
        .WithSummary("Schedule a new appointment")
        .WithDescription("Creates a new appointment. Enforces conflict detection to prevent overlapping appointments for the same veterinarian.")
        .Produces<AppointmentResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<AppointmentResponse>, NotFound>> (
            int id, UpdateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateAsync(id, request, ct);
            return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
        })
        .WithName("UpdateAppointment")
        .WithSummary("Update an appointment")
        .WithDescription("Updates appointment details. Re-checks conflicts if date/time/vet changes.")
        .Produces<AppointmentResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:int}/status", async Task<Results<Ok<AppointmentResponse>, NotFound>> (
            int id, UpdateAppointmentStatusRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateStatusAsync(id, request, ct);
            return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
        })
        .WithName("UpdateAppointmentStatus")
        .WithSummary("Update appointment status")
        .WithDescription("Updates the status of an appointment. Enforces workflow: Scheduled→CheckedIn/Cancelled/NoShow, CheckedIn→InProgress/Cancelled, InProgress→Completed.")
        .Produces<AppointmentResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
