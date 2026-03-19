using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipPlanService(FitnessDbContext db, ILogger<MembershipPlanService> logger) : IMembershipPlanService
{
    public async Task<IReadOnlyList<MembershipPlanResponse>> GetAllActiveAsync(CancellationToken ct)
    {
        return await db.MembershipPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);
    }

    public async Task<MembershipPlanResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var plan = await db.MembershipPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return plan is null ? null : MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct)
    {
        var existing = await db.MembershipPlans.AnyAsync(p => p.Name == request.Name, ct);
        if (existing)
            throw new InvalidOperationException($"A membership plan with name '{request.Name}' already exists.");

        var plan = new MembershipPlan
        {
            Name = request.Name,
            Description = request.Description,
            DurationMonths = request.DurationMonths,
            Price = request.Price,
            MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = request.AllowsPremiumClasses,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.MembershipPlans.Add(plan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created membership plan {PlanId}: {PlanName}", plan.Id, plan.Name);
        return MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> UpdateAsync(int id, UpdateMembershipPlanRequest request, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        var duplicate = await db.MembershipPlans.AnyAsync(p => p.Name == request.Name && p.Id != id, ct);
        if (duplicate)
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

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        var hasActiveMemberships = await db.Memberships
            .AnyAsync(m => m.MembershipPlanId == id && m.Status == MembershipStatus.Active, ct);

        if (hasActiveMemberships)
            throw new InvalidOperationException("Cannot deactivate plan with active memberships.");

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated membership plan {PlanId}", plan.Id);
    }

    private static MembershipPlanResponse MapToResponse(MembershipPlan p) =>
        new(p.Id, p.Name, p.Description, p.DurationMonths, p.Price,
            p.MaxClassBookingsPerWeek, p.AllowsPremiumClasses, p.IsActive,
            p.CreatedAt, p.UpdatedAt);
}
