using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class ClassTypeService(FitnessDbContext db, ILogger<ClassTypeService> logger) : IClassTypeService
{
    public async Task<IReadOnlyList<ClassTypeDto>> GetAllAsync(DifficultyLevel? difficulty, bool? isPremium)
    {
        var query = db.ClassTypes.Where(c => c.IsActive);

        if (difficulty.HasValue)
            query = query.Where(c => c.DifficultyLevel == difficulty.Value);
        if (isPremium.HasValue)
            query = query.Where(c => c.IsPremium == isPremium.Value);

        return await query.OrderBy(c => c.Name).Select(c => ToDto(c)).ToListAsync();
    }

    public async Task<ClassTypeDto?> GetByIdAsync(int id)
    {
        var ct = await db.ClassTypes.FindAsync(id);
        return ct is null ? null : ToDto(ct);
    }

    public async Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto)
    {
        if (await db.ClassTypes.AnyAsync(c => c.Name == dto.Name))
            throw new ValidationException("A class type with this name already exists.");

        var ct = new ClassType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDurationMinutes = dto.DefaultDurationMinutes,
            DefaultCapacity = dto.DefaultCapacity,
            IsPremium = dto.IsPremium,
            CaloriesPerSession = dto.CaloriesPerSession,
            DifficultyLevel = dto.DifficultyLevel
        };
        db.ClassTypes.Add(ct);
        await db.SaveChangesAsync();
        logger.LogInformation("Created class type {Name} (Id={Id})", ct.Name, ct.Id);
        return ToDto(ct);
    }

    public async Task<ClassTypeDto?> UpdateAsync(int id, UpdateClassTypeDto dto)
    {
        var ct = await db.ClassTypes.FindAsync(id);
        if (ct is null) return null;

        if (await db.ClassTypes.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new ValidationException("A class type with this name already exists.");

        ct.Name = dto.Name;
        ct.Description = dto.Description;
        ct.DefaultDurationMinutes = dto.DefaultDurationMinutes;
        ct.DefaultCapacity = dto.DefaultCapacity;
        ct.IsPremium = dto.IsPremium;
        ct.CaloriesPerSession = dto.CaloriesPerSession;
        ct.DifficultyLevel = dto.DifficultyLevel;
        ct.IsActive = dto.IsActive;
        ct.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated class type {Id}", id);
        return ToDto(ct);
    }

    private static ClassTypeDto ToDto(ClassType c) => new(
        c.Id, c.Name, c.Description, c.DefaultDurationMinutes,
        c.DefaultCapacity, c.IsPremium, c.CaloriesPerSession,
        c.DifficultyLevel, c.IsActive, c.CreatedAt, c.UpdatedAt);
}
