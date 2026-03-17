using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class MembershipPlanService(FitnessDbContext db, ILogger<MembershipPlanService> logger) : IMembershipPlanService
{
    public async Task<IReadOnlyList<MembershipPlanDto>> GetAllActiveAsync()
    {
        return await db.MembershipPlans
            .Where(p => p.IsActive)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<MembershipPlanDto?> GetByIdAsync(int id)
    {
        var plan = await db.MembershipPlans.FindAsync(id);
        return plan is null ? null : ToDto(plan);
    }

    public async Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto)
    {
        var plan = new MembershipPlan
        {
            Name = dto.Name,
            Description = dto.Description,
            DurationMonths = dto.DurationMonths,
            Price = dto.Price,
            MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = dto.AllowsPremiumClasses
        };
        db.MembershipPlans.Add(plan);
        await db.SaveChangesAsync();
        logger.LogInformation("Created membership plan {PlanName} (Id={PlanId})", plan.Name, plan.Id);
        return ToDto(plan);
    }

    public async Task<MembershipPlanDto?> UpdateAsync(int id, UpdateMembershipPlanDto dto)
    {
        var plan = await db.MembershipPlans.FindAsync(id);
        if (plan is null) return null;

        plan.Name = dto.Name;
        plan.Description = dto.Description;
        plan.DurationMonths = dto.DurationMonths;
        plan.Price = dto.Price;
        plan.MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = dto.AllowsPremiumClasses;
        plan.IsActive = dto.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated membership plan {PlanId}", id);
        return ToDto(plan);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var plan = await db.MembershipPlans.FindAsync(id);
        if (plan is null) return false;

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Deactivated membership plan {PlanId}", id);
        return true;
    }

    private static MembershipPlanDto ToDto(MembershipPlan p) => new(
        p.Id, p.Name, p.Description, p.DurationMonths, p.Price,
        p.MaxClassBookingsPerWeek, p.AllowsPremiumClasses,
        p.IsActive, p.CreatedAt, p.UpdatedAt);
}
