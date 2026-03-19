using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static RouteGroupBuilder MapClassScheduleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/classes")
            .WithTags("Class Schedules");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/available", GetAvailableAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapPatch("/{id:int}/cancel", CancelAsync);
        group.MapGet("/{id:int}/roster", GetRosterAsync);
        group.MapGet("/{id:int}/waitlist", GetWaitlistAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        DateOnly? date, int? classTypeId, int? instructorId, bool? hasAvailability,
        int page, int pageSize,
        IClassScheduleService service, CancellationToken ct)
    {
        if (page < 1) { page = 1; }
        if (pageSize < 1 || pageSize > 100) { pageSize = 20; }

        var result = await service.GetAllAsync(date, classTypeId, instructorId, hasAvailability, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetAvailableAsync(
        IClassScheduleService service, CancellationToken ct)
    {
        var classes = await service.GetAvailableClassesAsync(ct);
        return TypedResults.Ok(classes);
    }

    private static async Task<IResult> GetByIdAsync(
        int id, IClassScheduleService service, CancellationToken ct)
    {
        var schedule = await service.GetByIdAsync(id, ct);
        return schedule is not null
            ? TypedResults.Ok(schedule)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct)
    {
        try
        {
            var schedule = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/classes/{schedule.Id}", schedule);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateAsync(
        int id, UpdateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct)
    {
        try
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is not null
                ? TypedResults.Ok(result)
                : TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CancelAsync(
        int id, CancelClassRequest request, IClassScheduleService service, CancellationToken ct)
    {
        try
        {
            var schedule = await service.CancelClassAsync(id, request.Reason, ct);
            return TypedResults.Ok(schedule);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetRosterAsync(
        int id, IClassScheduleService service, CancellationToken ct)
    {
        var roster = await service.GetRosterAsync(id, ct);
        return TypedResults.Ok(roster);
    }

    private static async Task<IResult> GetWaitlistAsync(
        int id, IClassScheduleService service, CancellationToken ct)
    {
        var waitlist = await service.GetWaitlistAsync(id, ct);
        return TypedResults.Ok(waitlist);
    }
}
