using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static RouteGroupBuilder MapMemberEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/members").WithTags("Members");

        group.MapGet("/", async Task<Ok<PagedResponse<MemberResponse>>> (
            string? search, bool? isActive, int? page, int? pageSize,
            MemberService service, CancellationToken ct) =>
        {
            var p = page is null or < 1 ? 1 : page.Value;
            var ps = pageSize is null or < 1 or > 100 ? 20 : pageSize.Value;
            return TypedResults.Ok(await service.GetAllAsync(search, isActive, p, ps, ct));
        });

        group.MapGet("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound<string>>> (int id, MemberService service, CancellationToken ct) =>
        {
            var member = await service.GetByIdAsync(id, ct);
            return member is not null
                ? TypedResults.Ok(member)
                : TypedResults.NotFound("Member not found.");
        });

        group.MapPost("/", async Task<Results<Created<MemberResponse>, Conflict<string>>> (CreateMemberRequest request, MemberService service, CancellationToken ct) =>
        {
            try
            {
                var member = await service.CreateAsync(request, ct);
                return TypedResults.Created($"/api/members/{member.Id}", member);
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapPut("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound<string>, Conflict<string>>> (int id, UpdateMemberRequest request, MemberService service, CancellationToken ct) =>
        {
            try
            {
                var member = await service.UpdateAsync(id, request, ct);
                return member is not null
                    ? TypedResults.Ok(member)
                    : TypedResults.NotFound("Member not found.");
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound<string>, Conflict<string>>> (int id, MemberService service, CancellationToken ct) =>
        {
            var result = await service.DeactivateAsync(id, ct);
            return result switch
            {
                null => TypedResults.NoContent(),
                "has_bookings" => TypedResults.Conflict("Cannot deactivate member with future bookings."),
                _ => TypedResults.NotFound("Member not found.")
            };
        });

        group.MapGet("/{id:int}/bookings", async Task<Ok<PagedResponse<BookingResponse>>> (
            int id, string? status, DateTime? from, DateTime? to,
            int? page, int? pageSize,
            MemberService service, CancellationToken ct) =>
        {
            var p = page is null or < 1 ? 1 : page.Value;
            var ps = pageSize is null or < 1 or > 100 ? 20 : pageSize.Value;
            return TypedResults.Ok(await service.GetBookingsAsync(id, status, from, to, p, ps, ct));
        });

        group.MapGet("/{id:int}/bookings/upcoming", async Task<Ok<List<BookingResponse>>> (int id, MemberService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetUpcomingBookingsAsync(id, ct)));

        group.MapGet("/{id:int}/memberships", async Task<Ok<List<MembershipResponse>>> (int id, MemberService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetMembershipsAsync(id, ct)));

        return group;
    }
}
