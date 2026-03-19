using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static void MapInstructorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/instructors")
            .WithTags("Instructors");

        group.MapGet("/", async (string? specialization, bool? isActive, IInstructorService service, CancellationToken ct) =>
        {
            var instructors = await service.GetAllAsync(specialization, isActive, ct);
            return TypedResults.Ok(instructors);
        })
        .WithName("GetInstructors")
        .WithSummary("List instructors with optional filters");

        group.MapGet("/{id:int}", async Task<Results<Ok<InstructorResponse>, NotFound>> (
            int id, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.GetByIdAsync(id, ct);
            return instructor is null ? TypedResults.NotFound() : TypedResults.Ok(instructor);
        })
        .WithName("GetInstructorById")
        .WithSummary("Get an instructor by ID");

        group.MapPost("/", async (CreateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/instructors/{instructor.Id}", instructor);
        })
        .WithName("CreateInstructor")
        .WithSummary("Create a new instructor");

        group.MapPut("/{id:int}", async Task<Results<Ok<InstructorResponse>, NotFound>> (
            int id, UpdateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(instructor);
        })
        .WithName("UpdateInstructor")
        .WithSummary("Update an instructor");

        group.MapGet("/{id:int}/schedule", async (
            int id, DateOnly? from, DateOnly? to, IInstructorService service, CancellationToken ct) =>
        {
            var schedule = await service.GetScheduleAsync(id, from, to, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("GetInstructorSchedule")
        .WithSummary("Get an instructor's class schedule");
    }
}
