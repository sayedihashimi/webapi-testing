using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static RouteGroupBuilder MapInstructorEndpoints(this RouteGroupBuilder group)
    {
        var instructors = group.MapGroup("/instructors").WithTags("Instructors");

        instructors.MapGet("/", GetAllAsync)
            .WithSummary("List instructors with optional filters");

        instructors.MapGet("/{id:int}", GetByIdAsync)
            .WithSummary("Get instructor details");

        instructors.MapPost("/", CreateAsync)
            .WithSummary("Create a new instructor");

        instructors.MapPut("/{id:int}", UpdateAsync)
            .WithSummary("Update instructor profile");

        instructors.MapGet("/{id:int}/schedule", GetScheduleAsync)
            .WithSummary("Get instructor's class schedule");

        return group;
    }

    private static async Task<Ok<IReadOnlyList<InstructorResponse>>> GetAllAsync(
        IInstructorService service,
        string? specialization, bool? isActive,
        CancellationToken ct)
    {
        var instructors = await service.GetAllAsync(specialization, isActive, ct);
        return TypedResults.Ok(instructors);
    }

    private static async Task<Results<Ok<InstructorResponse>, NotFound>> GetByIdAsync(
        int id, IInstructorService service, CancellationToken ct)
    {
        var instructor = await service.GetByIdAsync(id, ct);
        return instructor is not null
            ? TypedResults.Ok(instructor)
            : TypedResults.NotFound();
    }

    private static async Task<Created<InstructorResponse>> CreateAsync(
        CreateInstructorRequest request, IInstructorService service, CancellationToken ct)
    {
        var instructor = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/instructors/{instructor.Id}", instructor);
    }

    private static async Task<Results<Ok<InstructorResponse>, NotFound>> UpdateAsync(
        int id, UpdateInstructorRequest request, IInstructorService service, CancellationToken ct)
    {
        var instructor = await service.UpdateAsync(id, request, ct);
        return instructor is not null
            ? TypedResults.Ok(instructor)
            : TypedResults.NotFound();
    }

    private static async Task<Ok<IReadOnlyList<ClassScheduleResponse>>> GetScheduleAsync(
        int id, IInstructorService service,
        DateOnly? fromDate, DateOnly? toDate,
        CancellationToken ct)
    {
        var schedule = await service.GetScheduleAsync(id, fromDate, toDate, ct);
        return TypedResults.Ok(schedule);
    }
}
