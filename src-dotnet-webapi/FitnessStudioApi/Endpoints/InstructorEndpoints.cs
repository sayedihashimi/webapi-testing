using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static void MapInstructorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/instructors").WithTags("Instructors");

        group.MapGet("/", async (string? specialization, bool? isActive, int page = 1, int pageSize = 20, IInstructorService service = default!, CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var result = await service.GetAllAsync(specialization, isActive, page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithName("GetInstructors")
        .WithSummary("List instructors")
        .WithDescription("Returns paginated list of instructors. Filter by specialization and active status.")
        .Produces<PagedResponse<InstructorResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.GetByIdAsync(id, ct);
            return instructor is null ? Results.NotFound() : Results.Ok(instructor);
        })
        .WithName("GetInstructorById")
        .WithSummary("Get instructor details")
        .WithDescription("Returns the details of a specific instructor.")
        .Produces<InstructorResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/instructors/{instructor.Id}", instructor);
        })
        .WithName("CreateInstructor")
        .WithSummary("Create a new instructor")
        .WithDescription("Registers a new instructor. Email must be unique.")
        .Produces<InstructorResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.UpdateAsync(id, request, ct);
            return instructor is null ? Results.NotFound() : Results.Ok(instructor);
        })
        .WithName("UpdateInstructor")
        .WithSummary("Update an instructor")
        .WithDescription("Updates instructor information.")
        .Produces<InstructorResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/schedule", async (int id, DateOnly? fromDate, DateOnly? toDate, IInstructorService service, CancellationToken ct) =>
        {
            var result = await service.GetScheduleAsync(id, fromDate, toDate, ct);
            return Results.Ok(result);
        })
        .WithName("GetInstructorSchedule")
        .WithSummary("Get instructor schedule")
        .WithDescription("Returns class schedules for an instructor. Filter by date range.")
        .Produces<List<ClassScheduleResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
