using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipPlanService(FitnessDbContext db, ILogger<MembershipPlanService> logger) : IMembershipPlanService
{
    public async Task<List<MembershipPlanResponse>> GetAllActivePlansAsync(CancellationToken ct = default)
    {
        var plans = await db.MembershipPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .ToListAsync(ct);

        return plans.Select(MapToResponse).ToList();
    }

    public async Task<MembershipPlanResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct);
        return plan is null ? null : MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct = default)
    {
        if (await db.MembershipPlans.AnyAsync(p => p.Name == request.Name, ct))
        {
            throw new InvalidOperationException($"A membership plan with name '{request.Name}' already exists.");
        }

        var plan = new MembershipPlan
        {
            Name = request.Name,
            Description = request.Description,
            DurationMonths = request.DurationMonths,
            Price = request.Price,
            MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = request.AllowsPremiumClasses
        };

        db.MembershipPlans.Add(plan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created membership plan {PlanName} with ID {PlanId}", plan.Name, plan.Id);

        return MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse?> UpdateAsync(int id, UpdateMembershipPlanRequest request, CancellationToken ct = default)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct);
        if (plan is null)
        {
            return null;
        }

        if (await db.MembershipPlans.AnyAsync(p => p.Name == request.Name && p.Id != id, ct))
        {
            throw new InvalidOperationException($"A membership plan with name '{request.Name}' already exists.");
        }

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.DurationMonths = request.DurationMonths;
        plan.Price = request.Price;
        plan.MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = request.AllowsPremiumClasses;
        plan.IsActive = request.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated membership plan {PlanId}", id);

        return MapToResponse(plan);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct);
        if (plan is null)
        {
            return false;
        }

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated membership plan {PlanId}", id);

        return true;
    }

    private static MembershipPlanResponse MapToResponse(MembershipPlan plan) => new(
        plan.Id,
        plan.Name,
        plan.Description,
        plan.DurationMonths,
        plan.Price,
        plan.MaxClassBookingsPerWeek,
        plan.AllowsPremiumClasses,
        plan.IsActive,
        plan.CreatedAt,
        plan.UpdatedAt);
}
