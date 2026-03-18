using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassTypeService(StudioDbContext db)
{
    public async Task<List<ClassTypeResponse>> GetAllAsync(
        string? difficulty, bool? isPremium, CancellationToken ct = default)
    {
        var query = db.ClassTypes.Where(c => c.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var dl))
        {
            query = query.Where(c => c.DifficultyLevel == dl);
        }

        if (isPremium.HasValue)
        {
            query = query.Where(c => c.IsPremium == isPremium.Value);
        }

        var types = await query.OrderBy(c => c.Name).ToListAsync(ct);
        return types.Select(ToResponse).ToList();
    }

    public async Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct);
        return classType is null ? null : ToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct = default)
    {
        if (await db.ClassTypes.AnyAsync(c => c.Name == request.Name, ct))
        {
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");
        }

        if (!Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, true, out var dl))
        {
            throw new InvalidOperationException($"Invalid difficulty level: '{request.DifficultyLevel}'.");
        }

        var classType = new ClassType
        {
            Name = request.Name,
            Description = request.Description,
            DefaultDurationMinutes = request.DefaultDurationMinutes,
            DefaultCapacity = request.DefaultCapacity,
            IsPremium = request.IsPremium,
            CaloriesPerSession = request.CaloriesPerSession,
            DifficultyLevel = dl
        };

        db.ClassTypes.Add(classType);
        await db.SaveChangesAsync(ct);
        return ToResponse(classType);
    }

    public async Task<ClassTypeResponse?> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct = default)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct);
        if (classType is null)
        {
            return null;
        }

        if (await db.ClassTypes.AnyAsync(c => c.Name == request.Name && c.Id != id, ct))
        {
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");
        }

        if (!Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, true, out var dl))
        {
            throw new InvalidOperationException($"Invalid difficulty level: '{request.DifficultyLevel}'.");
        }

        classType.Name = request.Name;
        classType.Description = request.Description;
        classType.DefaultDurationMinutes = request.DefaultDurationMinutes;
        classType.DefaultCapacity = request.DefaultCapacity;
        classType.IsPremium = request.IsPremium;
        classType.CaloriesPerSession = request.CaloriesPerSession;
        classType.DifficultyLevel = dl;
        classType.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return ToResponse(classType);
    }

    private static ClassTypeResponse ToResponse(ClassType c) => new(
        c.Id, c.Name, c.Description, c.DefaultDurationMinutes, c.DefaultCapacity,
        c.IsPremium, c.CaloriesPerSession, c.DifficultyLevel.ToString(), c.IsActive);
}
