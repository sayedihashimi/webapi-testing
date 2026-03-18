using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments");

        group.MapGet("/", async (DateTime? date, AppointmentStatus? status, int? vetId, int? petId, int page, int pageSize, IAppointmentService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            return Results.Ok(await service.GetAllAsync(date, status, vetId, petId, page, pageSize, ct));
        })
        .WithName("GetAppointments")
        .WithSummary("List appointments")
        .WithDescription("Returns a paginated list of appointments. Filter by date, status, veterinarian, or pet.")
        .Produces<PaginatedResponse<AppointmentResponse>>();

        group.MapGet("/today", async (IAppointmentService service, CancellationToken ct) =>
            Results.Ok(await service.GetTodayAsync(ct)))
        .WithName("GetTodaysAppointments")
        .WithSummary("Get today's appointments")
        .WithDescription("Returns all appointments scheduled for today, ordered by time.")
        .Produces<List<AppointmentResponse>>();

        group.MapGet("/{id:int}", async (int id, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.GetByIdAsync(id, ct);
            return appointment is null ? Results.NotFound() : Results.Ok(appointment);
        })
        .WithName("GetAppointmentById")
        .WithSummary("Get appointment details")
        .WithDescription("Returns full appointment details including pet, veterinarian, owner, and medical record.")
        .Produces<AppointmentDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.CreateAsync(request, ct);
            return Results.Created($"/api/appointments/{appointment.Id}", appointment);
        })
        .WithName("CreateAppointment")
        .WithSummary("Schedule an appointment")
        .WithDescription("Schedules a new appointment. Validates scheduling conflicts and requires a future date.")
        .Produces<AppointmentResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateAsync(id, request, ct);
            return appointment is null ? Results.NotFound() : Results.Ok(appointment);
        })
        .WithName("UpdateAppointment")
        .WithSummary("Update an appointment")
        .WithDescription("Updates appointment details. Re-validates scheduling conflicts.")
        .Produces<AppointmentResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:int}/status", async (int id, UpdateAppointmentStatusRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateStatusAsync(id, request, ct);
            return appointment is null ? Results.NotFound() : Results.Ok(appointment);
        })
        .WithName("UpdateAppointmentStatus")
        .WithSummary("Update appointment status")
        .WithDescription("Updates the status following the workflow: Scheduled→CheckedIn/Cancelled/NoShow, CheckedIn→InProgress/Cancelled, InProgress→Completed.")
        .Produces<AppointmentResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
