using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static RouteGroupBuilder MapAppointmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments");

        group.MapGet("/", async (DateOnly? date, string? status, int? vetId, int? petId, int page, int pageSize, IAppointmentService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            return Results.Ok(await service.GetAllAsync(date, status, vetId, petId, page, pageSize, ct));
        })
        .WithName("GetAppointments")
        .WithSummary("List appointments")
        .WithDescription("Returns a paginated list of appointments with optional filters for date, status, veterinarian, and pet.")
        .Produces<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK);

        group.MapGet("/today", async (IAppointmentService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetTodayAsync(ct));
        })
        .WithName("GetTodayAppointments")
        .WithSummary("Get today's appointments")
        .WithDescription("Returns all appointments scheduled for today, ordered by time.")
        .Produces<List<AppointmentResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.GetByIdAsync(id, ct);
            return appointment is null ? Results.NotFound() : Results.Ok(appointment);
        })
        .WithName("GetAppointmentById")
        .WithSummary("Get an appointment by ID")
        .WithDescription("Returns appointment details including pet, veterinarian, and medical record information.")
        .Produces<AppointmentResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/appointments/{appointment.Id}", appointment);
        })
        .WithName("CreateAppointment")
        .WithSummary("Schedule a new appointment")
        .WithDescription("Creates a new appointment. Validates scheduling conflicts and requires a future date.")
        .Produces<AppointmentResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateAsync(id, request, ct);
            return appointment is null ? Results.NotFound() : Results.Ok(appointment);
        })
        .WithName("UpdateAppointment")
        .WithSummary("Update an appointment")
        .WithDescription("Updates an existing appointment. Cannot update completed, cancelled, or no-show appointments.")
        .Produces<AppointmentResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:int}/status", async (int id, UpdateAppointmentStatusRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateStatusAsync(id, request, ct);
            return appointment is null ? Results.NotFound() : Results.Ok(appointment);
        })
        .WithName("UpdateAppointmentStatus")
        .WithSummary("Update appointment status")
        .WithDescription("Changes appointment status following the workflow: Scheduled → CheckedIn/Cancelled/NoShow, CheckedIn → InProgress/Cancelled, InProgress → Completed.")
        .Produces<AppointmentResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        return group;
    }
}
