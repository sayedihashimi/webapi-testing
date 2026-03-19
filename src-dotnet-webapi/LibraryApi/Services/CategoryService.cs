using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class CategoryService(LibraryDbContext db, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct)
    {
        return await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync(ct);
    }

    public async Task<CategoryDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Categories.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailResponse(
                c.Id, c.Name, c.Description, c.BookCategories.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        var exists = await db.Categories.AsNoTracking()
            .AnyAsync(c => c.Name.ToLower() == request.Name.ToLower(), ct);
        if (exists)
            throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([id], ct);
        if (category is null) return null;

        var duplicate = await db.Categories.AsNoTracking()
            .AnyAsync(c => c.Id != id && c.Name.ToLower() == request.Name.ToLower(), ct);
        if (duplicate)
            throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");

        category.Name = request.Name;
        category.Description = request.Description;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated category {CategoryId}", id);

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var category = await db.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category is null)
            throw new KeyNotFoundException($"Category with ID {id} not found.");

        if (category.BookCategories.Count > 0)
            throw new InvalidOperationException($"Cannot delete category with ID {id} because it has associated books.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted category {CategoryId}", id);
    }
}
