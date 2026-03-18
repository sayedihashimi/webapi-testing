using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
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

    public async Task<PagedResult<AuthorDto>> GetAuthorsAsync(string? search, PaginationParams pagination)
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
            .Select(a => new AuthorDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Biography = a.Biography,
                BirthDate = a.BirthDate,
                Country = a.Country,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<AuthorDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<AuthorDetailDto> GetAuthorByIdAsync(int id)
    {
        var author = await _db.Authors
            .Include(a => a.BookAuthors)
            .ThenInclude(ba => ba.Book)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException($"Author with ID {id} not found");

        return new AuthorDetailDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            Biography = author.Biography,
            BirthDate = author.BirthDate,
            Country = author.Country,
            CreatedAt = author.CreatedAt,
            Books = author.BookAuthors.Select(ba => new BookSummaryDto
            {
                Id = ba.Book.Id,
                Title = ba.Book.Title,
                ISBN = ba.Book.ISBN,
                TotalCopies = ba.Book.TotalCopies,
                AvailableCopies = ba.Book.AvailableCopies
            }).ToList()
        };
    }

    public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto dto)
    {
        var author = new Author
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Biography = dto.Biography,
            BirthDate = dto.BirthDate,
            Country = dto.Country,
            CreatedAt = DateTime.UtcNow
        };

        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created author: {FirstName} {LastName} (ID: {Id})", author.FirstName, author.LastName, author.Id);

        return new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            Biography = author.Biography,
            BirthDate = author.BirthDate,
            Country = author.Country,
            CreatedAt = author.CreatedAt
        };
    }

    public async Task<AuthorDto> UpdateAuthorAsync(int id, UpdateAuthorDto dto)
    {
        var author = await _db.Authors.FindAsync(id)
            ?? throw new NotFoundException($"Author with ID {id} not found");

        author.FirstName = dto.FirstName;
        author.LastName = dto.LastName;
        author.Biography = dto.Biography;
        author.BirthDate = dto.BirthDate;
        author.Country = dto.Country;

        await _db.SaveChangesAsync();

        return new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            Biography = author.Biography,
            BirthDate = author.BirthDate,
            Country = author.Country,
            CreatedAt = author.CreatedAt
        };
    }

    public async Task DeleteAuthorAsync(int id)
    {
        var author = await _db.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException($"Author with ID {id} not found");

        if (author.BookAuthors.Any())
            throw new BusinessRuleException("Cannot delete author with associated books. Remove the books first.", 409);

        _db.Authors.Remove(author);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted author: {FirstName} {LastName} (ID: {Id})", author.FirstName, author.LastName, author.Id);
    }
}
