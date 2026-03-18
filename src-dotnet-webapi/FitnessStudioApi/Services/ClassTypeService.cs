using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class ClassTypeService(FitnessDbContext db, ILogger<ClassTypeService> logger) : IClassTypeService
{
    public async Task<PagedResponse<ClassTypeResponse>> GetAllAsync(string? difficulty, bool? isPremium, int page, int pageSize, CancellationToken ct)
    {
        var query = db.ClassTypes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var level))
            query = query.Where(c => c.DifficultyLevel == level);

        if (isPremium.HasValue)
            query = query.Where(c => c.IsPremium == isPremium.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToResponse(c))
            .ToListAsync(ct);

        return PagedResponse<ClassTypeResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var classType = await db.ClassTypes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return classType is null ? null : MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct)
    {
        var exists = await db.ClassTypes.AnyAsync(c => c.Name == request.Name, ct);
        if (exists)
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");

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
        logger.LogInformation("Created class type {ClassTypeId} '{Name}'", classType.Id, classType.Name);
        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse?> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct);
        if (classType is null) return null;

        var nameConflict = await db.ClassTypes.AnyAsync(c => c.Name == request.Name && c.Id != id, ct);
        if (nameConflict)
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");

        classType.Name = request.Name;
        classType.Description = request.Description;
        classType.DefaultDurationMinutes = request.DefaultDurationMinutes;
        classType.DefaultCapacity = request.DefaultCapacity;
        classType.IsPremium = request.IsPremium;
        classType.CaloriesPerSession = request.CaloriesPerSession;
        classType.DifficultyLevel = request.DifficultyLevel;
        classType.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated class type {ClassTypeId}", id);
        return MapToResponse(classType);
    }

    private static ClassTypeResponse MapToResponse(ClassType c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        DefaultDurationMinutes = c.DefaultDurationMinutes,
        DefaultCapacity = c.DefaultCapacity,
        IsPremium = c.IsPremium,
        CaloriesPerSession = c.CaloriesPerSession,
        DifficultyLevel = c.DifficultyLevel,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
