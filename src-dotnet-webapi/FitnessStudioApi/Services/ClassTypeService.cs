using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassTypeService(StudioDbContext db, ILogger<ClassTypeService> logger) : IClassTypeService
{
    public async Task<IReadOnlyList<ClassTypeResponse>> GetAllAsync(string? difficulty, bool? isPremium, CancellationToken ct)
    {
        var query = db.ClassTypes.AsNoTracking().Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var level))
            query = query.Where(c => c.DifficultyLevel == level);

        if (isPremium.HasValue)
            query = query.Where(c => c.IsPremium == isPremium.Value);

        return await query.OrderBy(c => c.Name)
            .Select(c => MapToResponse(c))
            .ToListAsync(ct);
    }

    public async Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var classType = await db.ClassTypes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return classType is null ? null : MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct)
    {
        if (await db.ClassTypes.AnyAsync(c => c.Name == request.Name, ct))
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");

        var classType = new ClassType
        {
            Name = request.Name,
            Description = request.Description,
            DefaultDurationMinutes = request.DefaultDurationMinutes,
            DefaultCapacity = request.DefaultCapacity,
            IsPremium = request.IsPremium,
            CaloriesPerSession = request.CaloriesPerSession,
            DifficultyLevel = request.DifficultyLevel,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.ClassTypes.Add(classType);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created class type {ClassTypeId}: {Name}", classType.Id, classType.Name);
        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Class type {id} not found.");

        if (await db.ClassTypes.AnyAsync(c => c.Name == request.Name && c.Id != id, ct))
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");

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
        logger.LogInformation("Updated class type {ClassTypeId}", id);
        return MapToResponse(classType);
    }

    private static ClassTypeResponse MapToResponse(ClassType c) =>
        new(c.Id, c.Name, c.Description, c.DefaultDurationMinutes, c.DefaultCapacity,
            c.IsPremium, c.CaloriesPerSession, c.DifficultyLevel.ToString(), c.IsActive,
            c.CreatedAt, c.UpdatedAt);
}
