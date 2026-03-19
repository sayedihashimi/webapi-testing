using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class MembershipService : IMembershipService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<MembershipService> _logger;

    public MembershipService(FitnessDbContext db, ILogger<MembershipService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MembershipDto> CreateAsync(CreateMembershipDto dto)
    {
        var member = await _db.Members.FindAsync(dto.MemberId)
            ?? throw new KeyNotFoundException($"Member with ID {dto.MemberId} not found.");

        var plan = await _db.MembershipPlans.FindAsync(dto.MembershipPlanId)
            ?? throw new KeyNotFoundException($"Membership plan with ID {dto.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new BusinessRuleException("Cannot create a membership with an inactive plan.");

        var hasActiveMembership = await _db.Memberships.AnyAsync(ms =>
            ms.MemberId == dto.MemberId &&
            (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen));

        if (hasActiveMembership)
            throw new BusinessRuleException("Member already has an active or frozen membership. Cancel or let it expire first.");

        if (!Enum.TryParse<PaymentStatus>(dto.PaymentStatus, true, out var paymentStatus))
            paymentStatus = PaymentStatus.Paid;

        var membership = new Membership
        {
            MemberId = dto.MemberId,
            MembershipPlanId = dto.MembershipPlanId,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = paymentStatus
        };

        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created membership for member {MemberId} with plan {PlanName}", dto.MemberId, plan.Name);

        return await GetByIdAsync(membership.Id);
    }

    public async Task<MembershipDto> GetByIdAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        return MapToDto(ms);
    }

    public async Task<MembershipDto> CancelAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status == MembershipStatus.Cancelled)
            throw new BusinessRuleException("Membership is already cancelled.");

        if (ms.Status == MembershipStatus.Expired)
            throw new BusinessRuleException("Cannot cancel an expired membership.");

        ms.Status = MembershipStatus.Cancelled;
        ms.PaymentStatus = PaymentStatus.Refunded;
        ms.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Cancelled membership {MembershipId} for member {MemberId}", id, ms.MemberId);
        return MapToDto(ms);
    }

    public async Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (ms.FreezeStartDate.HasValue)
            throw new BusinessRuleException("This membership has already been frozen once. Only one freeze per term is allowed.");

        ms.Status = MembershipStatus.Frozen;
        ms.FreezeStartDate = DateOnly.FromDateTime(DateTime.Today);
        ms.FreezeEndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(dto.FreezeDurationDays));
        ms.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Frozen membership {MembershipId} for {Days} days", id, dto.FreezeDurationDays);
        return MapToDto(ms);
    }

    public async Task<MembershipDto> UnfreezeAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        if (!ms.FreezeStartDate.HasValue || !ms.FreezeEndDate.HasValue)
            throw new BusinessRuleException("Freeze dates are not set.");

        var freezeDuration = ms.FreezeEndDate.Value.DayNumber - ms.FreezeStartDate.Value.DayNumber;
        ms.EndDate = ms.EndDate.AddDays(freezeDuration);
        ms.Status = MembershipStatus.Active;
        ms.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Unfrozen membership {MembershipId}, end date extended by {Days} days", id, freezeDuration);
        return MapToDto(ms);
    }

    public async Task<MembershipDto> RenewAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Expired && ms.Status != MembershipStatus.Cancelled)
            throw new BusinessRuleException("Only expired or cancelled memberships can be renewed.");

        var hasActiveMembership = await _db.Memberships.AnyAsync(m =>
            m.MemberId == ms.MemberId && m.Id != id &&
            (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen));

        if (hasActiveMembership)
            throw new BusinessRuleException("Member already has an active or frozen membership.");

        var startDate = DateOnly.FromDateTime(DateTime.Today);
        var newMembership = new Membership
        {
            MemberId = ms.MemberId,
            MembershipPlanId = ms.MembershipPlanId,
            StartDate = startDate,
            EndDate = startDate.AddMonths(ms.MembershipPlan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid
        };

        _db.Memberships.Add(newMembership);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Renewed membership for member {MemberId} with plan {PlanName}", ms.MemberId, ms.MembershipPlan.Name);

        return await GetByIdAsync(newMembership.Id);
    }

    internal static MembershipDto MapToDto(Membership ms) => new()
    {
        Id = ms.Id,
        MemberId = ms.MemberId,
        MemberName = $"{ms.Member.FirstName} {ms.Member.LastName}",
        MembershipPlanId = ms.MembershipPlanId,
        PlanName = ms.MembershipPlan.Name,
        StartDate = ms.StartDate,
        EndDate = ms.EndDate,
        Status = ms.Status.ToString(),
        PaymentStatus = ms.PaymentStatus.ToString(),
        FreezeStartDate = ms.FreezeStartDate,
        FreezeEndDate = ms.FreezeEndDate,
        CreatedAt = ms.CreatedAt,
        UpdatedAt = ms.UpdatedAt
    };
}
