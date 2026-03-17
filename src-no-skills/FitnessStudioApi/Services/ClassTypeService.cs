using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class ClassTypeService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<ClassTypeService> _logger;

    public ClassTypeService(FitnessDbContext db, ILogger<ClassTypeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<ClassTypeDto>> GetAllAsync(DifficultyLevel? difficulty, bool? isPremium)
    {
        var query = _db.ClassTypes.AsQueryable();

        if (difficulty.HasValue)
            query = query.Where(ct => ct.DifficultyLevel == difficulty.Value);

        if (isPremium.HasValue)
            query = query.Where(ct => ct.IsPremium == isPremium.Value);

        return await query
            .OrderBy(ct => ct.Name)
            .Select(ct => MapToDto(ct))
            .ToListAsync();
    }

    public async Task<ClassTypeDto?> GetByIdAsync(int id)
    {
        var ct = await _db.ClassTypes.FindAsync(id);
        return ct == null ? null : MapToDto(ct);
    }

    public async Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto)
    {
        if (await _db.ClassTypes.AnyAsync(ct => ct.Name == dto.Name))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.", 409);

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

        _db.ClassTypes.Add(ct);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created class type {ClassTypeName} with ID {ClassTypeId}", ct.Name, ct.Id);
        return MapToDto(ct);
    }

    public async Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto)
    {
        var ct = await _db.ClassTypes.FindAsync(id)
            ?? throw new BusinessRuleException($"Class type with ID {id} not found.", 404);

        if (await _db.ClassTypes.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.", 409);

        ct.Name = dto.Name;
        ct.Description = dto.Description;
        ct.DefaultDurationMinutes = dto.DefaultDurationMinutes;
        ct.DefaultCapacity = dto.DefaultCapacity;
        ct.IsPremium = dto.IsPremium;
        ct.CaloriesPerSession = dto.CaloriesPerSession;
        ct.DifficultyLevel = dto.DifficultyLevel;
        ct.IsActive = dto.IsActive;
        ct.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated class type {ClassTypeId}", id);
        return MapToDto(ct);
    }

    private static ClassTypeDto MapToDto(ClassType ct) => new()
    {
        Id = ct.Id,
        Name = ct.Name,
        Description = ct.Description,
        DefaultDurationMinutes = ct.DefaultDurationMinutes,
        DefaultCapacity = ct.DefaultCapacity,
        IsPremium = ct.IsPremium,
        CaloriesPerSession = ct.CaloriesPerSession,
        DifficultyLevel = ct.DifficultyLevel,
        IsActive = ct.IsActive,
        CreatedAt = ct.CreatedAt,
        UpdatedAt = ct.UpdatedAt
    };
}
