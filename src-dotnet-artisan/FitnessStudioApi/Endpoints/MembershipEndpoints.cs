using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipEndpoints
{
    public static RouteGroupBuilder MapMembershipEndpoints(this RouteGroupBuilder group)
    {
        var memberships = group.MapGroup("/memberships").WithTags("Memberships");

        memberships.MapPost("/", CreateAsync)
            .WithSummary("Purchase/create a membership for a member");

        memberships.MapGet("/{id:int}", GetByIdAsync)
            .WithSummary("Get membership details");

        memberships.MapPost("/{id:int}/cancel", CancelAsync)
            .WithSummary("Cancel a membership");

        memberships.MapPost("/{id:int}/freeze", FreezeAsync)
            .WithSummary("Freeze a membership");

        memberships.MapPost("/{id:int}/unfreeze", UnfreezeAsync)
            .WithSummary("Unfreeze a membership");

        memberships.MapPost("/{id:int}/renew", RenewAsync)
            .WithSummary("Renew an expired membership");

        return group;
    }

    private static async Task<Results<Created<MembershipResponse>, BadRequest<string>>> CreateAsync(
        CreateMembershipRequest request, IMembershipService service, CancellationToken ct)
    {
        var (result, error) = await service.CreateAsync(request, ct);
        return result is not null
            ? TypedResults.Created($"/api/memberships/{result.Id}", result)
            : TypedResults.BadRequest(error);
    }

    private static async Task<Results<Ok<MembershipResponse>, NotFound>> GetByIdAsync(
        int id, IMembershipService service, CancellationToken ct)
    {
        var membership = await service.GetByIdAsync(id, ct);
        return membership is not null
            ? TypedResults.Ok(membership)
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, BadRequest<string>>> CancelAsync(
        int id, IMembershipService service, CancellationToken ct)
    {
        var (success, error) = await service.CancelAsync(id, ct);
        return success
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(error);
    }

    private static async Task<Results<NoContent, BadRequest<string>>> FreezeAsync(
        int id, FreezeMembershipRequest request, IMembershipService service, CancellationToken ct)
    {
        var (success, error) = await service.FreezeAsync(id, request, ct);
        return success
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(error);
    }

    private static async Task<Results<NoContent, BadRequest<string>>> UnfreezeAsync(
        int id, IMembershipService service, CancellationToken ct)
    {
        var (success, error) = await service.UnfreezeAsync(id, ct);
        return success
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(error);
    }

    private static async Task<Results<Created<MembershipResponse>, BadRequest<string>>> RenewAsync(
        int id, IMembershipService service, CancellationToken ct)
    {
        var (result, error) = await service.RenewAsync(id, ct);
        return result is not null
            ? TypedResults.Created($"/api/memberships/{result.Id}", result)
            : TypedResults.BadRequest(error);
    }
}
