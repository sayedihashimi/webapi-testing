using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members").WithTags("Members");

        group.MapGet("/", async Task<Ok<PaginatedResponse<MemberListResponse>>> (
            string? search, bool? isActive, int? page, int? pageSize,
            IMemberService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(search, isActive, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMembers")
        .WithSummary("List members")
        .WithDescription("Returns a paginated list of members. Filter by name, email, or active status.")
        .Produces<PaginatedResponse<MemberListResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.GetByIdAsync(id, ct);
            return member is null ? TypedResults.NotFound() : TypedResults.Ok(member);
        })
        .WithName("GetMemberById")
        .WithSummary("Get a member by ID")
        .WithDescription("Returns full member details including active membership and booking stats.")
        .Produces<MemberResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<MemberResponse>> (
            CreateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/members/{member.Id}", member);
        })
        .WithName("CreateMember")
        .WithSummary("Register a new member")
        .WithDescription("Registers a new member. Must be at least 16 years old. Email must be unique.")
        .Produces<MemberResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound>> (
            int id, UpdateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(member);
        })
        .WithName("UpdateMember")
        .WithSummary("Update a member")
        .WithDescription("Updates an existing member's details.")
        .Produces<MemberResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteMember")
        .WithSummary("Deactivate a member")
        .WithDescription("Deactivates a member. Fails if they have future confirmed bookings.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/bookings", async Task<Ok<PaginatedResponse<BookingResponse>>> (
            int id, string? status, DateOnly? fromDate, DateOnly? toDate,
            int? page, int? pageSize,
            IMemberService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetMemberBookingsAsync(id, status, fromDate, toDate, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMemberBookings")
        .WithSummary("Get member's bookings")
        .WithDescription("Returns a paginated list of a member's bookings. Filter by status and date range.")
        .Produces<PaginatedResponse<BookingResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/bookings/upcoming", async Task<Ok<IReadOnlyList<BookingResponse>>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            var bookings = await service.GetUpcomingBookingsAsync(id, ct);
            return TypedResults.Ok(bookings);
        })
        .WithName("GetMemberUpcomingBookings")
        .WithSummary("Get member's upcoming bookings")
        .WithDescription("Returns all confirmed bookings for future classes.")
        .Produces<IReadOnlyList<BookingResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/memberships", async Task<Ok<IReadOnlyList<MembershipResponse>>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            var memberships = await service.GetMembershipHistoryAsync(id, ct);
            return TypedResults.Ok(memberships);
        })
        .WithName("GetMemberMemberships")
        .WithSummary("Get member's membership history")
        .WithDescription("Returns all memberships for a member, ordered by start date descending.")
        .Produces<IReadOnlyList<MembershipResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
