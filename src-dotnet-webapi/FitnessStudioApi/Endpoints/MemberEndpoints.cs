using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/members").WithTags("Members");

        group.MapGet("/", async (string? search, bool? isActive, int page = 1, int pageSize = 20, IMemberService service = default!, CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var result = await service.GetAllAsync(search, isActive, page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithName("GetMembers")
        .WithSummary("List members")
        .WithDescription("Returns paginated list of members. Supports search by name/email and active status filter.")
        .Produces<PagedResponse<MemberResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.GetByIdAsync(id, ct);
            return member is null ? Results.NotFound() : Results.Ok(member);
        })
        .WithName("GetMemberById")
        .WithSummary("Get member details")
        .WithDescription("Returns detailed member info including active membership and booking statistics.")
        .Produces<MemberDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/members/{member.Id}", member);
        })
        .WithName("CreateMember")
        .WithSummary("Register a new member")
        .WithDescription("Registers a new member. Email must be unique. Must be at least 16 years old.")
        .Produces<MemberResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.UpdateAsync(id, request, ct);
            return member is null ? Results.NotFound() : Results.Ok(member);
        })
        .WithName("UpdateMember")
        .WithSummary("Update a member")
        .WithDescription("Updates member information. Email must remain unique.")
        .Produces<MemberResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var (success, error) = await service.DeactivateAsync(id, ct);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeactivateMember")
        .WithSummary("Deactivate a member")
        .WithDescription("Soft-deletes a member. Fails if member has future confirmed bookings.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/bookings", async (int id, string? status, DateOnly? fromDate, DateOnly? toDate, int page = 1, int pageSize = 20, IMemberService service = default!, CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var result = await service.GetMemberBookingsAsync(id, status, fromDate, toDate, page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithName("GetMemberBookings")
        .WithSummary("Get member bookings")
        .WithDescription("Returns paginated bookings for a member. Filter by status and date range.")
        .Produces<PagedResponse<BookingResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/bookings/upcoming", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.GetUpcomingBookingsAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("GetMemberUpcomingBookings")
        .WithSummary("Get upcoming confirmed bookings")
        .WithDescription("Returns all future confirmed bookings for a member.")
        .Produces<List<BookingResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/memberships", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.GetMemberMembershipsAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("GetMemberMemberships")
        .WithSummary("Get member membership history")
        .WithDescription("Returns all memberships (current and historical) for a member.")
        .Produces<List<MembershipResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
