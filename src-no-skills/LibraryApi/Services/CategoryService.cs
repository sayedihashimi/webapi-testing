using LibraryApi.Data;
using LibraryApi.DTOs.Category;
using LibraryApi.Models;
using LibraryApi.Services.Interfaces;
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

    public async Task<List<CategoryListDto>> GetAllAsync()
    {
        return await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryListDto(c.Id, c.Name, c.Description, c.BookCategories.Count))
            .ToListAsync();
    }

    public async Task<CategoryDetailDto> GetByIdAsync(int id)
    {
        var cat = await _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailDto(c.Id, c.Name, c.Description, c.BookCategories.Count))
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Category with ID {id} not found.");
        return cat;
    }

    public async Task<CategoryDetailDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name))
            throw new ArgumentException($"A category with name '{dto.Name}' already exists.");

        var category = new Category { Name = dto.Name, Description = dto.Description };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);

        return await GetByIdAsync(category.Id);
    }

    public async Task<CategoryDetailDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category with ID {id} not found.");

        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new ArgumentException($"A category with name '{dto.Name}' already exists.");

        category.Name = dto.Name;
        category.Description = dto.Description;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated category {CategoryId}", id);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Category with ID {id} not found.");

        if (category.BookCategories.Count != 0)
            throw new InvalidOperationException($"Cannot delete category with ID {id} because it has {category.BookCategories.Count} associated book(s).");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted category {CategoryId}", id);
    }
}
