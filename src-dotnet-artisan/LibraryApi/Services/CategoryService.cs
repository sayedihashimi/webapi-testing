using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class CategoryService(LibraryDbContext db, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync(ct);
    }

    public async Task<CategoryDetailDto?> GetCategoryByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailDto(
                c.Id,
                c.Name,
                c.Description,
                c.BookCategories.Count
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var category = await db.Categories.FindAsync([id], ct);
        if (category is null)
        {
            return null;
        }

        category.Name = dto.Name;
        category.Description = dto.Description;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated category {CategoryId}", id);

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<(bool Found, bool HasBooks)> DeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        var category = await db.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category is null)
        {
            return (false, false);
        }

        if (category.BookCategories.Count > 0)
        {
            return (true, true);
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted category {CategoryId}", id);

        return (true, false);
    }
}
