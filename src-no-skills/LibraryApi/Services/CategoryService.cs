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

    public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(PaginationParams pagination)
    {
        var totalCount = await _db.Categories.CountAsync();

        var items = await _db.Categories
            .OrderBy(c => c.Name)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();

        return new PagedResult<CategoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<CategoryDetailDto> GetCategoryByIdAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException($"Category with ID {id} not found");

        return new CategoryDetailDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            BookCount = category.BookCategories.Count
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name))
            throw new BusinessRuleException($"A category with the name '{dto.Name}' already exists.", 409);

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created category: {Name} (ID: {Id})", category.Name, category.Id);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id)
            ?? throw new NotFoundException($"Category with ID {id} not found");

        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new BusinessRuleException($"A category with the name '{dto.Name}' already exists.", 409);

        category.Name = dto.Name;
        category.Description = dto.Description;

        await _db.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException($"Category with ID {id} not found");

        if (category.BookCategories.Any())
            throw new BusinessRuleException("Cannot delete category with associated books. Remove the books first.", 409);

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted category: {Name} (ID: {Id})", category.Name, category.Id);
    }
}
