using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipEndpoints
{
    public static void MapMembershipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/memberships").WithTags("Memberships");

        group.MapPost("/", async Task<Created<MembershipResponse>> (
            CreateMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/memberships/{membership.Id}", membership);
        })
        .WithName("CreateMembership")
        .WithSummary("Create a new membership")
        .WithDescription("Purchases a membership for a member. Member can only have one active/frozen membership at a time.")
        .Produces<MembershipResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}", async Task<Results<Ok<MembershipResponse>, NotFound>> (
            int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.GetByIdAsync(id, ct);
            return membership is null ? TypedResults.NotFound() : TypedResults.Ok(membership);
        })
        .WithName("GetMembershipById")
        .WithSummary("Get a membership by ID")
        .WithDescription("Returns the full details of a specific membership.")
        .Produces<MembershipResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/cancel", async Task<Results<Ok<MembershipResponse>, NotFound>> (
            int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CancelAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("CancelMembership")
        .WithSummary("Cancel a membership")
        .WithDescription("Cancels an active or frozen membership and issues a refund.")
        .Produces<MembershipResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/freeze", async Task<Results<Ok<MembershipResponse>, NotFound>> (
            int id, FreezeMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.FreezeAsync(id, request, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("FreezeMembership")
        .WithSummary("Freeze a membership")
        .WithDescription("Freezes an active membership for 7-30 days. Only allowed once per membership term.")
        .Produces<MembershipResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/unfreeze", async Task<Results<Ok<MembershipResponse>, NotFound>> (
            int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.UnfreezeAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("UnfreezeMembership")
        .WithSummary("Unfreeze a membership")
        .WithDescription("Unfreezes a frozen membership and extends the end date by the remaining freeze days.")
        .Produces<MembershipResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/renew", async Task<Results<Ok<MembershipResponse>, NotFound>> (
            int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.RenewAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("RenewMembership")
        .WithSummary("Renew an expired membership")
        .WithDescription("Renews an expired membership, starting a new term from today.")
        .Produces<MembershipResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
