using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class ClassTypeService : IClassTypeService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<ClassTypeService> _logger;

    public ClassTypeService(FitnessDbContext db, ILogger<ClassTypeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<ClassTypeResponseDto>> GetAllAsync(string? difficulty, bool? isPremium)
    {
        var query = _db.ClassTypes.Where(ct => ct.IsActive);

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var dl))
            query = query.Where(ct => ct.DifficultyLevel == dl);
        if (isPremium.HasValue)
            query = query.Where(ct => ct.IsPremium == isPremium.Value);

        return await query
            .OrderBy(ct => ct.Name)
            .Select(ct => MapToDto(ct))
            .ToListAsync();
    }

    public async Task<ClassTypeResponseDto?> GetByIdAsync(int id)
    {
        var ct = await _db.ClassTypes.FindAsync(id);
        return ct is null ? null : MapToDto(ct);
    }

    public async Task<ClassTypeResponseDto> CreateAsync(ClassTypeCreateDto dto)
    {
        if (await _db.ClassTypes.AnyAsync(ct => ct.Name == dto.Name))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.");

        if (!Enum.TryParse<DifficultyLevel>(dto.DifficultyLevel, true, out var difficultyLevel))
            throw new BusinessRuleException($"Invalid difficulty level '{dto.DifficultyLevel}'. Valid values: Beginner, Intermediate, Advanced, AllLevels");

        var classType = new ClassType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDurationMinutes = dto.DefaultDurationMinutes,
            DefaultCapacity = dto.DefaultCapacity,
            IsPremium = dto.IsPremium,
            CaloriesPerSession = dto.CaloriesPerSession,
            DifficultyLevel = difficultyLevel
        };

        _db.ClassTypes.Add(classType);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created class type {ClassTypeName} with ID {ClassTypeId}", classType.Name, classType.Id);
        return MapToDto(classType);
    }

    public async Task<ClassTypeResponseDto?> UpdateAsync(int id, ClassTypeUpdateDto dto)
    {
        var classType = await _db.ClassTypes.FindAsync(id);
        if (classType is null) return null;

        if (await _db.ClassTypes.AnyAsync(ct => ct.Name == dto.Name && ct.Id != id))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.");

        if (!Enum.TryParse<DifficultyLevel>(dto.DifficultyLevel, true, out var difficultyLevel))
            throw new BusinessRuleException($"Invalid difficulty level '{dto.DifficultyLevel}'.");

        classType.Name = dto.Name;
        classType.Description = dto.Description;
        classType.DefaultDurationMinutes = dto.DefaultDurationMinutes;
        classType.DefaultCapacity = dto.DefaultCapacity;
        classType.IsPremium = dto.IsPremium;
        classType.CaloriesPerSession = dto.CaloriesPerSession;
        classType.DifficultyLevel = difficultyLevel;
        classType.IsActive = dto.IsActive;
        classType.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated class type {ClassTypeId}", id);
        return MapToDto(classType);
    }

    private static ClassTypeResponseDto MapToDto(ClassType ct) => new()
    {
        Id = ct.Id,
        Name = ct.Name,
        Description = ct.Description,
        DefaultDurationMinutes = ct.DefaultDurationMinutes,
        DefaultCapacity = ct.DefaultCapacity,
        IsPremium = ct.IsPremium,
        CaloriesPerSession = ct.CaloriesPerSession,
        DifficultyLevel = ct.DifficultyLevel.ToString(),
        IsActive = ct.IsActive,
        CreatedAt = ct.CreatedAt,
        UpdatedAt = ct.UpdatedAt
    };
}
