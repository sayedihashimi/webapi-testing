using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipPlanService(StudioDbContext db)
{
    public async Task<List<MembershipPlanResponse>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await db.MembershipPlans
            .Where(p => p.IsActive)
            .Select(p => ToResponse(p))
            .ToListAsync(ct);
    }

    public async Task<MembershipPlanResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct);
        return plan is null ? null : ToResponse(plan);
    }

    public async Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct = default)
    {
        if (await db.MembershipPlans.AnyAsync(p => p.Name == request.Name, ct))
        {
            throw new InvalidOperationException($"A plan with name '{request.Name}' already exists.");
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
        return ToResponse(plan);
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
            throw new InvalidOperationException($"A plan with name '{request.Name}' already exists.");
        }

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.DurationMonths = request.DurationMonths;
        plan.Price = request.Price;
        plan.MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = request.AllowsPremiumClasses;
        plan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return ToResponse(plan);
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
        return true;
    }

    private static MembershipPlanResponse ToResponse(MembershipPlan p) => new(
        p.Id, p.Name, p.Description, p.DurationMonths, p.Price,
        p.MaxClassBookingsPerWeek, p.AllowsPremiumClasses, p.IsActive);
}
