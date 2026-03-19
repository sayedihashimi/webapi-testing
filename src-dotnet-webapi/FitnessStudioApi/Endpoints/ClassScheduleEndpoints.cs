using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static void MapClassScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classes")
            .WithTags("Class Schedules");

        group.MapGet("/", async (
            DateOnly? date, int? classTypeId, int? instructorId, bool? available,
            int? page, int? pageSize,
            IClassScheduleService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(date, classTypeId, instructorId, available,
                page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetClassSchedules")
        .WithSummary("List class schedules with filters and pagination");

        group.MapGet("/{id:int}", async Task<Results<Ok<ClassScheduleResponse>, NotFound>> (
            int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.GetByIdAsync(id, ct);
            return schedule is null ? TypedResults.NotFound() : TypedResults.Ok(schedule);
        })
        .WithName("GetClassScheduleById")
        .WithSummary("Get a class schedule by ID");

        group.MapPost("/", async (CreateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/classes/{schedule.Id}", schedule);
        })
        .WithName("CreateClassSchedule")
        .WithSummary("Schedule a new class");

        group.MapPut("/{id:int}", async Task<Results<Ok<ClassScheduleResponse>, NotFound>> (
            int id, UpdateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("UpdateClassSchedule")
        .WithSummary("Update a class schedule");

        group.MapPatch("/{id:int}/cancel", async (
            int id, CancelClassRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("CancelClassSchedule")
        .WithSummary("Cancel a class and all its bookings");

        group.MapGet("/{id:int}/roster", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var roster = await service.GetRosterAsync(id, ct);
            return TypedResults.Ok(roster);
        })
        .WithName("GetClassRoster")
        .WithSummary("Get confirmed members for a class");

        group.MapGet("/{id:int}/waitlist", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var waitlist = await service.GetWaitlistAsync(id, ct);
            return TypedResults.Ok(waitlist);
        })
        .WithName("GetClassWaitlist")
        .WithSummary("Get waitlisted members for a class");

        group.MapGet("/available", async (IClassScheduleService service, CancellationToken ct) =>
        {
            var available = await service.GetAvailableAsync(ct);
            return TypedResults.Ok(available);
        })
        .WithName("GetAvailableClasses")
        .WithSummary("Get classes with available spots in the next 7 days");
    }
}
