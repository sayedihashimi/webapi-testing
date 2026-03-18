using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class ClassTypeService : IClassTypeService
{
    private readonly FitnessDbContext _db;

    public ClassTypeService(FitnessDbContext db) => _db = db;

    public async Task<IReadOnlyList<ClassTypeDto>> GetAllAsync(string? difficulty, bool? isPremium)
    {
        var query = _db.ClassTypes.Where(ct => ct.IsActive);

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var dl))
            query = query.Where(ct => ct.DifficultyLevel == dl);

        if (isPremium.HasValue)
            query = query.Where(ct => ct.IsPremium == isPremium.Value);

        return await query.OrderBy(ct => ct.Name).Select(ct => ToDto(ct)).ToListAsync();
    }

    public async Task<ClassTypeDto?> GetByIdAsync(int id)
    {
        var ct = await _db.ClassTypes.FindAsync(id);
        return ct is null ? null : ToDto(ct);
    }

    public async Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto)
    {
        if (await _db.ClassTypes.AnyAsync(ct => ct.Name == dto.Name))
            throw new ConflictException($"A class type with name '{dto.Name}' already exists.");

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
        return ToDto(ct);
    }

    public async Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto)
    {
        var ct = await _db.ClassTypes.FindAsync(id)
            ?? throw new NotFoundException($"Class type with ID {id} not found.");

        if (await _db.ClassTypes.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new ConflictException($"A class type with name '{dto.Name}' already exists.");

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
        return ToDto(ct);
    }

    private static ClassTypeDto ToDto(ClassType ct) => new(
        ct.Id, ct.Name, ct.Description, ct.DefaultDurationMinutes, ct.DefaultCapacity,
        ct.IsPremium, ct.CaloriesPerSession, ct.DifficultyLevel, ct.IsActive,
        ct.CreatedAt, ct.UpdatedAt);
}
