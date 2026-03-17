using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class CategoryService
{
    private readonly LibraryDbContext _db;

    public CategoryService(LibraryDbContext db) => _db = db;

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _db.Categories.OrderBy(c => c.Name)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync();
    }

    public async Task<CategoryDetailDto> GetByIdAsync(int id)
    {
        var cat = await _db.Categories.Include(c => c.BookCategories)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException($"Category with ID {id} not found.");

        return new CategoryDetailDto
        {
            Id = cat.Id, Name = cat.Name, Description = cat.Description,
            BookCount = cat.BookCategories.Count
        };
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name))
            throw new BusinessRuleException("A category with this name already exists.", 409);

        var cat = new Category { Name = dto.Name, Description = dto.Description };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return new CategoryDto { Id = cat.Id, Name = cat.Name, Description = cat.Description };
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var cat = await _db.Categories.FindAsync(id)
            ?? throw new NotFoundException($"Category with ID {id} not found.");

        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new BusinessRuleException("A category with this name already exists.", 409);

        cat.Name = dto.Name;
        cat.Description = dto.Description;
        await _db.SaveChangesAsync();
        return new CategoryDto { Id = cat.Id, Name = cat.Name, Description = cat.Description };
    }

    public async Task DeleteAsync(int id)
    {
        var cat = await _db.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException($"Category with ID {id} not found.");

        if (cat.BookCategories.Any())
            throw new BusinessRuleException("Cannot delete category with associated books.", 409);

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
    }
}
