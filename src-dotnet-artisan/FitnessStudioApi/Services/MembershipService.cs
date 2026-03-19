using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService(FitnessDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new InvalidOperationException("Member not found.");

        var plan = await db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new InvalidOperationException("Membership plan not found.");

        if (!plan.IsActive)
        {
            throw new InvalidOperationException("Cannot create membership for an inactive plan.");
        }

        var hasActiveOrFrozen = await db.Memberships.AnyAsync(
            ms => ms.MemberId == request.MemberId &&
                  (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        if (hasActiveOrFrozen)
        {
            throw new InvalidOperationException("Member already has an active or frozen membership.");
        }

        var membership = new Membership
        {
            MemberId = request.MemberId,
            MembershipPlanId = request.MembershipPlanId,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = request.PaymentStatus
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created membership {MembershipId} for member {MemberId}", membership.Id, request.MemberId);

        return await GetByIdAsync(membership.Id, ct) ?? throw new InvalidOperationException("Failed to retrieve created membership.");
    }

    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct);

        return membership is null ? null : MapToResponse(membership);
    }

    public async Task<MembershipResponse> CancelAsync(int id, CancellationToken ct = default)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct)
            ?? throw new KeyNotFoundException("Membership not found.");

        if (membership.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
        {
            throw new InvalidOperationException($"Cannot cancel a membership that is {membership.Status}.");
        }

        membership.Status = MembershipStatus.Cancelled;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled membership {MembershipId}", id);

        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct = default)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct)
            ?? throw new KeyNotFoundException("Membership not found.");

        if (membership.Status != MembershipStatus.Active)
        {
            throw new InvalidOperationException("Only active memberships can be frozen.");
        }

        if (membership.FreezeStartDate.HasValue)
        {
            throw new InvalidOperationException("This membership has already been frozen once during this term.");
        }

        var freezeDays = request.FreezeEndDate.DayNumber - request.FreezeStartDate.DayNumber;
        if (freezeDays < 7 || freezeDays > 30)
        {
            throw new InvalidOperationException("Freeze duration must be between 7 and 30 days.");
        }

        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = request.FreezeStartDate;
        membership.FreezeEndDate = request.FreezeEndDate;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Froze membership {MembershipId} from {Start} to {End}", id, request.FreezeStartDate, request.FreezeEndDate);

        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct = default)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct)
            ?? throw new KeyNotFoundException("Membership not found.");

        if (membership.Status != MembershipStatus.Frozen)
        {
            throw new InvalidOperationException("Only frozen memberships can be unfrozen.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var remainingFreezeDays = membership.FreezeEndDate!.Value.DayNumber - today.DayNumber;
        if (remainingFreezeDays > 0)
        {
            membership.EndDate = membership.EndDate.AddDays(remainingFreezeDays);
        }

        membership.Status = MembershipStatus.Active;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Unfroze membership {MembershipId}, extended end date to {EndDate}", id, membership.EndDate);

        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct = default)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct)
            ?? throw new KeyNotFoundException("Membership not found.");

        if (membership.Status is not (MembershipStatus.Active or MembershipStatus.Expired))
        {
            throw new InvalidOperationException($"Cannot renew a membership that is {membership.Status}.");
        }

        var newStartDate = membership.Status == MembershipStatus.Expired
            ? DateOnly.FromDateTime(DateTime.UtcNow)
            : membership.EndDate;

        membership.StartDate = newStartDate;
        membership.EndDate = newStartDate.AddMonths(membership.MembershipPlan.DurationMonths);
        membership.Status = MembershipStatus.Active;
        membership.FreezeStartDate = null;
        membership.FreezeEndDate = null;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Renewed membership {MembershipId}", id);

        return MapToResponse(membership);
    }

    private static MembershipResponse MapToResponse(Membership ms) => new(
        ms.Id,
        ms.MemberId,
        $"{ms.Member.FirstName} {ms.Member.LastName}",
        ms.MembershipPlanId,
        ms.MembershipPlan.Name,
        ms.StartDate,
        ms.EndDate,
        ms.Status,
        ms.PaymentStatus,
        ms.FreezeStartDate,
        ms.FreezeEndDate,
        ms.CreatedAt,
        ms.UpdatedAt);
}
