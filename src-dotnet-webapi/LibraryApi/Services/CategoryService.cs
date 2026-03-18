using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface ICategoryService
{
    Task<PaginatedResponse<CategoryResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<CategoryDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken ct);
    Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}

public class CategoryService(LibraryDbContext db) : ICategoryService
{
    public async Task<PaginatedResponse<CategoryResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var totalCount = await db.Categories.CountAsync(ct);

        var items = await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new CategoryResponse { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync(ct);

        return PaginatedResponse<CategoryResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<CategoryDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Categories.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                BookCount = c.BookCategories.Count
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        if (await db.Categories.AnyAsync(c => c.Name == request.Name, ct))
            throw new InvalidOperationException($"A category with name '{request.Name}' already exists.");

        var category = new Category { Name = request.Name, Description = request.Description };
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return new CategoryResponse { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([id], ct);
        if (category is null) return null;

        if (await db.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id, ct))
            throw new InvalidOperationException($"A category with name '{request.Name}' already exists.");

        category.Name = request.Name;
        category.Description = request.Description;
        await db.SaveChangesAsync(ct);

        return new CategoryResponse { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var category = await db.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Category with ID {id} not found.");

        if (category.BookCategories.Count > 0)
            throw new InvalidOperationException("Cannot delete a category that has associated books.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
    }
}
