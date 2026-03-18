using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class MembershipEndpoints
{
    public static void MapMembershipEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/memberships").WithTags("Memberships");

        group.MapPost("/", async (CreateMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/memberships/{membership.Id}", membership);
        })
        .WithName("CreateMembership")
        .WithSummary("Create a new membership")
        .WithDescription("Creates a membership for a member with a specific plan. Member can only have one active/frozen membership at a time.")
        .Produces<MembershipResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.GetByIdAsync(id, ct);
            return membership is null ? Results.NotFound() : Results.Ok(membership);
        })
        .WithName("GetMembershipById")
        .WithSummary("Get membership details")
        .WithDescription("Returns the details of a specific membership.")
        .Produces<MembershipResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/cancel", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CancelAsync(id, ct);
            return Results.Ok(membership);
        })
        .WithName("CancelMembership")
        .WithSummary("Cancel a membership")
        .WithDescription("Cancels an active or frozen membership.")
        .Produces<MembershipResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/freeze", async (int id, FreezeMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.FreezeAsync(id, request, ct);
            return Results.Ok(membership);
        })
        .WithName("FreezeMembership")
        .WithSummary("Freeze a membership")
        .WithDescription("Freezes an active membership for 7-30 days. Only one freeze per membership term.")
        .Produces<MembershipResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/unfreeze", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.UnfreezeAsync(id, ct);
            return Results.Ok(membership);
        })
        .WithName("UnfreezeMembership")
        .WithSummary("Unfreeze a membership")
        .WithDescription("Unfreezes a frozen membership and extends the end date by the freeze duration.")
        .Produces<MembershipResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/renew", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.RenewAsync(id, ct);
            return Results.Ok(membership);
        })
        .WithName("RenewMembership")
        .WithSummary("Renew an expired membership")
        .WithDescription("Renews an expired membership with a new start date of today.")
        .Produces<MembershipResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
