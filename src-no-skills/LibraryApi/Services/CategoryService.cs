using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class CategoryService : ICategoryService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(LibraryDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<CategoryDto>> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await _context.Categories.CountAsync();
        var items = await _context.Categories
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync();

        return new PaginatedResponse<CategoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CategoryDetailDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return null;

        return new CategoryDetailDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            BookCount = category.BookCategories.Count
        };
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);
        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        category.Name = dto.Name;
        category.Description = dto.Description;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated category {CategoryId}", id);
        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return (false, "Category not found");

        if (category.BookCategories.Any())
            return (false, "Cannot delete category with associated books. Remove book associations first.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted category {CategoryId}", id);
        return (true, null);
    }
}
