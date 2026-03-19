using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MembershipPlanService : IMembershipPlanService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<MembershipPlanService> _logger;

    public MembershipPlanService(FitnessDbContext db, ILogger<MembershipPlanService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<MembershipPlanResponseDto>> GetAllActiveAsync()
    {
        return await _db.MembershipPlans
            .Where(p => p.IsActive)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<MembershipPlanResponseDto?> GetByIdAsync(int id)
    {
        var plan = await _db.MembershipPlans.FindAsync(id);
        return plan is null ? null : MapToDto(plan);
    }

    public async Task<MembershipPlanResponseDto> CreateAsync(MembershipPlanCreateDto dto)
    {
        if (await _db.MembershipPlans.AnyAsync(p => p.Name == dto.Name))
            throw new BusinessRuleException($"A plan with name '{dto.Name}' already exists.");

        var plan = new MembershipPlan
        {
            Name = dto.Name,
            Description = dto.Description,
            DurationMonths = dto.DurationMonths,
            Price = dto.Price,
            MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = dto.AllowsPremiumClasses
        };

        _db.MembershipPlans.Add(plan);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created membership plan {PlanName} with ID {PlanId}", plan.Name, plan.Id);
        return MapToDto(plan);
    }

    public async Task<MembershipPlanResponseDto?> UpdateAsync(int id, MembershipPlanUpdateDto dto)
    {
        var plan = await _db.MembershipPlans.FindAsync(id);
        if (plan is null) return null;

        if (await _db.MembershipPlans.AnyAsync(p => p.Name == dto.Name && p.Id != id))
            throw new BusinessRuleException($"A plan with name '{dto.Name}' already exists.");

        plan.Name = dto.Name;
        plan.Description = dto.Description;
        plan.DurationMonths = dto.DurationMonths;
        plan.Price = dto.Price;
        plan.MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = dto.AllowsPremiumClasses;
        plan.IsActive = dto.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated membership plan {PlanId}", id);
        return MapToDto(plan);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var plan = await _db.MembershipPlans.FindAsync(id);
        if (plan is null) return false;

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deactivated membership plan {PlanId}", id);
        return true;
    }

    private static MembershipPlanResponseDto MapToDto(MembershipPlan p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        DurationMonths = p.DurationMonths,
        Price = p.Price,
        MaxClassBookingsPerWeek = p.MaxClassBookingsPerWeek,
        AllowsPremiumClasses = p.AllowsPremiumClasses,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
