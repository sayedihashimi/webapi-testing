using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static RouteGroupBuilder MapClassScheduleEndpoints(this RouteGroupBuilder group)
    {
        var classes = group.MapGroup("/classes").WithTags("Class Schedules");

        classes.MapGet("/", GetAllAsync)
            .WithSummary("List scheduled classes with filters and pagination");

        classes.MapGet("/available", GetAvailableAsync)
            .WithSummary("Get classes with available spots in the next 7 days");

        classes.MapGet("/{id:int}", GetByIdAsync)
            .WithSummary("Get class schedule details");

        classes.MapPost("/", CreateAsync)
            .WithSummary("Schedule a new class");

        classes.MapPut("/{id:int}", UpdateAsync)
            .WithSummary("Update class schedule details");

        classes.MapPatch("/{id:int}/cancel", CancelAsync)
            .WithSummary("Cancel a class and all its bookings");

        classes.MapGet("/{id:int}/roster", GetRosterAsync)
            .WithSummary("Get confirmed members for a class");

        classes.MapGet("/{id:int}/waitlist", GetWaitlistAsync)
            .WithSummary("Get the waitlist for a class");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<ClassScheduleResponse>>> GetAllAsync(
        IClassScheduleService service,
        DateOnly? fromDate, DateOnly? toDate,
        int? classTypeId, int? instructorId, bool? available,
        int page = 1, int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, available, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ClassScheduleResponse>, NotFound>> GetByIdAsync(
        int id, IClassScheduleService service, CancellationToken ct)
    {
        var schedule = await service.GetByIdAsync(id, ct);
        return schedule is not null
            ? TypedResults.Ok(schedule)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<ClassScheduleResponse>, BadRequest<string>>> CreateAsync(
        CreateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct)
    {
        var (result, error) = await service.CreateAsync(request, ct);
        return result is not null
            ? TypedResults.Created($"/api/classes/{result.Id}", result)
            : TypedResults.BadRequest(error);
    }

    private static async Task<Results<Ok<ClassScheduleResponse>, BadRequest<string>, NotFound>> UpdateAsync(
        int id, UpdateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct)
    {
        var (result, error) = await service.UpdateAsync(id, request, ct);
        if (result is not null)
        {
            return TypedResults.Ok(result);
        }

        return error == "Class schedule not found"
            ? TypedResults.NotFound()
            : TypedResults.BadRequest(error);
    }

    private static async Task<Results<NoContent, BadRequest<string>, NotFound>> CancelAsync(
        int id, IClassScheduleService service, CancellationToken ct)
    {
        var (success, error) = await service.CancelAsync(id, ct);
        if (success)
        {
            return TypedResults.NoContent();
        }

        return error == "Class schedule not found"
            ? TypedResults.NotFound()
            : TypedResults.BadRequest(error);
    }

    private static async Task<Ok<IReadOnlyList<ClassRosterEntry>>> GetRosterAsync(
        int id, IClassScheduleService service, CancellationToken ct)
    {
        var roster = await service.GetRosterAsync(id, ct);
        return TypedResults.Ok(roster);
    }

    private static async Task<Ok<IReadOnlyList<WaitlistEntry>>> GetWaitlistAsync(
        int id, IClassScheduleService service, CancellationToken ct)
    {
        var waitlist = await service.GetWaitlistAsync(id, ct);
        return TypedResults.Ok(waitlist);
    }

    private static async Task<Ok<IReadOnlyList<ClassScheduleResponse>>> GetAvailableAsync(
        IClassScheduleService service, CancellationToken ct)
    {
        var classes = await service.GetAvailableAsync(ct);
        return TypedResults.Ok(classes);
    }
}
