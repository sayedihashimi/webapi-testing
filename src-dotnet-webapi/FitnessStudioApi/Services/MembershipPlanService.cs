using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MembershipPlanService(FitnessDbContext db, ILogger<MembershipPlanService> logger) : IMembershipPlanService
{
    public async Task<PaginatedResponse<MembershipPlanResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = db.MembershipPlans.AsNoTracking().Where(p => p.IsActive);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);

        return PaginatedResponse<MembershipPlanResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<MembershipPlanResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return plan is null ? null : MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct)
    {
        if (await db.MembershipPlans.AnyAsync(p => p.Name == request.Name, ct))
            throw new InvalidOperationException($"A membership plan with name '{request.Name}' already exists.");

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

    public async Task<MembershipPlanResponse?> UpdateAsync(int id, UpdateMembershipPlanRequest request, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct);
        if (plan is null) return null;

        if (await db.MembershipPlans.AnyAsync(p => p.Name == request.Name && p.Id != id, ct))
            throw new InvalidOperationException($"A membership plan with name '{request.Name}' already exists.");

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.DurationMonths = request.DurationMonths;
        plan.Price = request.Price;
        plan.MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = request.AllowsPremiumClasses;
        plan.IsActive = request.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated membership plan {PlanId}", plan.Id);
        return MapToResponse(plan);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct);
        if (plan is null) return false;

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated membership plan {PlanId}", plan.Id);
        return true;
    }

    private static MembershipPlanResponse MapToResponse(MembershipPlan plan) => new()
    {
        Id = plan.Id,
        Name = plan.Name,
        Description = plan.Description,
        DurationMonths = plan.DurationMonths,
        Price = plan.Price,
        MaxClassBookingsPerWeek = plan.MaxClassBookingsPerWeek,
        AllowsPremiumClasses = plan.AllowsPremiumClasses,
        IsActive = plan.IsActive,
        CreatedAt = plan.CreatedAt,
        UpdatedAt = plan.UpdatedAt
    };
}
