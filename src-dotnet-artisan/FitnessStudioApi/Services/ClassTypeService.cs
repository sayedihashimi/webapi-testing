using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassTypeService(FitnessDbContext db, ILogger<ClassTypeService> logger) : IClassTypeService
{
    public async Task<List<ClassTypeResponse>> GetAllAsync(
        DifficultyLevel? difficulty, bool? isPremium, CancellationToken ct = default)
    {
        var query = db.ClassTypes.Where(ct2 => ct2.IsActive);

        if (difficulty.HasValue)
        {
            query = query.Where(ct2 => ct2.DifficultyLevel == difficulty.Value);
        }

        if (isPremium.HasValue)
        {
            query = query.Where(ct2 => ct2.IsPremium == isPremium.Value);
        }

        var types = await query.OrderBy(ct2 => ct2.Name).ToListAsync(ct);
        return types.Select(MapToResponse).ToList();
    }

    public async Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct);
        return classType is null ? null : MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct = default)
    {
        if (await db.ClassTypes.AnyAsync(ct2 => ct2.Name == request.Name, ct))
        {
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");
        }

        var classType = new ClassType
        {
            Name = request.Name,
            Description = request.Description,
            DefaultDurationMinutes = request.DefaultDurationMinutes,
            DefaultCapacity = request.DefaultCapacity,
            IsPremium = request.IsPremium,
            CaloriesPerSession = request.CaloriesPerSession,
            DifficultyLevel = request.DifficultyLevel
        };

        db.ClassTypes.Add(classType);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created class type {Name} with ID {Id}", classType.Name, classType.Id);

        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse?> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct = default)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct);
        if (classType is null)
        {
            return null;
        }

        if (await db.ClassTypes.AnyAsync(ct2 => ct2.Name == request.Name && ct2.Id != id, ct))
        {
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");
        }

        classType.Name = request.Name;
        classType.Description = request.Description;
        classType.DefaultDurationMinutes = request.DefaultDurationMinutes;
        classType.DefaultCapacity = request.DefaultCapacity;
        classType.IsPremium = request.IsPremium;
        classType.CaloriesPerSession = request.CaloriesPerSession;
        classType.DifficultyLevel = request.DifficultyLevel;
        classType.IsActive = request.IsActive;
        classType.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated class type {Id}", id);

        return MapToResponse(classType);
    }

    private static ClassTypeResponse MapToResponse(ClassType ct) => new(
        ct.Id, ct.Name, ct.Description, ct.DefaultDurationMinutes, ct.DefaultCapacity,
        ct.IsPremium, ct.CaloriesPerSession, ct.DifficultyLevel, ct.IsActive,
        ct.CreatedAt, ct.UpdatedAt);
}
