using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class CategoryService(LibraryDbContext context, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<PaginatedResponse<CategoryResponse>> GetCategoriesAsync(int page, int pageSize, CancellationToken ct)
    {
        var totalCount = await context.Categories.CountAsync(ct);
        var items = await context.Categories
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync(ct);

        return new PaginatedResponse<CategoryResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    public async Task<CategoryDetailResponse?> GetCategoryByIdAsync(int id, CancellationToken ct)
    {
        var category = await context.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category is null)
        {
            return null;
        }

        return new CategoryDetailResponse(category.Id, category.Name, category.Description, category.BookCategories.Count);
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);
        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await context.Categories.FindAsync([id], ct);
        if (category is null)
        {
            return null;
        }

        category.Name = request.Name;
        category.Description = request.Description;

        await context.SaveChangesAsync(ct);
        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<(bool Found, bool HasBooks)> DeleteCategoryAsync(int id, CancellationToken ct)
    {
        var category = await context.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return (false, false);
        }

        if (category.BookCategories.Count > 0)
        {
            return (true, true);
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Deleted category {CategoryId}", id);
        return (true, false);
    }
}
