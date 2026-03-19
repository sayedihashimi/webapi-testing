using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static RouteGroupBuilder MapInstructorEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/instructors")
            .WithTags("Instructors");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapGet("/{id:int}/schedule", GetScheduleAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        string? specialization, bool? isActive,
        IInstructorService service, CancellationToken ct)
    {
        var instructors = await service.GetAllAsync(specialization, isActive, ct);
        return TypedResults.Ok(instructors);
    }

    private static async Task<IResult> GetByIdAsync(
        int id, IInstructorService service, CancellationToken ct)
    {
        var instructor = await service.GetByIdAsync(id, ct);
        return instructor is not null
            ? TypedResults.Ok(instructor)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreateInstructorRequest request, IInstructorService service, CancellationToken ct)
    {
        try
        {
            var instructor = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/instructors/{instructor.Id}", instructor);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateAsync(
        int id, UpdateInstructorRequest request, IInstructorService service, CancellationToken ct)
    {
        try
        {
            var instructor = await service.UpdateAsync(id, request, ct);
            return instructor is not null
                ? TypedResults.Ok(instructor)
                : TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetScheduleAsync(
        int id, DateOnly? from, DateOnly? to,
        IInstructorService service, CancellationToken ct)
    {
        var schedule = await service.GetScheduleAsync(id, from, to, ct);
        return TypedResults.Ok(schedule);
    }
}
