using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static void MapClassScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classes").WithTags("Class Schedules");

        group.MapGet("/", async Task<Ok<PaginatedResponse<ClassScheduleResponse>>> (
            DateOnly? fromDate, DateOnly? toDate, int? classTypeId, int? instructorId,
            bool? hasAvailability, int? page, int? pageSize,
            IClassScheduleService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, hasAvailability, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetClassSchedules")
        .WithSummary("List class schedules")
        .WithDescription("Returns a paginated list of class schedules. Filter by date, type, instructor, and availability.")
        .Produces<PaginatedResponse<ClassScheduleResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<ClassScheduleResponse>, NotFound>> (
            int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.GetByIdAsync(id, ct);
            return schedule is null ? TypedResults.NotFound() : TypedResults.Ok(schedule);
        })
        .WithName("GetClassScheduleById")
        .WithSummary("Get a class schedule by ID")
        .WithDescription("Returns the full details of a specific class schedule including enrollment and waitlist counts.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<ClassScheduleResponse>> (
            CreateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/classes/{schedule.Id}", schedule);
        })
        .WithName("CreateClassSchedule")
        .WithSummary("Schedule a new class")
        .WithDescription("Creates a new class schedule. Enforces instructor schedule conflict rules.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<ClassScheduleResponse>, NotFound>> (
            int id, UpdateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("UpdateClassSchedule")
        .WithSummary("Update a class schedule")
        .WithDescription("Updates an existing class schedule. Checks for instructor conflicts.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:int}/cancel", async Task<Results<Ok<ClassScheduleResponse>, NotFound>> (
            int id, CancelClassRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("CancelClassSchedule")
        .WithSummary("Cancel a class")
        .WithDescription("Cancels a class and all associated bookings. Waitlisted bookings are also cancelled.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/roster", async Task<Ok<IReadOnlyList<ClassRosterEntryResponse>>> (
            int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var roster = await service.GetRosterAsync(id, ct);
            return TypedResults.Ok(roster);
        })
        .WithName("GetClassRoster")
        .WithSummary("Get class roster")
        .WithDescription("Returns the list of confirmed and attended members for a class.")
        .Produces<IReadOnlyList<ClassRosterEntryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/waitlist", async Task<Ok<IReadOnlyList<WaitlistEntryResponse>>> (
            int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var waitlist = await service.GetWaitlistAsync(id, ct);
            return TypedResults.Ok(waitlist);
        })
        .WithName("GetClassWaitlist")
        .WithSummary("Get class waitlist")
        .WithDescription("Returns the waitlisted members for a class, ordered by position.")
        .Produces<IReadOnlyList<WaitlistEntryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/available", async Task<Ok<IReadOnlyList<ClassScheduleResponse>>> (
            IClassScheduleService service, CancellationToken ct) =>
        {
            var classes = await service.GetAvailableAsync(ct);
            return TypedResults.Ok(classes);
        })
        .WithName("GetAvailableClasses")
        .WithSummary("Get available classes")
        .WithDescription("Returns classes with available spots in the next 7 days.")
        .Produces<IReadOnlyList<ClassScheduleResponse>>(StatusCodes.Status200OK);
    }
}
