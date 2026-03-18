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
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        var plan = await db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {request.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new ArgumentException("Cannot create membership with an inactive plan.");

        var hasActiveMembership = await db.Memberships.AnyAsync(
            ms => ms.MemberId == request.MemberId &&
                  (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        if (hasActiveMembership)
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

        logger.LogInformation("Created membership {MembershipId} for member {MemberId}", membership.Id, membership.MemberId);

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
        var ms = await db.Memberships.Include(m => m.Member).Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status == MembershipStatus.Cancelled)
            throw new InvalidOperationException("Membership is already cancelled.");

        if (ms.Status == MembershipStatus.Expired)
            throw new InvalidOperationException("Cannot cancel an expired membership.");

        ms.Status = MembershipStatus.Cancelled;
        ms.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled membership {MembershipId}", ms.Id);
        return MapToResponse(ms);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var ms = await db.Memberships.Include(m => m.Member).Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

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
        ms.EndDate = ms.EndDate.AddDays(freezeDays);
        ms.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Froze membership {MembershipId} until {FreezeEnd}", ms.Id, request.FreezeEndDate);
        return MapToResponse(ms);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct)
    {
        var ms = await db.Memberships.Include(m => m.Member).Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Frozen)
            throw new InvalidOperationException("Only frozen memberships can be unfrozen.");

        ms.Status = MembershipStatus.Active;
        ms.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Unfroze membership {MembershipId}", ms.Id);
        return MapToResponse(ms);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct)
    {
        var ms = await db.Memberships.Include(m => m.Member).Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Active && ms.Status != MembershipStatus.Expired)
            throw new InvalidOperationException("Only active or expired memberships can be renewed.");

        var newStartDate = ms.EndDate > DateOnly.FromDateTime(DateTime.UtcNow)
            ? ms.EndDate
            : DateOnly.FromDateTime(DateTime.UtcNow);

        ms.StartDate = newStartDate;
        ms.EndDate = newStartDate.AddMonths(ms.MembershipPlan.DurationMonths);
        ms.Status = MembershipStatus.Active;
        ms.FreezeStartDate = null;
        ms.FreezeEndDate = null;
        ms.PaymentStatus = PaymentStatus.Paid;
        ms.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Renewed membership {MembershipId}", ms.Id);
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
