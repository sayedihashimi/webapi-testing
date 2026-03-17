using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class MembershipService(FitnessDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    public async Task<MembershipDto?> GetByIdAsync(int id)
    {
        var ms = await db.Memberships
            .Include(m => m.Member).Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id);
        return ms is null ? null : ToDto(ms);
    }

    public async Task<(MembershipDto? Result, string? Error)> CreateAsync(CreateMembershipDto dto)
    {
        var member = await db.Members.FindAsync(dto.MemberId);
        if (member is null) return (null, "Member not found.");

        var plan = await db.MembershipPlans.FindAsync(dto.MembershipPlanId);
        if (plan is null) return (null, "Membership plan not found.");
        if (!plan.IsActive) return (null, "Membership plan is not active.");

        var hasActive = await db.Memberships
            .AnyAsync(m => m.MemberId == dto.MemberId &&
                (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen));
        if (hasActive) return (null, "Member already has an active or frozen membership.");

        var membership = new Membership
        {
            MemberId = dto.MemberId,
            MembershipPlanId = dto.MembershipPlanId,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = dto.PaymentStatus
        };
        db.Memberships.Add(membership);
        await db.SaveChangesAsync();

        logger.LogInformation("Created membership {MembershipId} for member {MemberId}, plan {PlanId}", membership.Id, dto.MemberId, dto.MembershipPlanId);

        var result = await db.Memberships
            .Include(m => m.Member).Include(m => m.MembershipPlan)
            .FirstAsync(m => m.Id == membership.Id);
        return (ToDto(result), null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int id)
    {
        var ms = await db.Memberships.FindAsync(id);
        if (ms is null) return (false, "Membership not found.");
        if (ms.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
            return (false, "Membership is already cancelled or expired.");

        ms.Status = MembershipStatus.Cancelled;
        ms.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Cancelled membership {MembershipId}", id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> FreezeAsync(int id, FreezeMembershipDto dto)
    {
        var ms = await db.Memberships.FindAsync(id);
        if (ms is null) return (false, "Membership not found.");
        if (ms.Status != MembershipStatus.Active)
            return (false, "Only active memberships can be frozen.");

        if (ms.FreezeStartDate.HasValue)
            return (false, "Membership has already been frozen once this term.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        ms.Status = MembershipStatus.Frozen;
        ms.FreezeStartDate = today;
        ms.FreezeEndDate = today.AddDays(dto.FreezeDays);
        ms.EndDate = ms.EndDate.AddDays(dto.FreezeDays);
        ms.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Froze membership {MembershipId} for {Days} days", id, dto.FreezeDays);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UnfreezeAsync(int id)
    {
        var ms = await db.Memberships.FindAsync(id);
        if (ms is null) return (false, "Membership not found.");
        if (ms.Status != MembershipStatus.Frozen)
            return (false, "Membership is not frozen.");

        ms.Status = MembershipStatus.Active;
        ms.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Unfroze membership {MembershipId}", id);
        return (true, null);
    }

    public async Task<(MembershipDto? Result, string? Error)> RenewAsync(int id)
    {
        var ms = await db.Memberships.Include(m => m.MembershipPlan).Include(m => m.Member).FirstOrDefaultAsync(m => m.Id == id);
        if (ms is null) return (null, "Membership not found.");
        if (ms.Status != MembershipStatus.Expired)
            return (null, "Only expired memberships can be renewed.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var newMs = new Membership
        {
            MemberId = ms.MemberId,
            MembershipPlanId = ms.MembershipPlanId,
            StartDate = today,
            EndDate = today.AddMonths(ms.MembershipPlan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid
        };
        db.Memberships.Add(newMs);
        await db.SaveChangesAsync();

        var result = await db.Memberships
            .Include(m => m.Member).Include(m => m.MembershipPlan)
            .FirstAsync(m => m.Id == newMs.Id);
        logger.LogInformation("Renewed membership {OldId} → {NewId} for member {MemberId}", id, newMs.Id, ms.MemberId);
        return (ToDto(result), null);
    }

    internal static MembershipDto ToDto(Membership ms) => new(
        ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
        ms.MembershipPlanId, ms.MembershipPlan.Name,
        ms.StartDate, ms.EndDate, ms.Status, ms.PaymentStatus,
        ms.FreezeStartDate, ms.FreezeEndDate,
        ms.CreatedAt, ms.UpdatedAt);
}
