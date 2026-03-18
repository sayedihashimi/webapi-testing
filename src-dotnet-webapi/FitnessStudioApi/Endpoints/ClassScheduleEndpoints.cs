using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static void MapClassScheduleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/classes").WithTags("Class Schedules");

        group.MapGet("/", async (DateOnly? date, int? classTypeId, int? instructorId, bool? available, int page = 1, int pageSize = 20, IClassScheduleService service = default!, CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var result = await service.GetAllAsync(date, classTypeId, instructorId, available, page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithName("GetClassSchedules")
        .WithSummary("List class schedules")
        .WithDescription("Returns paginated class schedules. Filter by date, class type, instructor, and availability.")
        .Produces<PagedResponse<ClassScheduleResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.GetByIdAsync(id, ct);
            return schedule is null ? Results.NotFound() : Results.Ok(schedule);
        })
        .WithName("GetClassScheduleById")
        .WithSummary("Get class schedule details")
        .WithDescription("Returns details of a specific class schedule including enrollment and waitlist counts.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/classes/{schedule.Id}", schedule);
        })
        .WithName("CreateClassSchedule")
        .WithSummary("Schedule a new class")
        .WithDescription("Schedules a new class. Validates instructor availability to prevent scheduling conflicts.")
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
        .WithDescription("Updates an existing class schedule. Re-validates instructor conflicts.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:int}/cancel", async (int id, CancelClassRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CancelAsync(id, request, ct);
            return Results.Ok(schedule);
        })
        .WithName("CancelClassSchedule")
        .WithSummary("Cancel a class")
        .WithDescription("Cancels a class and automatically cancels all associated bookings with reason 'Class cancelled by studio'.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/roster", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var roster = await service.GetRosterAsync(id, ct);
            return Results.Ok(roster);
        })
        .WithName("GetClassRoster")
        .WithSummary("Get class roster")
        .WithDescription("Returns the list of confirmed and attended members for a class.")
        .Produces<List<ClassRosterEntry>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/waitlist", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var waitlist = await service.GetWaitlistAsync(id, ct);
            return Results.Ok(waitlist);
        })
        .WithName("GetClassWaitlist")
        .WithSummary("Get class waitlist")
        .WithDescription("Returns the waitlisted members for a class ordered by position.")
        .Produces<List<WaitlistEntry>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/available", async (IClassScheduleService service, CancellationToken ct) =>
        {
            var result = await service.GetAvailableAsync(ct);
            return Results.Ok(result);
        })
        .WithName("GetAvailableClasses")
        .WithSummary("Get available classes")
        .WithDescription("Returns classes with available spots in the next 7 days.")
        .Produces<List<ClassScheduleResponse>>(StatusCodes.Status200OK);
    }
}
