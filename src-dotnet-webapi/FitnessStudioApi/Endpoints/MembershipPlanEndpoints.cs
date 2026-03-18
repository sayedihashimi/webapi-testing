using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class MembershipPlanEndpoints
{
    public static void MapMembershipPlanEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/membership-plans").WithTags("Membership Plans");

        group.MapGet("/", async (IMembershipPlanService service, CancellationToken ct) =>
        {
            var plans = await service.GetAllActiveAsync(ct);
            return Results.Ok(plans);
        })
        .WithName("GetMembershipPlans")
        .WithSummary("List all active membership plans")
        .WithDescription("Returns all active membership plans available at Zenith Fitness Studio.")
        .Produces<List<MembershipPlanResponse>>(StatusCodes.Status200OK);

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
            return TypedResults.Created($"/api/membership-plans/{plan.Id}", plan);
        })
        .WithName("CreateMembershipPlan")
        .WithSummary("Create a new membership plan")
        .WithDescription("Creates a new membership plan. Name must be unique.")
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
        .WithDescription("Updates an existing membership plan. Name must remain unique.")
        .Produces<MembershipPlanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

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
