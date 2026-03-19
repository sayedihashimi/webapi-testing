using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static RouteGroupBuilder MapMemberEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/members")
            .WithTags("Members");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        group.MapGet("/{id:int}/bookings", GetBookingsAsync);
        group.MapGet("/{id:int}/bookings/upcoming", GetUpcomingBookingsAsync);
        group.MapGet("/{id:int}/memberships", GetMembershipsAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        string? search, bool? isActive, int page, int pageSize,
        IMemberService service, CancellationToken ct)
    {
        if (page < 1) { page = 1; }
        if (pageSize < 1 || pageSize > 100) { pageSize = 20; }

        var result = await service.GetAllAsync(search, isActive, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync(
        int id, IMemberService service, CancellationToken ct)
    {
        var member = await service.GetByIdAsync(id, ct);
        return member is not null
            ? TypedResults.Ok(member)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreateMemberRequest request, IMemberService service, CancellationToken ct)
    {
        try
        {
            var member = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/members/{member.Id}", member);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateAsync(
        int id, UpdateMemberRequest request, IMemberService service, CancellationToken ct)
    {
        try
        {
            var member = await service.UpdateAsync(id, request, ct);
            return member is not null
                ? TypedResults.Ok(member)
                : TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteAsync(
        int id, IMemberService service, CancellationToken ct)
    {
        var (success, error) = await service.DeleteAsync(id, ct);
        if (success)
        {
            return TypedResults.NoContent();
        }

        return error == "Member not found."
            ? TypedResults.NotFound()
            : TypedResults.BadRequest(new { error });
    }

    private static async Task<IResult> GetBookingsAsync(
        int id, string? status, int page, int pageSize,
        IMemberService service, CancellationToken ct)
    {
        if (page < 1) { page = 1; }
        if (pageSize < 1 || pageSize > 100) { pageSize = 20; }

        var result = await service.GetMemberBookingsAsync(id, status, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetUpcomingBookingsAsync(
        int id, IMemberService service, CancellationToken ct)
    {
        var bookings = await service.GetUpcomingBookingsAsync(id, ct);
        return TypedResults.Ok(bookings);
    }

    private static async Task<IResult> GetMembershipsAsync(
        int id, IMemberService service, CancellationToken ct)
    {
        var memberships = await service.GetMemberMembershipsAsync(id, ct);
        return TypedResults.Ok(memberships);
    }
}
