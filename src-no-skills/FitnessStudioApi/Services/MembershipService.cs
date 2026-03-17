using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class MembershipService
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
            ?? throw new BusinessRuleException($"Member with ID {dto.MemberId} not found.", 404);

        var plan = await _db.MembershipPlans.FindAsync(dto.MembershipPlanId)
            ?? throw new BusinessRuleException($"Membership plan with ID {dto.MembershipPlanId} not found.", 404);

        if (!plan.IsActive)
            throw new BusinessRuleException("Cannot create membership with an inactive plan.");

        var hasActive = await _db.Memberships.AnyAsync(ms =>
            ms.MemberId == dto.MemberId &&
            (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen));

        if (hasActive)
            throw new BusinessRuleException("Member already has an active or frozen membership.", 409);

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

        _logger.LogInformation("Created membership {MembershipId} for member {MemberId} on plan {PlanName}",
            membership.Id, dto.MemberId, plan.Name);

        return await GetByIdAsync(membership.Id) ?? throw new InvalidOperationException();
    }

    public async Task<MembershipDto?> GetByIdAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id);

        return ms == null ? null : MapToDto(ms);
    }

    public async Task<MembershipDto> CancelAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new BusinessRuleException($"Membership with ID {id} not found.", 404);

        if (ms.Status == MembershipStatus.Cancelled)
            throw new BusinessRuleException("Membership is already cancelled.");

        if (ms.Status == MembershipStatus.Expired)
            throw new BusinessRuleException("Cannot cancel an expired membership.");

        ms.Status = MembershipStatus.Cancelled;
        ms.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Cancelled membership {MembershipId}", id);
        return MapToDto(ms);
    }

    public async Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new BusinessRuleException($"Membership with ID {id} not found.", 404);

        if (ms.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (ms.FreezeStartDate.HasValue)
            throw new BusinessRuleException("This membership has already been frozen once during this term.");

        ms.Status = MembershipStatus.Frozen;
        ms.FreezeStartDate = DateOnly.FromDateTime(DateTime.Today);
        ms.FreezeEndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(dto.DurationDays));
        ms.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Froze membership {MembershipId} for {Days} days", id, dto.DurationDays);
        return MapToDto(ms);
    }

    public async Task<MembershipDto> UnfreezeAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new BusinessRuleException($"Membership with ID {id} not found.", 404);

        if (ms.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        var freezeDays = ms.FreezeEndDate!.Value.DayNumber - ms.FreezeStartDate!.Value.DayNumber;
        ms.EndDate = ms.EndDate.AddDays(freezeDays);
        ms.Status = MembershipStatus.Active;
        ms.FreezeEndDate = DateOnly.FromDateTime(DateTime.Today);
        ms.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Unfroze membership {MembershipId}, extended end date by {Days} days", id, freezeDays);
        return MapToDto(ms);
    }

    public async Task<MembershipDto> RenewAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new BusinessRuleException($"Membership with ID {id} not found.", 404);

        if (ms.Status != MembershipStatus.Expired && ms.Status != MembershipStatus.Cancelled)
            throw new BusinessRuleException("Only expired or cancelled memberships can be renewed.");

        var hasActive = await _db.Memberships.AnyAsync(m =>
            m.MemberId == ms.MemberId && m.Id != id &&
            (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen));

        if (hasActive)
            throw new BusinessRuleException("Member already has an active or frozen membership.", 409);

        var newStart = DateOnly.FromDateTime(DateTime.Today);
        ms.StartDate = newStart;
        ms.EndDate = newStart.AddMonths(ms.MembershipPlan.DurationMonths);
        ms.Status = MembershipStatus.Active;
        ms.PaymentStatus = PaymentStatus.Paid;
        ms.FreezeStartDate = null;
        ms.FreezeEndDate = null;
        ms.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Renewed membership {MembershipId}", id);
        return MapToDto(ms);
    }

    public static MembershipDto MapToDto(Membership ms) => new()
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
