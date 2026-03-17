using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class CategoryService : ICategoryService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(LibraryDbContext db, ILogger<CategoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();
    }

    public async Task<CategoryDetailDto?> GetByIdAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return null;

        return new CategoryDetailDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            BookCount = category.BookCategories.Count
        };
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name))
            throw new InvalidOperationException($"Category '{dto.Name}' already exists");

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created category {Id}: {Name}", category.Id, category.Name);

        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<CategoryDto?> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return null;

        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new InvalidOperationException($"Category '{dto.Name}' already exists");

        category.Name = dto.Name;
        category.Description = dto.Description;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated category {Id}", id);

        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            throw new KeyNotFoundException($"Category with id {id} not found");

        if (category.BookCategories.Any())
            throw new InvalidOperationException($"Cannot delete category with id {id} because it has associated books");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted category {Id}", id);

        return true;
    }
}
