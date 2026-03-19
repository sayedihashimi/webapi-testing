using LibraryApi.Data;
using LibraryApi.DTOs.Author;
using LibraryApi.DTOs.Common;
using LibraryApi.Models;
using LibraryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<AuthorService> _logger;

    public AuthorService(LibraryDbContext db, ILogger<AuthorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<AuthorListDto>> GetAllAsync(string? search, PaginationParams pagination)
    {
        var query = _db.Authors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(a => a.FirstName.ToLower().Contains(s) || a.LastName.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new AuthorListDto(
                a.Id, a.FirstName, a.LastName, a.Country,
                a.BookAuthors.Count))
            .ToListAsync();

        return new PagedResult<AuthorListDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<AuthorDetailDto> GetByIdAsync(int id)
    {
        var author = await _db.Authors
            .Include(a => a.BookAuthors).ThenInclude(ba => ba.Book)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Author with ID {id} not found.");

        return new AuthorDetailDto(
            author.Id, author.FirstName, author.LastName,
            author.Biography, author.BirthDate, author.Country, author.CreatedAt,
            author.BookAuthors.Select(ba => new AuthorBookDto(ba.Book.Id, ba.Book.Title, ba.Book.ISBN)).ToList());
    }

    public async Task<AuthorDetailDto> CreateAsync(CreateAuthorDto dto)
    {
        var author = new Author
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Biography = dto.Biography,
            BirthDate = dto.BirthDate,
            Country = dto.Country
        };

        _db.Authors.Add(author);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created author {AuthorId}: {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);

        return await GetByIdAsync(author.Id);
    }

    public async Task<AuthorDetailDto> UpdateAsync(int id, UpdateAuthorDto dto)
    {
        var author = await _db.Authors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Author with ID {id} not found.");

        author.FirstName = dto.FirstName;
        author.LastName = dto.LastName;
        author.Biography = dto.Biography;
        author.BirthDate = dto.BirthDate;
        author.Country = dto.Country;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated author {AuthorId}", id);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var author = await _db.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Author with ID {id} not found.");

        if (author.BookAuthors.Count != 0)
            throw new InvalidOperationException($"Cannot delete author with ID {id} because they have {author.BookAuthors.Count} associated book(s).");

        _db.Authors.Remove(author);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted author {AuthorId}", id);
    }
}
