using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipPlanEndpoints
{
    public static void MapMembershipPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/membership-plans").WithTags("Membership Plans");

        group.MapGet("/", async Task<Ok<IReadOnlyList<MembershipPlanResponse>>> (
            IMembershipPlanService service, CancellationToken ct) =>
        {
            var plans = await service.GetAllActiveAsync(ct);
            return TypedResults.Ok(plans);
        })
        .WithName("GetMembershipPlans")
        .WithSummary("List all active membership plans")
        .WithDescription("Returns all active membership plans ordered by price.")
        .Produces<IReadOnlyList<MembershipPlanResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<MembershipPlanResponse>, NotFound>> (
            int id, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.GetByIdAsync(id, ct);
            return plan is null ? TypedResults.NotFound() : TypedResults.Ok(plan);
        })
        .WithName("GetMembershipPlanById")
        .WithSummary("Get a membership plan by ID")
        .WithDescription("Returns the full details of a specific membership plan.")
        .Produces<MembershipPlanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<MembershipPlanResponse>> (
            CreateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/membership-plans/{plan.Id}", plan);
        })
        .WithName("CreateMembershipPlan")
        .WithSummary("Create a new membership plan")
        .WithDescription("Creates a new membership plan with the given details.")
        .Produces<MembershipPlanResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<MembershipPlanResponse>, NotFound>> (
            int id, UpdateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(plan);
        })
        .WithName("UpdateMembershipPlan")
        .WithSummary("Update a membership plan")
        .WithDescription("Updates an existing membership plan.")
        .Produces<MembershipPlanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IMembershipPlanService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteMembershipPlan")
        .WithSummary("Deactivate a membership plan")
        .WithDescription("Soft-deletes a membership plan by setting IsActive to false.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
