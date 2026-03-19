using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Middleware;
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

    public async Task<PagedResult<AuthorDto>> GetAuthorsAsync(string? search, int page, int pageSize)
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
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return new PagedResult<AuthorDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<AuthorDetailDto> GetAuthorByIdAsync(int id)
    {
        var author = await _db.Authors
            .Include(a => a.BookAuthors).ThenInclude(ba => ba.Book)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException($"Author with ID {id} not found.");

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

    public async Task<AuthorDto> CreateAuthorAsync(AuthorCreateDto dto)
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
        _logger.LogInformation("Author created: {AuthorId} - {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);
        return MapToDto(author);
    }

    public async Task<AuthorDto> UpdateAuthorAsync(int id, AuthorUpdateDto dto)
    {
        var author = await _db.Authors.FindAsync(id)
            ?? throw new NotFoundException($"Author with ID {id} not found.");

        author.FirstName = dto.FirstName;
        author.LastName = dto.LastName;
        author.Biography = dto.Biography;
        author.BirthDate = dto.BirthDate;
        author.Country = dto.Country;
        await _db.SaveChangesAsync();
        return MapToDto(author);
    }

    public async Task DeleteAuthorAsync(int id)
    {
        var author = await _db.Authors.Include(a => a.BookAuthors).FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException($"Author with ID {id} not found.");

        if (author.BookAuthors.Any())
            throw new ConflictException($"Cannot delete author with ID {id} because they have associated books.");

        _db.Authors.Remove(author);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Author deleted: {AuthorId}", id);
    }

    private static AuthorDto MapToDto(Author a) => new()
    {
        Id = a.Id,
        FirstName = a.FirstName,
        LastName = a.LastName,
        Biography = a.Biography,
        BirthDate = a.BirthDate,
        Country = a.Country,
        CreatedAt = a.CreatedAt
    };
}
