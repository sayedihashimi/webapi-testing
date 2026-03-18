using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static void MapInstructorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/instructors").WithTags("Instructors");

        group.MapGet("/", async (string? specialization, bool? isActive, int? page, int? pageSize, IInstructorService service, CancellationToken ct) =>
        {
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            return Results.Ok(await service.GetAllAsync(specialization, isActive, p, ps, ct));
        })
        .WithName("GetInstructors")
        .WithSummary("List instructors")
        .WithDescription("Returns a paginated list of instructors with optional specialization and active filters.")
        .Produces<PaginatedResponse<InstructorResponse>>(StatusCodes.Status200OK);

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
            return Results.Created($"/api/instructors/{instructor.Id}", instructor);
        })
        .WithName("CreateInstructor")
        .WithSummary("Create a new instructor")
        .WithDescription("Registers a new instructor.")
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
        .WithDescription("Updates an existing instructor's profile.")
        .Produces<InstructorResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/schedule", async (int id, DateOnly? from, DateOnly? to, IInstructorService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetScheduleAsync(id, from, to, ct));
        })
        .WithName("GetInstructorSchedule")
        .WithSummary("Get instructor schedule")
        .WithDescription("Returns class schedules for an instructor with optional date range filter.")
        .Produces<List<ClassScheduleResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
