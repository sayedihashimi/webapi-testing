using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static RouteGroupBuilder MapClassScheduleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/classes").WithTags("Class Schedules");

        group.MapGet("/", async Task<Ok<PagedResponse<ClassScheduleResponse>>> (
            DateTime? from, DateTime? to, int? classTypeId, int? instructorId, bool? available,
            int? page, int? pageSize,
            ClassScheduleService service, CancellationToken ct) =>
        {
            var p = page is null or < 1 ? 1 : page.Value;
            var ps = pageSize is null or < 1 or > 100 ? 20 : pageSize.Value;
            return TypedResults.Ok(await service.GetAllAsync(from, to, classTypeId, instructorId, available, p, ps, ct));
        });

        group.MapGet("/{id:int}", async Task<Results<Ok<ClassScheduleResponse>, NotFound<string>>> (int id, ClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.GetByIdAsync(id, ct);
            return schedule is not null
                ? TypedResults.Ok(schedule)
                : TypedResults.NotFound("Class schedule not found.");
        });

        group.MapPost("/", async Task<Results<Created<ClassScheduleResponse>, Conflict<string>>> (CreateClassScheduleRequest request, ClassScheduleService service, CancellationToken ct) =>
        {
            try
            {
                var schedule = await service.CreateAsync(request, ct);
                return TypedResults.Created($"/api/classes/{schedule.Id}", schedule);
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapPut("/{id:int}", async Task<Results<Ok<ClassScheduleResponse>, NotFound<string>, Conflict<string>>> (int id, UpdateClassScheduleRequest request, ClassScheduleService service, CancellationToken ct) =>
        {
            var (error, result) = await service.UpdateAsync(id, request, ct);
            return error switch
            {
                null => TypedResults.Ok(result!),
                "instructor_conflict" => TypedResults.Conflict("Instructor has a scheduling conflict."),
                _ => TypedResults.NotFound("Class schedule or instructor not found.")
            };
        });

        group.MapPatch("/{id:int}/cancel", async Task<Results<Ok<string>, NotFound<string>, Conflict<string>>> (int id, CancelClassRequest request, ClassScheduleService service, CancellationToken ct) =>
        {
            var result = await service.CancelClassAsync(id, request.Reason, ct);
            return result switch
            {
                null => TypedResults.Ok("Class cancelled and all bookings notified."),
                "not_found" => TypedResults.NotFound("Class schedule not found."),
                _ => TypedResults.Conflict("Only scheduled classes can be cancelled.")
            };
        });

        group.MapGet("/{id:int}/roster", async Task<Ok<List<RosterEntry>>> (int id, ClassScheduleService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetRosterAsync(id, ct)));

        group.MapGet("/{id:int}/waitlist", async Task<Ok<List<RosterEntry>>> (int id, ClassScheduleService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetWaitlistAsync(id, ct)));

        group.MapGet("/available", async Task<Ok<List<ClassScheduleResponse>>> (ClassScheduleService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAvailableAsync(ct)));

        return group;
    }
}
