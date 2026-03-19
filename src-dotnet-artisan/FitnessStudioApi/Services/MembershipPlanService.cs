using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipPlanService(AppDbContext db, ILogger<MembershipPlanService> logger) : IMembershipPlanService
{
    public async Task<IReadOnlyList<MembershipPlanResponse>> GetAllActiveAsync(CancellationToken ct)
    {
        return await db.MembershipPlans
            .Where(p => p.IsActive)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);
    }

    public async Task<MembershipPlanResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct);
        return plan is null ? null : MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct)
    {
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
        logger.LogInformation("Created membership plan {PlanName} with Id {PlanId}", plan.Name, plan.Id);
        return MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse?> UpdateAsync(int id, UpdateMembershipPlanRequest request, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct);
        if (plan is null)
        {
            return null;
        }

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.DurationMonths = request.DurationMonths;
        plan.Price = request.Price;
        plan.MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = request.AllowsPremiumClasses;
        plan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return MapToResponse(plan);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
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

    private static MembershipPlanResponse MapToResponse(MembershipPlan p) =>
        new(p.Id, p.Name, p.Description, p.DurationMonths, p.Price,
            p.MaxClassBookingsPerWeek, p.AllowsPremiumClasses, p.IsActive);
}
