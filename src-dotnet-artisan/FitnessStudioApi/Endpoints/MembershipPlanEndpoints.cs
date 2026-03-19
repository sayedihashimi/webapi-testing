using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipPlanEndpoints
{
    public static RouteGroupBuilder MapMembershipPlanEndpoints(this RouteGroupBuilder group)
    {
        var plans = group.MapGroup("/membership-plans").WithTags("Membership Plans");

        plans.MapGet("/", GetAllAsync)
            .WithSummary("List all active membership plans");

        plans.MapGet("/{id:int}", GetByIdAsync)
            .WithSummary("Get membership plan details");

        plans.MapPost("/", CreateAsync)
            .WithSummary("Create a new membership plan");

        plans.MapPut("/{id:int}", UpdateAsync)
            .WithSummary("Update a membership plan");

        plans.MapDelete("/{id:int}", DeleteAsync)
            .WithSummary("Deactivate a membership plan");

        return group;
    }

    private static async Task<Ok<IReadOnlyList<MembershipPlanResponse>>> GetAllAsync(
        IMembershipPlanService service, CancellationToken ct)
    {
        var plans = await service.GetAllActiveAsync(ct);
        return TypedResults.Ok(plans);
    }

    private static async Task<Results<Ok<MembershipPlanResponse>, NotFound>> GetByIdAsync(
        int id, IMembershipPlanService service, CancellationToken ct)
    {
        var plan = await service.GetByIdAsync(id, ct);
        return plan is not null
            ? TypedResults.Ok(plan)
            : TypedResults.NotFound();
    }

    private static async Task<Created<MembershipPlanResponse>> CreateAsync(
        CreateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct)
    {
        var plan = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/membership-plans/{plan.Id}", plan);
    }

    private static async Task<Results<Ok<MembershipPlanResponse>, NotFound>> UpdateAsync(
        int id, UpdateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct)
    {
        var plan = await service.UpdateAsync(id, request, ct);
        return plan is not null
            ? TypedResults.Ok(plan)
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, IMembershipPlanService service, CancellationToken ct)
    {
        var result = await service.DeactivateAsync(id, ct);
        return result ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
