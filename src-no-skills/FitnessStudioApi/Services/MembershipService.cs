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

    public async Task<MembershipDto> GetByIdAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        return MapToDto(membership);
    }

    public async Task<MembershipDto> CreateAsync(CreateMembershipDto dto)
    {
        var member = await _db.Members.FindAsync(dto.MemberId)
            ?? throw new KeyNotFoundException($"Member with ID {dto.MemberId} not found.");

        var plan = await _db.MembershipPlans.FindAsync(dto.MembershipPlanId)
            ?? throw new KeyNotFoundException($"Membership plan with ID {dto.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new BusinessRuleException("Cannot purchase an inactive membership plan.");

        // Check for existing active/frozen membership
        var hasActive = await _db.Memberships.AnyAsync(ms =>
            ms.MemberId == dto.MemberId &&
            (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen));

        if (hasActive)
            throw new BusinessRuleException("Member already has an active or frozen membership.");

        var startDate = dto.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var membership = new Membership
        {
            MemberId = dto.MemberId,
            MembershipPlanId = dto.MembershipPlanId,
            StartDate = startDate,
            EndDate = startDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = dto.PaymentStatus
        };

        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created membership (ID {Id}) for member {MemberId} with plan '{PlanName}'",
            membership.Id, membership.MemberId, plan.Name);

        return await GetByIdAsync(membership.Id);
    }

    public async Task<MembershipDto> CancelAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status == MembershipStatus.Cancelled)
            throw new BusinessRuleException("Membership is already cancelled.");

        if (membership.Status == MembershipStatus.Expired)
            throw new BusinessRuleException("Cannot cancel an expired membership.");

        membership.Status = MembershipStatus.Cancelled;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Cancelled membership (ID {Id}) for member {MemberId}", membership.Id, membership.MemberId);
        return MapToDto(membership);
    }

    public async Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto)
    {
        var membership = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (membership.FreezeStartDate.HasValue)
            throw new BusinessRuleException("This membership has already been frozen once. Only one freeze per term is allowed.");

        var freezeStart = DateOnly.FromDateTime(DateTime.UtcNow);
        var freezeEnd = freezeStart.AddDays(dto.FreezeDurationDays);

        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = freezeStart;
        membership.FreezeEndDate = freezeEnd;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Froze membership (ID {Id}) for {Days} days", membership.Id, dto.FreezeDurationDays);
        return MapToDto(membership);
    }

    public async Task<MembershipDto> UnfreezeAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        if (!membership.FreezeStartDate.HasValue || !membership.FreezeEndDate.HasValue)
            throw new BusinessRuleException("Freeze dates are not set.");

        var freezeDuration = membership.FreezeEndDate.Value.DayNumber - membership.FreezeStartDate.Value.DayNumber;
        membership.EndDate = membership.EndDate.AddDays(freezeDuration);
        membership.Status = MembershipStatus.Active;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Unfroze membership (ID {Id}), extended end date by {Days} days", membership.Id, freezeDuration);
        return MapToDto(membership);
    }

    public async Task<MembershipDto> RenewAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Expired && membership.Status != MembershipStatus.Cancelled)
            throw new BusinessRuleException("Only expired or cancelled memberships can be renewed.");

        // Check no existing active membership
        var hasActive = await _db.Memberships.AnyAsync(ms =>
            ms.MemberId == membership.MemberId &&
            ms.Id != id &&
            (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen));

        if (hasActive)
            throw new BusinessRuleException("Member already has an active or frozen membership.");

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var newMembership = new Membership
        {
            MemberId = membership.MemberId,
            MembershipPlanId = membership.MembershipPlanId,
            StartDate = startDate,
            EndDate = startDate.AddMonths(membership.MembershipPlan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid
        };

        _db.Memberships.Add(newMembership);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Renewed membership for member {MemberId}, new membership ID {NewId}", membership.MemberId, newMembership.Id);
        return await GetByIdAsync(newMembership.Id);
    }

    private static MembershipDto MapToDto(Membership ms) => new()
    {
        Id = ms.Id,
        MemberId = ms.MemberId,
        MemberName = ms.Member != null ? $"{ms.Member.FirstName} {ms.Member.LastName}" : "",
        MembershipPlanId = ms.MembershipPlanId,
        PlanName = ms.MembershipPlan?.Name ?? "",
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
