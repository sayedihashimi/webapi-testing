using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<MembershipResponseDto> CreateAsync(MembershipCreateDto dto)
    {
        var member = await _db.Members.FindAsync(dto.MemberId)
            ?? throw new BusinessRuleException("Member not found.", 404);

        var plan = await _db.MembershipPlans.FindAsync(dto.MembershipPlanId)
            ?? throw new BusinessRuleException("Membership plan not found.", 404);

        if (!plan.IsActive)
            throw new BusinessRuleException("Cannot purchase an inactive plan.");

        // Check for existing active/frozen membership
        var existingActive = await _db.Memberships.AnyAsync(m =>
            m.MemberId == dto.MemberId &&
            (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen));

        if (existingActive)
            throw new BusinessRuleException("Member already has an active or frozen membership. Cancel or let it expire before purchasing a new one.");

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

        _logger.LogInformation("Created membership {MembershipId} for member {MemberId} on plan {PlanName}", membership.Id, dto.MemberId, plan.Name);

        return await GetByIdAsync(membership.Id) ?? throw new BusinessRuleException("Failed to create membership.");
    }

    public async Task<MembershipResponseDto?> GetByIdAsync(int id)
    {
        var m = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id);
        return m is null ? null : MapToDto(m);
    }

    public async Task<MembershipResponseDto> CancelAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new BusinessRuleException("Membership not found.", 404);

        if (membership.Status == MembershipStatus.Cancelled)
            throw new BusinessRuleException("Membership is already cancelled.");

        if (membership.Status == MembershipStatus.Expired)
            throw new BusinessRuleException("Cannot cancel an expired membership.");

        membership.Status = MembershipStatus.Cancelled;
        membership.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Cancelled membership {MembershipId}", id);
        return MapToDto(membership);
    }

    public async Task<MembershipResponseDto> FreezeAsync(int id, FreezeMembershipDto dto)
    {
        var membership = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new BusinessRuleException("Membership not found.", 404);

        if (membership.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (membership.FreezeStartDate.HasValue)
            throw new BusinessRuleException("This membership has already been frozen once. Only one freeze per membership term is allowed.");

        var freezeStart = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.FreezeStartDate = freezeStart;
        membership.FreezeEndDate = freezeStart.AddDays(dto.FreezeDurationDays);
        membership.Status = MembershipStatus.Frozen;
        membership.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Froze membership {MembershipId} for {Days} days", id, dto.FreezeDurationDays);
        return MapToDto(membership);
    }

    public async Task<MembershipResponseDto> UnfreezeAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new BusinessRuleException("Membership not found.", 404);

        if (membership.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        if (membership.FreezeStartDate.HasValue && membership.FreezeEndDate.HasValue)
        {
            var freezeDuration = membership.FreezeEndDate.Value.DayNumber - membership.FreezeStartDate.Value.DayNumber;
            membership.EndDate = membership.EndDate.AddDays(freezeDuration);
        }

        membership.Status = MembershipStatus.Active;
        membership.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Unfroze membership {MembershipId}, extended EndDate to {EndDate}", id, membership.EndDate);
        return MapToDto(membership);
    }

    public async Task<MembershipResponseDto> RenewAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new BusinessRuleException("Membership not found.", 404);

        if (membership.Status != MembershipStatus.Expired)
            throw new BusinessRuleException("Only expired memberships can be renewed.");

        // Check no other active membership
        var hasActive = await _db.Memberships.AnyAsync(m =>
            m.MemberId == membership.MemberId && m.Id != id &&
            (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen));
        if (hasActive)
            throw new BusinessRuleException("Member already has an active or frozen membership.");

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

        _db.Memberships.Add(newMembership);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Renewed membership for member {MemberId}, new membership {MembershipId}", membership.MemberId, newMembership.Id);

        return await GetByIdAsync(newMembership.Id) ?? throw new BusinessRuleException("Failed to renew membership.");
    }

    internal static MembershipResponseDto MapToDto(Membership m) => new()
    {
        Id = m.Id,
        MemberId = m.MemberId,
        MemberName = m.Member != null ? $"{m.Member.FirstName} {m.Member.LastName}" : string.Empty,
        MembershipPlanId = m.MembershipPlanId,
        PlanName = m.MembershipPlan?.Name ?? string.Empty,
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        Status = m.Status.ToString(),
        PaymentStatus = m.PaymentStatus.ToString(),
        FreezeStartDate = m.FreezeStartDate,
        FreezeEndDate = m.FreezeEndDate,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };
}
