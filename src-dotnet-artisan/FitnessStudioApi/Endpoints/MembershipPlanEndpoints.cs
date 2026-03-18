using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipPlanEndpoints
{
    public static RouteGroupBuilder MapMembershipPlanEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/membership-plans").WithTags("Membership Plans");

        group.MapGet("/", async Task<Ok<List<MembershipPlanResponse>>> (MembershipPlanService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllActiveAsync(ct)));

        group.MapGet("/{id:int}", async Task<Results<Ok<MembershipPlanResponse>, NotFound<string>>> (int id, MembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.GetByIdAsync(id, ct);
            return plan is not null
                ? TypedResults.Ok(plan)
                : TypedResults.NotFound("Membership plan not found.");
        });

        group.MapPost("/", async Task<Results<Created<MembershipPlanResponse>, Conflict<string>>> (CreateMembershipPlanRequest request, MembershipPlanService service, CancellationToken ct) =>
        {
            try
            {
                var plan = await service.CreateAsync(request, ct);
                return TypedResults.Created($"/api/membership-plans/{plan.Id}", plan);
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapPut("/{id:int}", async Task<Results<Ok<MembershipPlanResponse>, NotFound<string>, Conflict<string>>> (int id, UpdateMembershipPlanRequest request, MembershipPlanService service, CancellationToken ct) =>
        {
            try
            {
                var plan = await service.UpdateAsync(id, request, ct);
                return plan is not null
                    ? TypedResults.Ok(plan)
                    : TypedResults.NotFound("Membership plan not found.");
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound<string>>> (int id, MembershipPlanService service, CancellationToken ct) =>
            await service.DeactivateAsync(id, ct)
                ? TypedResults.NoContent()
                : TypedResults.NotFound("Membership plan not found."));

        return group;
    }
}
