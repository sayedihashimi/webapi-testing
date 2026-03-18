using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipEndpoints
{
    public static RouteGroupBuilder MapMembershipEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/memberships").WithTags("Memberships");

        group.MapPost("/", async Task<Results<Created<MembershipResponse>, Conflict<string>>> (CreateMembershipRequest request, MembershipService service, CancellationToken ct) =>
        {
            try
            {
                var membership = await service.CreateAsync(request, ct);
                return TypedResults.Created($"/api/memberships/{membership.Id}", membership);
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapGet("/{id:int}", async Task<Results<Ok<MembershipResponse>, NotFound<string>>> (int id, MembershipService service, CancellationToken ct) =>
        {
            var membership = await service.GetByIdAsync(id, ct);
            return membership is not null
                ? TypedResults.Ok(membership)
                : TypedResults.NotFound("Membership not found.");
        });

        group.MapPost("/{id:int}/cancel", async Task<Results<Ok<string>, NotFound<string>, Conflict<string>>> (int id, MembershipService service, CancellationToken ct) =>
        {
            var result = await service.CancelAsync(id, ct);
            return result switch
            {
                null => TypedResults.Ok("Membership cancelled."),
                "not_found" => TypedResults.NotFound("Membership not found."),
                _ => TypedResults.Conflict("Only active or frozen memberships can be cancelled.")
            };
        });

        group.MapPost("/{id:int}/freeze", async Task<Results<Ok<string>, NotFound<string>, Conflict<string>>> (int id, FreezeMembershipRequest request, MembershipService service, CancellationToken ct) =>
        {
            var result = await service.FreezeAsync(id, request, ct);
            return result switch
            {
                null => TypedResults.Ok("Membership frozen."),
                "not_found" => TypedResults.NotFound("Membership not found."),
                "not_active" => TypedResults.Conflict("Only active memberships can be frozen."),
                _ => TypedResults.Conflict("Membership has already been frozen once this term.")
            };
        });

        group.MapPost("/{id:int}/unfreeze", async Task<Results<Ok<string>, NotFound<string>, Conflict<string>>> (int id, MembershipService service, CancellationToken ct) =>
        {
            var result = await service.UnfreezeAsync(id, ct);
            return result switch
            {
                null => TypedResults.Ok("Membership unfrozen. End date extended."),
                "not_found" => TypedResults.NotFound("Membership not found."),
                _ => TypedResults.Conflict("Membership is not frozen.")
            };
        });

        group.MapPost("/{id:int}/renew", async Task<Results<Created<MembershipResponse>, NotFound<string>, Conflict<string>>> (int id, MembershipService service, CancellationToken ct) =>
        {
            var (error, result) = await service.RenewAsync(id, ct);
            return error switch
            {
                null => TypedResults.Created($"/api/memberships/{result!.Id}", result),
                "not_found" => TypedResults.NotFound("Membership not found."),
                "not_expired" => TypedResults.Conflict("Only expired memberships can be renewed."),
                _ => TypedResults.Conflict("Member already has an active membership.")
            };
        });

        return group;
    }
}
