using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members");

        group.MapGet("/", async (
            string? search, bool? isActive, int? page, int? pageSize,
            IMemberService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, isActive, page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMembers")
        .WithSummary("List members with search, filter, and pagination");

        group.MapGet("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.GetByIdAsync(id, ct);
            return member is null ? TypedResults.NotFound() : TypedResults.Ok(member);
        })
        .WithName("GetMemberById")
        .WithSummary("Get a member by ID");

        group.MapPost("/", async (CreateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/members/{member.Id}", member);
        })
        .WithName("CreateMember")
        .WithSummary("Register a new member");

        group.MapPut("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound>> (
            int id, UpdateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(member);
        })
        .WithName("UpdateMember")
        .WithSummary("Update a member's information");

        group.MapDelete("/{id:int}", async (int id, IMemberService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteMember")
        .WithSummary("Deactivate a member (fails if future bookings exist)");

        group.MapGet("/{id:int}/bookings", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var bookings = await service.GetBookingsAsync(id, ct);
            return TypedResults.Ok(bookings);
        })
        .WithName("GetMemberBookings")
        .WithSummary("Get all bookings for a member");

        group.MapGet("/{id:int}/bookings/upcoming", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var bookings = await service.GetUpcomingBookingsAsync(id, ct);
            return TypedResults.Ok(bookings);
        })
        .WithName("GetMemberUpcomingBookings")
        .WithSummary("Get upcoming confirmed bookings for a member");

        group.MapGet("/{id:int}/memberships", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var memberships = await service.GetMembershipsAsync(id, ct);
            return TypedResults.Ok(memberships);
        })
        .WithName("GetMemberMemberships")
        .WithSummary("Get membership history for a member");
    }
}
