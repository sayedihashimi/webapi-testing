using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static void MapInstructorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/instructors").WithTags("Instructors");

        group.MapGet("/", async Task<Ok<PaginatedResponse<InstructorResponse>>> (
            string? specialization, bool? isActive, int? page, int? pageSize,
            IInstructorService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(specialization, isActive, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetInstructors")
        .WithSummary("List instructors")
        .WithDescription("Returns a paginated list of instructors. Filter by specialization or active status.")
        .Produces<PaginatedResponse<InstructorResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<InstructorResponse>, NotFound>> (
            int id, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.GetByIdAsync(id, ct);
            return instructor is null ? TypedResults.NotFound() : TypedResults.Ok(instructor);
        })
        .WithName("GetInstructorById")
        .WithSummary("Get an instructor by ID")
        .WithDescription("Returns the full details of a specific instructor.")
        .Produces<InstructorResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<InstructorResponse>> (
            CreateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/instructors/{instructor.Id}", instructor);
        })
        .WithName("CreateInstructor")
        .WithSummary("Create a new instructor")
        .WithDescription("Creates a new instructor. Email must be unique.")
        .Produces<InstructorResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<InstructorResponse>, NotFound>> (
            int id, UpdateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(instructor);
        })
        .WithName("UpdateInstructor")
        .WithSummary("Update an instructor")
        .WithDescription("Updates an existing instructor's details.")
        .Produces<InstructorResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/schedule", async Task<Ok<IReadOnlyList<ClassScheduleResponse>>> (
            int id, DateOnly? fromDate, DateOnly? toDate,
            IInstructorService service, CancellationToken ct) =>
        {
            var schedule = await service.GetScheduleAsync(id, fromDate, toDate, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("GetInstructorSchedule")
        .WithSummary("Get instructor's schedule")
        .WithDescription("Returns the class schedule for a specific instructor. Filter by date range.")
        .Produces<IReadOnlyList<ClassScheduleResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
