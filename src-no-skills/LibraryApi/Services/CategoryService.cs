using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Middleware;
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

    public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(int page, int pageSize)
    {
        var totalCount = await _db.Categories.CountAsync();
        var items = await _db.Categories
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync();

        return new PagedResult<CategoryDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<CategoryDetailDto> GetCategoryByIdAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException($"Category with ID {id} not found.");

        return new CategoryDetailDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            BookCount = category.BookCategories.Count
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto dto)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name))
            throw new ConflictException($"A category with name '{dto.Name}' already exists.");

        var category = new Category { Name = dto.Name, Description = dto.Description };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Category created: {CategoryId} - {Name}", category.Id, category.Name);
        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, CategoryUpdateDto dto)
    {
        var category = await _db.Categories.FindAsync(id)
            ?? throw new NotFoundException($"Category with ID {id} not found.");

        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new ConflictException($"A category with name '{dto.Name}' already exists.");

        category.Name = dto.Name;
        category.Description = dto.Description;
        await _db.SaveChangesAsync();
        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _db.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException($"Category with ID {id} not found.");

        if (category.BookCategories.Any())
            throw new ConflictException($"Cannot delete category with ID {id} because it has associated books.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Category deleted: {CategoryId}", id);
    }
}
