using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class MembershipPlanEndpoints
{
    public static RouteGroupBuilder MapMembershipPlanEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/membership-plans")
            .WithTags("Membership Plans");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeactivateAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        IMembershipPlanService service, CancellationToken ct)
    {
        var plans = await service.GetAllActivePlansAsync(ct);
        return TypedResults.Ok(plans);
    }

    private static async Task<IResult> GetByIdAsync(
        int id, IMembershipPlanService service, CancellationToken ct)
    {
        var plan = await service.GetByIdAsync(id, ct);
        return plan is not null
            ? TypedResults.Ok(plan)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct)
    {
        try
        {
            var plan = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/membership-plans/{plan.Id}", plan);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateAsync(
        int id, UpdateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct)
    {
        try
        {
            var plan = await service.UpdateAsync(id, request, ct);
            return plan is not null
                ? TypedResults.Ok(plan)
                : TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeactivateAsync(
        int id, IMembershipPlanService service, CancellationToken ct)
    {
        var result = await service.DeactivateAsync(id, ct);
        return result
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}
