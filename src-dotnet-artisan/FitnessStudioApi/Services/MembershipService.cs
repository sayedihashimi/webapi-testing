using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService(StudioDbContext db)
{
    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new InvalidOperationException("Member not found.");

        var plan = await db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new InvalidOperationException("Membership plan not found.");

        if (!plan.IsActive)
        {
            throw new InvalidOperationException("Membership plan is not active.");
        }

        var hasActiveOrFrozen = await db.Memberships
            .AnyAsync(ms => ms.MemberId == request.MemberId &&
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
            PaymentStatus = Enum.TryParse<PaymentStatus>(request.PaymentStatus, true, out var ps) ? ps : PaymentStatus.Pending
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);

        membership.Member = member;
        membership.MembershipPlan = plan;
        return ToResponse(membership);
    }

    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct);

        return membership is null ? null : ToResponse(membership);
    }

    public async Task<string?> CancelAsync(int id, CancellationToken ct = default)
    {
        var membership = await db.Memberships.FindAsync([id], ct);
        if (membership is null)
        {
            return "not_found";
        }

        if (membership.Status is not (MembershipStatus.Active or MembershipStatus.Frozen))
        {
            return "invalid_status";
        }

        membership.Status = MembershipStatus.Cancelled;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct = default)
    {
        var membership = await db.Memberships.FindAsync([id], ct);
        if (membership is null)
        {
            return "not_found";
        }

        if (membership.Status != MembershipStatus.Active)
        {
            return "not_active";
        }

        if (membership.FreezeStartDate.HasValue)
        {
            return "already_frozen_once";
        }

        var freezeStart = DateOnly.FromDateTime(DateTime.UtcNow);
        var freezeEnd = freezeStart.AddDays(request.DurationDays);

        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = freezeStart;
        membership.FreezeEndDate = freezeEnd;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> UnfreezeAsync(int id, CancellationToken ct = default)
    {
        var membership = await db.Memberships.FindAsync([id], ct);
        if (membership is null)
        {
            return "not_found";
        }

        if (membership.Status != MembershipStatus.Frozen)
        {
            return "not_frozen";
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var frozenDays = membership.FreezeStartDate.HasValue
            ? today.DayNumber - membership.FreezeStartDate.Value.DayNumber
            : 0;

        membership.Status = MembershipStatus.Active;
        membership.EndDate = membership.EndDate.AddDays(frozenDays);
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task<(string? Error, MembershipResponse? Result)> RenewAsync(int id, CancellationToken ct = default)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct);

        if (membership is null)
        {
            return ("not_found", null);
        }

        if (membership.Status != MembershipStatus.Expired)
        {
            return ("not_expired", null);
        }

        var hasActiveOrFrozen = await db.Memberships
            .AnyAsync(ms => ms.MemberId == membership.MemberId && ms.Id != id &&
                           (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        if (hasActiveOrFrozen)
        {
            return ("has_active", null);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var newMembership = new Membership
        {
            MemberId = membership.MemberId,
            MembershipPlanId = membership.MembershipPlanId,
            StartDate = today,
            EndDate = today.AddMonths(membership.MembershipPlan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Pending
        };

        db.Memberships.Add(newMembership);
        await db.SaveChangesAsync(ct);

        newMembership.Member = membership.Member;
        newMembership.MembershipPlan = membership.MembershipPlan;
        return (null, ToResponse(newMembership));
    }

    internal static MembershipResponse ToResponse(Membership ms) => new(
        ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
        ms.MembershipPlanId, ms.MembershipPlan.Name,
        ms.StartDate, ms.EndDate, ms.Status.ToString(), ms.PaymentStatus.ToString(),
        ms.FreezeStartDate, ms.FreezeEndDate);
}
