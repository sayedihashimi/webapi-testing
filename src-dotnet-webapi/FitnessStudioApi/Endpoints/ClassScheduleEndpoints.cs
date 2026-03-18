using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static void MapClassScheduleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/classes").WithTags("Class Schedules");

        group.MapGet("/", async (DateOnly? date, int? classTypeId, int? instructorId, bool? available, int? page, int? pageSize, IClassScheduleService service, CancellationToken ct) =>
        {
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            return Results.Ok(await service.GetAllAsync(date, classTypeId, instructorId, available, p, ps, ct));
        })
        .WithName("GetClassSchedules")
        .WithSummary("List class schedules")
        .WithDescription("Returns a paginated list of class schedules with optional date, type, instructor, and availability filters.")
        .Produces<PaginatedResponse<ClassScheduleResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.GetByIdAsync(id, ct);
            return schedule is null ? Results.NotFound() : Results.Ok(schedule);
        })
        .WithName("GetClassScheduleById")
        .WithSummary("Get class schedule details")
        .WithDescription("Returns class schedule details including enrollment count, waitlist, and availability.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CreateAsync(request, ct);
            return Results.Created($"/api/classes/{schedule.Id}", schedule);
        })
        .WithName("CreateClassSchedule")
        .WithSummary("Schedule a new class")
        .WithDescription("Schedules a new class. Validates instructor availability and prevents scheduling conflicts.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.UpdateAsync(id, request, ct);
            return schedule is null ? Results.NotFound() : Results.Ok(schedule);
        })
        .WithName("UpdateClassSchedule")
        .WithSummary("Update a class schedule")
        .WithDescription("Updates an existing class schedule. Validates instructor conflicts if time/instructor changed.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:int}/cancel", async (int id, CancelClassRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CancelAsync(id, request, ct);
            return Results.Ok(schedule);
        })
        .WithName("CancelClassSchedule")
        .WithSummary("Cancel a class")
        .WithDescription("Cancels a class and automatically cancels all associated bookings with a reason.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/roster", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetRosterAsync(id, ct));
        })
        .WithName("GetClassRoster")
        .WithSummary("Get class roster")
        .WithDescription("Returns the list of confirmed/attended members for a class.")
        .Produces<List<ClassRosterResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/waitlist", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetWaitlistAsync(id, ct));
        })
        .WithName("GetClassWaitlist")
        .WithSummary("Get class waitlist")
        .WithDescription("Returns the waitlisted members for a class, ordered by position.")
        .Produces<List<ClassRosterResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/available", async (IClassScheduleService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetAvailableAsync(ct));
        })
        .WithName("GetAvailableClasses")
        .WithSummary("Get available classes")
        .WithDescription("Returns classes with available spots in the next 7 days.")
        .Produces<List<ClassScheduleResponse>>(StatusCodes.Status200OK);
    }
}
