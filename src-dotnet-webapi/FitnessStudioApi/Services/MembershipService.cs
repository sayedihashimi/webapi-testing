using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService(StudioDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships.AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return membership is null ? null : MapToResponse(membership);
    }

    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member {request.MemberId} not found.");

        var plan = await db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new KeyNotFoundException($"Membership plan {request.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new InvalidOperationException("Cannot purchase an inactive membership plan.");

        var hasActiveOrFrozen = await db.Memberships.AnyAsync(
            m => m.MemberId == request.MemberId
                && (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasActiveOrFrozen)
            throw new InvalidOperationException("Member already has an active or frozen membership.");

        var membership = new Membership
        {
            MemberId = request.MemberId,
            MembershipPlanId = request.MembershipPlanId,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = request.PaymentStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);

        // Reload with navigation properties
        var result = await db.Memberships.AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstAsync(m => m.Id == membership.Id, ct);

        logger.LogInformation("Created membership {MembershipId} for member {MemberId}", membership.Id, membership.MemberId);
        return MapToResponse(result);
    }

    public async Task<MembershipResponse> CancelAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership {id} not found.");

        if (membership.Status is not (MembershipStatus.Active or MembershipStatus.Frozen))
            throw new InvalidOperationException($"Cannot cancel a membership with status '{membership.Status}'.");

        membership.Status = MembershipStatus.Cancelled;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Cancelled membership {MembershipId}", id);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership {id} not found.");

        if (membership.Status != MembershipStatus.Active)
            throw new InvalidOperationException("Only active memberships can be frozen.");

        if (membership.FreezeStartDate.HasValue)
            throw new InvalidOperationException("This membership has already been frozen once during this term.");

        var freezeDays = request.FreezeEndDate.DayNumber - request.FreezeStartDate.DayNumber;
        if (freezeDays < 7 || freezeDays > 30)
            throw new ArgumentException("Freeze duration must be between 7 and 30 days.");

        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = request.FreezeStartDate;
        membership.FreezeEndDate = request.FreezeEndDate;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Froze membership {MembershipId} from {Start} to {End}", id, request.FreezeStartDate, request.FreezeEndDate);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership {id} not found.");

        if (membership.Status != MembershipStatus.Frozen)
            throw new InvalidOperationException("Only frozen memberships can be unfrozen.");

        // Extend end date by freeze duration
        if (membership.FreezeStartDate.HasValue && membership.FreezeEndDate.HasValue)
        {
            var freezeDays = membership.FreezeEndDate.Value.DayNumber - membership.FreezeStartDate.Value.DayNumber;
            membership.EndDate = membership.EndDate.AddDays(freezeDays);
        }

        membership.Status = MembershipStatus.Active;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Unfroze membership {MembershipId}", id);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership {id} not found.");

        if (membership.Status != MembershipStatus.Expired)
            throw new InvalidOperationException("Only expired memberships can be renewed.");

        var hasActiveOrFrozen = await db.Memberships.AnyAsync(
            m => m.MemberId == membership.MemberId
                && m.Id != id
                && (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasActiveOrFrozen)
            throw new InvalidOperationException("Member already has an active or frozen membership.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        membership.StartDate = today;
        membership.EndDate = today.AddMonths(membership.MembershipPlan.DurationMonths);
        membership.Status = MembershipStatus.Active;
        membership.PaymentStatus = PaymentStatus.Pending;
        membership.FreezeStartDate = null;
        membership.FreezeEndDate = null;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Renewed membership {MembershipId}", id);
        return MapToResponse(membership);
    }

    private static MembershipResponse MapToResponse(Membership m) =>
        new(m.Id, m.MemberId, $"{m.Member.FirstName} {m.Member.LastName}",
            m.MembershipPlanId, m.MembershipPlan.Name,
            m.StartDate, m.EndDate,
            m.Status.ToString(), m.PaymentStatus.ToString(),
            m.FreezeStartDate, m.FreezeEndDate,
            m.CreatedAt, m.UpdatedAt);
}
