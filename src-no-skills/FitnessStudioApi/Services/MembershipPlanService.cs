using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

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

    public async Task<List<MembershipPlanDto>> GetAllAsync()
    {
        return await _db.MembershipPlans
            .Where(p => p.IsActive)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<MembershipPlanDto> GetByIdAsync(int id)
    {
        var plan = await _db.MembershipPlans.FindAsync(id)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");
        return MapToDto(plan);
    }

    public async Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto)
    {
        if (await _db.MembershipPlans.AnyAsync(p => p.Name == dto.Name))
            throw new BusinessRuleException($"A membership plan with name '{dto.Name}' already exists.", 409, "Conflict");

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

        _logger.LogInformation("Created membership plan '{Name}' with ID {Id}", plan.Name, plan.Id);
        return MapToDto(plan);
    }

    public async Task<MembershipPlanDto> UpdateAsync(int id, UpdateMembershipPlanDto dto)
    {
        var plan = await _db.MembershipPlans.FindAsync(id)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        if (await _db.MembershipPlans.AnyAsync(p => p.Name == dto.Name && p.Id != id))
            throw new BusinessRuleException($"A membership plan with name '{dto.Name}' already exists.", 409, "Conflict");

        plan.Name = dto.Name;
        plan.Description = dto.Description;
        plan.DurationMonths = dto.DurationMonths;
        plan.Price = dto.Price;
        plan.MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = dto.AllowsPremiumClasses;
        plan.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated membership plan '{Name}' (ID {Id})", plan.Name, plan.Id);
        return MapToDto(plan);
    }

    public async Task DeleteAsync(int id)
    {
        var plan = await _db.MembershipPlans.FindAsync(id)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        plan.IsActive = false;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deactivated membership plan '{Name}' (ID {Id})", plan.Name, plan.Id);
    }

    private static MembershipPlanDto MapToDto(MembershipPlan p) => new()
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
