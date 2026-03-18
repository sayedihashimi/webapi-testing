using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

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

    public async Task<List<ClassTypeDto>> GetAllAsync(DifficultyLevel? difficulty, bool? isPremium)
    {
        var query = _db.ClassTypes.Where(ct => ct.IsActive);

        if (difficulty.HasValue)
            query = query.Where(ct => ct.DifficultyLevel == difficulty.Value);

        if (isPremium.HasValue)
            query = query.Where(ct => ct.IsPremium == isPremium.Value);

        return await query
            .OrderBy(ct => ct.Name)
            .Select(ct => MapToDto(ct))
            .ToListAsync();
    }

    public async Task<ClassTypeDto> GetByIdAsync(int id)
    {
        var classType = await _db.ClassTypes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Class type with ID {id} not found.");
        return MapToDto(classType);
    }

    public async Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto)
    {
        if (await _db.ClassTypes.AnyAsync(ct => ct.Name == dto.Name))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.", 409, "Conflict");

        var classType = new ClassType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDurationMinutes = dto.DefaultDurationMinutes,
            DefaultCapacity = dto.DefaultCapacity,
            IsPremium = dto.IsPremium,
            CaloriesPerSession = dto.CaloriesPerSession,
            DifficultyLevel = dto.DifficultyLevel
        };

        _db.ClassTypes.Add(classType);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created class type '{Name}' (ID {Id})", classType.Name, classType.Id);
        return MapToDto(classType);
    }

    public async Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto)
    {
        var classType = await _db.ClassTypes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Class type with ID {id} not found.");

        if (await _db.ClassTypes.AnyAsync(ct => ct.Name == dto.Name && ct.Id != id))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.", 409, "Conflict");

        classType.Name = dto.Name;
        classType.Description = dto.Description;
        classType.DefaultDurationMinutes = dto.DefaultDurationMinutes;
        classType.DefaultCapacity = dto.DefaultCapacity;
        classType.IsPremium = dto.IsPremium;
        classType.CaloriesPerSession = dto.CaloriesPerSession;
        classType.DifficultyLevel = dto.DifficultyLevel;
        classType.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated class type '{Name}' (ID {Id})", classType.Name, classType.Id);
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
        DifficultyLevel = ct.DifficultyLevel,
        IsActive = ct.IsActive,
        CreatedAt = ct.CreatedAt,
        UpdatedAt = ct.UpdatedAt
    };
}
