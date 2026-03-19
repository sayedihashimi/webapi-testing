using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService(AppDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    public async Task<(MembershipResponse? Result, string? Error)> CreateAsync(
        CreateMembershipRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct);
        if (member is null)
        {
            return (null, "Member not found");
        }

        var plan = await db.MembershipPlans.FindAsync([request.MembershipPlanId], ct);
        if (plan is null)
        {
            return (null, "Membership plan not found");
        }

        if (!plan.IsActive)
        {
            return (null, "Membership plan is not active");
        }

        var hasActive = await db.Memberships.AnyAsync(ms =>
            ms.MemberId == request.MemberId &&
            (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
        {
            return (null, "Member already has an active or frozen membership");
        }

        if (!Enum.TryParse<PaymentStatus>(request.PaymentStatus, true, out var paymentStatus))
        {
            return (null, "Invalid payment status");
        }

        var membership = new Membership
        {
            MemberId = request.MemberId,
            MembershipPlanId = request.MembershipPlanId,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = paymentStatus
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created membership {MembershipId} for member {MemberId} on plan {PlanName}",
            membership.Id, membership.MemberId, plan.Name);

        // Reload with navigation properties
        await db.Entry(membership).Reference(m => m.Member).LoadAsync(ct);
        await db.Entry(membership).Reference(m => m.MembershipPlan).LoadAsync(ct);

        return (MapToResponse(membership), null);
    }

    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct);

        return membership is null ? null : MapToResponse(membership);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships.FindAsync([id], ct);
        if (membership is null)
        {
            return (false, "Membership not found");
        }

        if (membership.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
        {
            return (false, "Membership is already cancelled or expired");
        }

        membership.Status = MembershipStatus.Cancelled;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled membership {MembershipId}", id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var membership = await db.Memberships.FindAsync([id], ct);
        if (membership is null)
        {
            return (false, "Membership not found");
        }

        if (membership.Status is not MembershipStatus.Active)
        {
            return (false, "Only active memberships can be frozen");
        }

        if (request.FreezeDurationDays < 7 || request.FreezeDurationDays > 30)
        {
            return (false, "Freeze duration must be between 7 and 30 days");
        }

        if (membership.FreezeStartDate.HasValue)
        {
            return (false, "This membership has already been frozen once this term");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = today;
        membership.FreezeEndDate = today.AddDays(request.FreezeDurationDays);
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Froze membership {MembershipId} for {Days} days", id, request.FreezeDurationDays);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UnfreezeAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships.FindAsync([id], ct);
        if (membership is null)
        {
            return (false, "Membership not found");
        }

        if (membership.Status is not MembershipStatus.Frozen)
        {
            return (false, "Membership is not frozen");
        }

        if (membership.FreezeStartDate.HasValue && membership.FreezeEndDate.HasValue)
        {
            var freezeDuration = membership.FreezeEndDate.Value.DayNumber - membership.FreezeStartDate.Value.DayNumber;
            membership.EndDate = membership.EndDate.AddDays(freezeDuration);
        }

        membership.Status = MembershipStatus.Active;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Unfroze membership {MembershipId}, end date extended to {EndDate}", id, membership.EndDate);
        return (true, null);
    }

    public async Task<(MembershipResponse? Result, string? Error)> RenewAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct);

        if (membership is null)
        {
            return (null, "Membership not found");
        }

        if (membership.Status is not MembershipStatus.Expired)
        {
            return (null, "Only expired memberships can be renewed");
        }

        var hasActive = await db.Memberships.AnyAsync(ms =>
            ms.MemberId == membership.MemberId &&
            ms.Id != id &&
            (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
        {
            return (null, "Member already has an active or frozen membership");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var newMembership = new Membership
        {
            MemberId = membership.MemberId,
            MembershipPlanId = membership.MembershipPlanId,
            StartDate = today,
            EndDate = today.AddMonths(membership.MembershipPlan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid
        };

        db.Memberships.Add(newMembership);
        await db.SaveChangesAsync(ct);

        await db.Entry(newMembership).Reference(m => m.Member).LoadAsync(ct);
        await db.Entry(newMembership).Reference(m => m.MembershipPlan).LoadAsync(ct);

        logger.LogInformation("Renewed membership for member {MemberId}, new membership {MembershipId}",
            membership.MemberId, newMembership.Id);

        return (MapToResponse(newMembership), null);
    }

    private static MembershipResponse MapToResponse(Membership ms) =>
        new(ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
            ms.MembershipPlanId, ms.MembershipPlan.Name, ms.StartDate, ms.EndDate,
            ms.Status.ToString(), ms.PaymentStatus.ToString(),
            ms.FreezeStartDate, ms.FreezeEndDate);
}
