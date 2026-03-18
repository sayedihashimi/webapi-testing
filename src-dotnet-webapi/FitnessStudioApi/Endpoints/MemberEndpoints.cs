using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/members").WithTags("Members");

        group.MapGet("/", async (string? search, bool? isActive, int? page, int? pageSize, IMemberService service, CancellationToken ct) =>
        {
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            return Results.Ok(await service.GetAllAsync(search, isActive, p, ps, ct));
        })
        .WithName("GetMembers")
        .WithSummary("List members")
        .WithDescription("Returns a paginated list of members with optional search and active filter.")
        .Produces<PaginatedResponse<MemberResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.GetByIdAsync(id, ct);
            return member is null ? Results.NotFound() : Results.Ok(member);
        })
        .WithName("GetMemberById")
        .WithSummary("Get member details")
        .WithDescription("Returns detailed member information including active membership and booking stats.")
        .Produces<MemberDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.CreateAsync(request, ct);
            return Results.Created($"/api/members/{member.Id}", member);
        })
        .WithName("CreateMember")
        .WithSummary("Register a new member")
        .WithDescription("Registers a new member. Must be at least 16 years old. Email must be unique.")
        .Produces<MemberResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.UpdateAsync(id, request, ct);
            return member is null ? Results.NotFound() : Results.Ok(member);
        })
        .WithName("UpdateMember")
        .WithSummary("Update member profile")
        .WithDescription("Updates an existing member's profile information.")
        .Produces<MemberResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.DeactivateAsync(id, ct);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeactivateMember")
        .WithSummary("Deactivate a member")
        .WithDescription("Soft-deletes a member. Fails if the member has future bookings.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/bookings", async (int id, string? status, DateOnly? from, DateOnly? to, int? page, int? pageSize, IMemberService service, CancellationToken ct) =>
        {
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            return Results.Ok(await service.GetBookingsAsync(id, status, from, to, p, ps, ct));
        })
        .WithName("GetMemberBookings")
        .WithSummary("Get member bookings")
        .WithDescription("Returns a paginated list of a member's bookings with optional status and date filters.")
        .Produces<PaginatedResponse<BookingResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/bookings/upcoming", async (int id, IMemberService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetUpcomingBookingsAsync(id, ct));
        })
        .WithName("GetMemberUpcomingBookings")
        .WithSummary("Get upcoming confirmed bookings")
        .WithDescription("Returns all upcoming confirmed bookings for a member.")
        .Produces<List<BookingResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/memberships", async (int id, IMemberService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetMembershipsAsync(id, ct));
        })
        .WithName("GetMemberMemberships")
        .WithSummary("Get membership history")
        .WithDescription("Returns all memberships (active, expired, cancelled) for a member.")
        .Produces<List<MembershipResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
