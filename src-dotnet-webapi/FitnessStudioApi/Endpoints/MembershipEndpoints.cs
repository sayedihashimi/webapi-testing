using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipEndpoints
{
    public static void MapMembershipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/memberships")
            .WithTags("Memberships");

        group.MapPost("/", async (CreateMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/memberships/{membership.Id}", membership);
        })
        .WithName("CreateMembership")
        .WithSummary("Purchase/create a new membership");

        group.MapGet("/{id:int}", async Task<Results<Ok<MembershipResponse>, NotFound>> (
            int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.GetByIdAsync(id, ct);
            return membership is null ? TypedResults.NotFound() : TypedResults.Ok(membership);
        })
        .WithName("GetMembershipById")
        .WithSummary("Get membership details");

        group.MapPost("/{id:int}/cancel", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CancelAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("CancelMembership")
        .WithSummary("Cancel an active or frozen membership");

        group.MapPost("/{id:int}/freeze", async (int id, FreezeMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.FreezeAsync(id, request, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("FreezeMembership")
        .WithSummary("Freeze an active membership (7-30 days)");

        group.MapPost("/{id:int}/unfreeze", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.UnfreezeAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("UnfreezeMembership")
        .WithSummary("Unfreeze a frozen membership and extend end date");

        group.MapPost("/{id:int}/renew", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.RenewAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("RenewMembership")
        .WithSummary("Renew an expired membership");
    }
}
