using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MembershipService(FitnessDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member {request.MemberId} not found.");

        var plan = await db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new KeyNotFoundException($"Membership plan {request.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new ArgumentException("Cannot create membership for an inactive plan.");

        var hasActive = await db.Memberships.AnyAsync(
            ms => ms.MemberId == request.MemberId && (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);
        if (hasActive)
            throw new InvalidOperationException("Member already has an active or frozen membership.");

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

        logger.LogInformation("Created membership {MembershipId} for member {MemberId} on plan {PlanName}", membership.Id, member.Id, plan.Name);

        return await GetByIdAsync(membership.Id, ct) ?? throw new InvalidOperationException("Failed to retrieve created membership.");
    }

    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var ms = await db.Memberships.AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return ms is null ? null : MapToResponse(ms);
    }

    public async Task<MembershipResponse> CancelAsync(int id, CancellationToken ct)
    {
        var ms = await db.Memberships.Include(m => m.Member).Include(m => m.MembershipPlan).FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership {id} not found.");

        if (ms.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
            throw new InvalidOperationException($"Cannot cancel a membership that is {ms.Status}.");

        ms.Status = MembershipStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Cancelled membership {MembershipId}", id);
        return MapToResponse(ms);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var ms = await db.Memberships.Include(m => m.Member).Include(m => m.MembershipPlan).FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership {id} not found.");

        if (ms.Status != MembershipStatus.Active)
            throw new InvalidOperationException("Only active memberships can be frozen.");

        if (ms.FreezeStartDate.HasValue)
            throw new InvalidOperationException("This membership has already been frozen once during this term.");

        var freezeDays = request.FreezeEndDate.DayNumber - request.FreezeStartDate.DayNumber;
        if (freezeDays < 7 || freezeDays > 30)
            throw new ArgumentException("Freeze duration must be between 7 and 30 days.");

        ms.Status = MembershipStatus.Frozen;
        ms.FreezeStartDate = request.FreezeStartDate;
        ms.FreezeEndDate = request.FreezeEndDate;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Froze membership {MembershipId} from {Start} to {End}", id, request.FreezeStartDate, request.FreezeEndDate);
        return MapToResponse(ms);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct)
    {
        var ms = await db.Memberships.Include(m => m.Member).Include(m => m.MembershipPlan).FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership {id} not found.");

        if (ms.Status != MembershipStatus.Frozen)
            throw new InvalidOperationException("Only frozen memberships can be unfrozen.");

        var freezeDays = ms.FreezeEndDate!.Value.DayNumber - ms.FreezeStartDate!.Value.DayNumber;
        ms.EndDate = ms.EndDate.AddDays(freezeDays);
        ms.Status = MembershipStatus.Active;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Unfroze membership {MembershipId}, extended end date by {Days} days", id, freezeDays);
        return MapToResponse(ms);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct)
    {
        var ms = await db.Memberships.Include(m => m.Member).Include(m => m.MembershipPlan).FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership {id} not found.");

        if (ms.Status != MembershipStatus.Expired)
            throw new InvalidOperationException("Only expired memberships can be renewed.");

        var hasActive = await db.Memberships.AnyAsync(
            m => m.MemberId == ms.MemberId && m.Id != id && (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);
        if (hasActive)
            throw new InvalidOperationException("Member already has an active or frozen membership.");

        ms.StartDate = DateOnly.FromDateTime(DateTime.Today);
        ms.EndDate = ms.StartDate.AddMonths(ms.MembershipPlan.DurationMonths);
        ms.Status = MembershipStatus.Active;
        ms.PaymentStatus = PaymentStatus.Paid;
        ms.FreezeStartDate = null;
        ms.FreezeEndDate = null;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Renewed membership {MembershipId}", id);
        return MapToResponse(ms);
    }

    private static MembershipResponse MapToResponse(Membership ms) => new()
    {
        Id = ms.Id,
        MemberId = ms.MemberId,
        MemberName = $"{ms.Member.FirstName} {ms.Member.LastName}",
        MembershipPlanId = ms.MembershipPlanId,
        PlanName = ms.MembershipPlan.Name,
        StartDate = ms.StartDate,
        EndDate = ms.EndDate,
        Status = ms.Status,
        PaymentStatus = ms.PaymentStatus,
        FreezeStartDate = ms.FreezeStartDate,
        FreezeEndDate = ms.FreezeEndDate,
        CreatedAt = ms.CreatedAt,
        UpdatedAt = ms.UpdatedAt
    };
}
