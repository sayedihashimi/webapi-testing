using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class MembershipEndpoints
{
    public static RouteGroupBuilder MapMembershipEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/memberships")
            .WithTags("Memberships");

        group.MapPost("/", CreateAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/{id:int}/cancel", CancelAsync);
        group.MapPost("/{id:int}/freeze", FreezeAsync);
        group.MapPost("/{id:int}/unfreeze", UnfreezeAsync);
        group.MapPost("/{id:int}/renew", RenewAsync);

        return group;
    }

    private static async Task<IResult> CreateAsync(
        CreateMembershipRequest request, IMembershipService service, CancellationToken ct)
    {
        try
        {
            var membership = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/memberships/{membership.Id}", membership);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetByIdAsync(
        int id, IMembershipService service, CancellationToken ct)
    {
        var membership = await service.GetByIdAsync(id, ct);
        return membership is not null
            ? TypedResults.Ok(membership)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CancelAsync(
        int id, IMembershipService service, CancellationToken ct)
    {
        try
        {
            var membership = await service.CancelAsync(id, ct);
            return TypedResults.Ok(membership);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> FreezeAsync(
        int id, FreezeMembershipRequest request, IMembershipService service, CancellationToken ct)
    {
        try
        {
            var membership = await service.FreezeAsync(id, request, ct);
            return TypedResults.Ok(membership);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UnfreezeAsync(
        int id, IMembershipService service, CancellationToken ct)
    {
        try
        {
            var membership = await service.UnfreezeAsync(id, ct);
            return TypedResults.Ok(membership);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> RenewAsync(
        int id, IMembershipService service, CancellationToken ct)
    {
        try
        {
            var membership = await service.RenewAsync(id, ct);
            return TypedResults.Ok(membership);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }
}
