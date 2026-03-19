using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipPlanEndpoints
{
    public static void MapMembershipPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/membership-plans")
            .WithTags("Membership Plans");

        group.MapGet("/", async (IMembershipPlanService service, CancellationToken ct) =>
        {
            var plans = await service.GetAllActiveAsync(ct);
            return TypedResults.Ok(plans);
        })
        .WithName("GetMembershipPlans")
        .WithSummary("List all active membership plans")
        .Produces<IReadOnlyList<MembershipPlanResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<MembershipPlanResponse>, NotFound>> (
            int id, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.GetByIdAsync(id, ct);
            return plan is null ? TypedResults.NotFound() : TypedResults.Ok(plan);
        })
        .WithName("GetMembershipPlanById")
        .WithSummary("Get a membership plan by ID");

        group.MapPost("/", async (CreateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/membership-plans/{plan.Id}", plan);
        })
        .WithName("CreateMembershipPlan")
        .WithSummary("Create a new membership plan");

        group.MapPut("/{id:int}", async Task<Results<Ok<MembershipPlanResponse>, NotFound>> (
            int id, UpdateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(plan);
        })
        .WithName("UpdateMembershipPlan")
        .WithSummary("Update an existing membership plan");

        group.MapDelete("/{id:int}", async (int id, IMembershipPlanService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteMembershipPlan")
        .WithSummary("Deactivate a membership plan");
    }
}
