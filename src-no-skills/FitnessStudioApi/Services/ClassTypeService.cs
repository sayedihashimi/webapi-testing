using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class ClassTypeService : IClassTypeService
{
    private readonly FitnessDbContext _db;

    public ClassTypeService(FitnessDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClassTypeDto>> GetAllAsync(string? difficulty, bool? isPremium)
    {
        var query = _db.ClassTypes.Where(ct => ct.IsActive);

        if (isPremium.HasValue)
            query = query.Where(ct => ct.IsPremium == isPremium.Value);

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var dl))
            query = query.Where(ct => ct.DifficultyLevel == dl);

        return await query
            .OrderBy(ct => ct.Name)
            .Select(ct => MapToDto(ct))
            .ToListAsync();
    }

    public async Task<ClassTypeDto> GetByIdAsync(int id)
    {
        var ct = await _db.ClassTypes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Class type with ID {id} not found.");
        return MapToDto(ct);
    }

    public async Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto)
    {
        if (await _db.ClassTypes.AnyAsync(ct => ct.Name == dto.Name))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.", 409);

        if (!Enum.TryParse<DifficultyLevel>(dto.DifficultyLevel, true, out var dl))
            throw new BusinessRuleException($"Invalid difficulty level: {dto.DifficultyLevel}. Valid values: Beginner, Intermediate, Advanced, AllLevels");

        var classType = new ClassType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDurationMinutes = dto.DefaultDurationMinutes,
            DefaultCapacity = dto.DefaultCapacity,
            IsPremium = dto.IsPremium,
            CaloriesPerSession = dto.CaloriesPerSession,
            DifficultyLevel = dl
        };

        _db.ClassTypes.Add(classType);
        await _db.SaveChangesAsync();
        return MapToDto(classType);
    }

    public async Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto)
    {
        var classType = await _db.ClassTypes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Class type with ID {id} not found.");

        if (await _db.ClassTypes.AnyAsync(ct => ct.Name == dto.Name && ct.Id != id))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.", 409);

        if (!Enum.TryParse<DifficultyLevel>(dto.DifficultyLevel, true, out var dl))
            throw new BusinessRuleException($"Invalid difficulty level: {dto.DifficultyLevel}");

        classType.Name = dto.Name;
        classType.Description = dto.Description;
        classType.DefaultDurationMinutes = dto.DefaultDurationMinutes;
        classType.DefaultCapacity = dto.DefaultCapacity;
        classType.IsPremium = dto.IsPremium;
        classType.CaloriesPerSession = dto.CaloriesPerSession;
        classType.DifficultyLevel = dl;
        classType.IsActive = dto.IsActive;
        classType.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(classType);
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
        DifficultyLevel = ct.DifficultyLevel.ToString(),
        IsActive = ct.IsActive
    };
}
