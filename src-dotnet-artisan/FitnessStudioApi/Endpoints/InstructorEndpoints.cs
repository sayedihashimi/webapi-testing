using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static RouteGroupBuilder MapInstructorEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/instructors").WithTags("Instructors");

        group.MapGet("/", async Task<Ok<List<InstructorResponse>>> (string? specialization, bool? isActive, InstructorService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(specialization, isActive, ct)));

        group.MapGet("/{id:int}", async Task<Results<Ok<InstructorResponse>, NotFound<string>>> (int id, InstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.GetByIdAsync(id, ct);
            return instructor is not null
                ? TypedResults.Ok(instructor)
                : TypedResults.NotFound("Instructor not found.");
        });

        group.MapPost("/", async Task<Results<Created<InstructorResponse>, Conflict<string>>> (CreateInstructorRequest request, InstructorService service, CancellationToken ct) =>
        {
            try
            {
                var instructor = await service.CreateAsync(request, ct);
                return TypedResults.Created($"/api/instructors/{instructor.Id}", instructor);
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapPut("/{id:int}", async Task<Results<Ok<InstructorResponse>, NotFound<string>, Conflict<string>>> (int id, UpdateInstructorRequest request, InstructorService service, CancellationToken ct) =>
        {
            try
            {
                var instructor = await service.UpdateAsync(id, request, ct);
                return instructor is not null
                    ? TypedResults.Ok(instructor)
                    : TypedResults.NotFound("Instructor not found.");
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapGet("/{id:int}/schedule", async Task<Ok<List<ClassScheduleResponse>>> (
            int id, DateTime? from, DateTime? to,
            InstructorService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetScheduleAsync(id, from, to, ct)));

        return group;
    }
}
