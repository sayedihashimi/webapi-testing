using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
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

    public async Task<MembershipDto> CreateAsync(CreateMembershipDto dto)
    {
        var member = await _db.Members.FindAsync(dto.MemberId)
            ?? throw new NotFoundException($"Member with ID {dto.MemberId} not found.");

        var plan = await _db.MembershipPlans.FindAsync(dto.MembershipPlanId)
            ?? throw new NotFoundException($"Membership plan with ID {dto.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new BusinessRuleException("Cannot create membership for an inactive plan.");

        // Check if member already has an active/frozen membership
        var existing = await _db.Memberships
            .AnyAsync(ms => ms.MemberId == dto.MemberId &&
                (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen));
        if (existing)
            throw new BusinessRuleException("Member already has an active or frozen membership.");

        var membership = new Membership
        {
            MemberId = dto.MemberId,
            MembershipPlanId = dto.MembershipPlanId,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = dto.PaymentStatus
        };
        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        await _db.Entry(membership).Reference(m => m.Member).LoadAsync();
        await _db.Entry(membership).Reference(m => m.MembershipPlan).LoadAsync();

        _logger.LogInformation("Membership {MembershipId} created for member {MemberId} on plan {PlanName}",
            membership.Id, member.Id, plan.Name);

        return ToDto(membership);
    }

    public async Task<MembershipDto?> GetByIdAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id);
        return membership is null ? null : ToDto(membership);
    }

    public async Task<MembershipDto> CancelAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id)
            ?? throw new NotFoundException($"Membership with ID {id} not found.");

        if (membership.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
            throw new BusinessRuleException($"Cannot cancel a membership that is already {membership.Status}.");

        membership.Status = MembershipStatus.Cancelled;
        membership.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Membership {MembershipId} cancelled", id);
        return ToDto(membership);
    }

    public async Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto)
    {
        var membership = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id)
            ?? throw new NotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (membership.FreezeStartDate.HasValue)
            throw new BusinessRuleException("This membership has already been frozen once and cannot be frozen again.");

        if (dto.FreezeDurationDays < 7 || dto.FreezeDurationDays > 30)
            throw new BusinessRuleException("Freeze duration must be between 7 and 30 days.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = today;
        membership.FreezeEndDate = today.AddDays(dto.FreezeDurationDays);
        membership.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Membership {MembershipId} frozen for {Days} days", id, dto.FreezeDurationDays);
        return ToDto(membership);
    }

    public async Task<MembershipDto> UnfreezeAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id)
            ?? throw new NotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        // Extend EndDate by freeze duration
        if (membership.FreezeStartDate.HasValue && membership.FreezeEndDate.HasValue)
        {
            var freezeDays = membership.FreezeEndDate.Value.DayNumber - membership.FreezeStartDate.Value.DayNumber;
            membership.EndDate = membership.EndDate.AddDays(freezeDays);
        }

        membership.Status = MembershipStatus.Active;
        membership.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Membership {MembershipId} unfrozen, EndDate extended to {EndDate}", id, membership.EndDate);
        return ToDto(membership);
    }

    public async Task<MembershipDto> RenewAsync(int id)
    {
        var membership = await _db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id)
            ?? throw new NotFoundException($"Membership with ID {id} not found.");

        if (membership.Status is not (MembershipStatus.Expired or MembershipStatus.Cancelled))
            throw new BusinessRuleException("Only expired or cancelled memberships can be renewed.");

        // Check no existing active/frozen membership
        var hasActive = await _db.Memberships
            .AnyAsync(ms => ms.MemberId == membership.MemberId && ms.Id != id &&
                (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen));
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

        await _db.Entry(newMembership).Reference(m => m.Member).LoadAsync();
        await _db.Entry(newMembership).Reference(m => m.MembershipPlan).LoadAsync();

        _logger.LogInformation("Membership renewed for member {MemberId}, new membership ID {NewId}", membership.MemberId, newMembership.Id);
        return ToDto(newMembership);
    }

    internal static MembershipDto ToDto(Membership ms) => new(
        ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
        ms.MembershipPlanId, ms.MembershipPlan.Name,
        ms.StartDate, ms.EndDate, ms.Status, ms.PaymentStatus,
        ms.FreezeStartDate, ms.FreezeEndDate, ms.CreatedAt, ms.UpdatedAt);
}
