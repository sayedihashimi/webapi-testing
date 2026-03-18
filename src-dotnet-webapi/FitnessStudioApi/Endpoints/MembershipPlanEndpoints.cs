using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class MembershipPlanEndpoints
{
    public static void MapMembershipPlanEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/membership-plans").WithTags("Membership Plans");

        group.MapGet("/", async (int? page, int? pageSize, IMembershipPlanService service, CancellationToken ct) =>
        {
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            return Results.Ok(await service.GetAllAsync(p, ps, ct));
        })
        .WithName("GetMembershipPlans")
        .WithSummary("List all active membership plans")
        .WithDescription("Returns a paginated list of active membership plans.")
        .Produces<PaginatedResponse<MembershipPlanResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.GetByIdAsync(id, ct);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        })
        .WithName("GetMembershipPlanById")
        .WithSummary("Get a membership plan by ID")
        .WithDescription("Returns the details of a specific membership plan.")
        .Produces<MembershipPlanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.CreateAsync(request, ct);
            return Results.Created($"/api/membership-plans/{plan.Id}", plan);
        })
        .WithName("CreateMembershipPlan")
        .WithSummary("Create a new membership plan")
        .WithDescription("Creates a new membership plan with the specified details.")
        .Produces<MembershipPlanResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.UpdateAsync(id, request, ct);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        })
        .WithName("UpdateMembershipPlan")
        .WithSummary("Update a membership plan")
        .WithDescription("Updates an existing membership plan with the specified details.")
        .Produces<MembershipPlanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async (int id, IMembershipPlanService service, CancellationToken ct) =>
        {
            var result = await service.DeactivateAsync(id, ct);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeactivateMembershipPlan")
        .WithSummary("Deactivate a membership plan")
        .WithDescription("Soft-deletes a membership plan by setting IsActive to false.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
