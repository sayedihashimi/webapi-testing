using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService(FitnessDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        if (!member.IsActive)
            throw new ArgumentException("Cannot create membership for inactive member.");

        var plan = await db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {request.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new ArgumentException("Cannot create membership with an inactive plan.");

        var hasActive = await db.Memberships.AnyAsync(
            m => m.MemberId == request.MemberId &&
                (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
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

        logger.LogInformation("Created membership {MembershipId} for member {MemberId}", membership.Id, membership.MemberId);

        return MapToResponse(membership, member, plan);
    }

    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (membership is null) return null;

        return MapToResponse(membership, membership.Member, membership.MembershipPlan);
    }

    public async Task<MembershipResponse> CancelAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Active && membership.Status != MembershipStatus.Frozen)
            throw new ArgumentException("Only active or frozen memberships can be cancelled.");

        membership.Status = MembershipStatus.Cancelled;
        membership.PaymentStatus = PaymentStatus.Refunded;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled membership {MembershipId}", id);
        return MapToResponse(membership, membership.Member, membership.MembershipPlan);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Active)
            throw new ArgumentException("Only active memberships can be frozen.");

        if (membership.FreezeStartDate.HasValue)
            throw new InvalidOperationException("This membership has already been frozen once during the current term.");

        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.FreezeEndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(request.DurationDays);
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Froze membership {MembershipId} for {Days} days", id, request.DurationDays);
        return MapToResponse(membership, membership.Member, membership.MembershipPlan);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Frozen)
            throw new ArgumentException("Only frozen memberships can be unfrozen.");

        // Extend end date by the remaining freeze days
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (membership.FreezeEndDate.HasValue && membership.FreezeEndDate.Value > today)
        {
            var remainingDays = membership.FreezeEndDate.Value.DayNumber - today.DayNumber;
            membership.EndDate = membership.EndDate.AddDays(remainingDays);
        }

        membership.Status = MembershipStatus.Active;
        membership.FreezeEndDate = today;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Unfroze membership {MembershipId}", id);
        return MapToResponse(membership, membership.Member, membership.MembershipPlan);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Expired)
            throw new ArgumentException("Only expired memberships can be renewed.");

        // Check no active membership exists
        var hasActive = await db.Memberships.AnyAsync(
            m => m.MemberId == membership.MemberId && m.Id != id &&
                (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
            throw new InvalidOperationException("Member already has an active or frozen membership.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.StartDate = today;
        membership.EndDate = today.AddMonths(membership.MembershipPlan.DurationMonths);
        membership.Status = MembershipStatus.Active;
        membership.PaymentStatus = PaymentStatus.Paid;
        membership.FreezeStartDate = null;
        membership.FreezeEndDate = null;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Renewed membership {MembershipId}", id);
        return MapToResponse(membership, membership.Member, membership.MembershipPlan);
    }

    private static MembershipResponse MapToResponse(Membership m, Member member, MembershipPlan plan) =>
        new(m.Id, m.MemberId, $"{member.FirstName} {member.LastName}",
            m.MembershipPlanId, plan.Name,
            m.StartDate, m.EndDate, m.Status, m.PaymentStatus,
            m.FreezeStartDate, m.FreezeEndDate, m.CreatedAt, m.UpdatedAt);
}
