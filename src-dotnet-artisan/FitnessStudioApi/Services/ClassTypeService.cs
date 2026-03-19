using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassTypeService(AppDbContext db, ILogger<ClassTypeService> logger) : IClassTypeService
{
    public async Task<IReadOnlyList<ClassTypeResponse>> GetAllAsync(
        string? difficulty, bool? isPremium, CancellationToken ct)
    {
        var query = db.ClassTypes.Where(ct2 => ct2.IsActive);

        if (isPremium.HasValue)
        {
            query = query.Where(ct2 => ct2.IsPremium == isPremium.Value);
        }

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var level))
        {
            query = query.Where(ct2 => ct2.DifficultyLevel == level);
        }

        return await query
            .OrderBy(ct2 => ct2.Name)
            .Select(ct2 => MapToResponse(ct2))
            .ToListAsync(ct);
    }

    public async Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct);
        return classType is null ? null : MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct)
    {
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
        logger.LogInformation("Created class type {ClassName} with Id {ClassTypeId}", classType.Name, classType.Id);
        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse?> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct);
        if (classType is null)
        {
            return null;
        }

        classType.Name = request.Name;
        classType.Description = request.Description;
        classType.DefaultDurationMinutes = request.DefaultDurationMinutes;
        classType.DefaultCapacity = request.DefaultCapacity;
        classType.IsPremium = request.IsPremium;
        classType.CaloriesPerSession = request.CaloriesPerSession;
        classType.DifficultyLevel = request.DifficultyLevel;
        classType.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return MapToResponse(classType);
    }

    private static ClassTypeResponse MapToResponse(ClassType ct) =>
        new(ct.Id, ct.Name, ct.Description, ct.DefaultDurationMinutes,
            ct.DefaultCapacity, ct.IsPremium, ct.CaloriesPerSession,
            ct.DifficultyLevel, ct.IsActive);
}
