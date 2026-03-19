using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static RouteGroupBuilder MapMemberEndpoints(this RouteGroupBuilder group)
    {
        var members = group.MapGroup("/members").WithTags("Members");

        members.MapGet("/", GetAllAsync)
            .WithSummary("List members with search, filter, and pagination");

        members.MapGet("/{id:int}", GetByIdAsync)
            .WithSummary("Get member details with active membership and booking stats");

        members.MapPost("/", CreateAsync)
            .WithSummary("Register a new member");

        members.MapPut("/{id:int}", UpdateAsync)
            .WithSummary("Update member profile");

        members.MapDelete("/{id:int}", DeleteAsync)
            .WithSummary("Deactivate a member");

        members.MapGet("/{id:int}/bookings", GetBookingsAsync)
            .WithSummary("Get member's bookings with filters");

        members.MapGet("/{id:int}/bookings/upcoming", GetUpcomingBookingsAsync)
            .WithSummary("Get member's upcoming confirmed bookings");

        members.MapGet("/{id:int}/memberships", GetMembershipHistoryAsync)
            .WithSummary("Get membership history for a member");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<MemberResponse>>> GetAllAsync(
        IMemberService service,
        string? search, bool? isActive, int page = 1, int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(search, isActive, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<MemberDetailResponse>, NotFound>> GetByIdAsync(
        int id, IMemberService service, CancellationToken ct)
    {
        var member = await service.GetByIdAsync(id, ct);
        return member is not null
            ? TypedResults.Ok(member)
            : TypedResults.NotFound();
    }

    private static async Task<Created<MemberResponse>> CreateAsync(
        CreateMemberRequest request, IMemberService service, CancellationToken ct)
    {
        var member = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/members/{member.Id}", member);
    }

    private static async Task<Results<Ok<MemberResponse>, NotFound>> UpdateAsync(
        int id, UpdateMemberRequest request, IMemberService service, CancellationToken ct)
    {
        var member = await service.UpdateAsync(id, request, ct);
        return member is not null
            ? TypedResults.Ok(member)
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>>> DeleteAsync(
        int id, IMemberService service, CancellationToken ct)
    {
        var (success, error) = await service.DeactivateAsync(id, ct);
        if (success)
        {
            return TypedResults.NoContent();
        }

        return error == "Member not found"
            ? TypedResults.NotFound()
            : TypedResults.Conflict(error);
    }

    private static async Task<Ok<PaginatedResponse<BookingResponse>>> GetBookingsAsync(
        int id, IMemberService service,
        string? status, DateOnly? fromDate, DateOnly? toDate,
        int page = 1, int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetMemberBookingsAsync(id, status, fromDate, toDate, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyList<BookingResponse>>> GetUpcomingBookingsAsync(
        int id, IMemberService service, CancellationToken ct)
    {
        var bookings = await service.GetUpcomingBookingsAsync(id, ct);
        return TypedResults.Ok(bookings);
    }

    private static async Task<Ok<IReadOnlyList<MembershipResponse>>> GetMembershipHistoryAsync(
        int id, IMemberService service, CancellationToken ct)
    {
        var memberships = await service.GetMembershipHistoryAsync(id, ct);
        return TypedResults.Ok(memberships);
    }
}
