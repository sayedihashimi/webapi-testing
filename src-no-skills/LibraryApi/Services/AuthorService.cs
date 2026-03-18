using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<AuthorService> _logger;

    public AuthorService(LibraryDbContext context, ILogger<AuthorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<AuthorDto>> GetAllAsync(string? search, int page, int pageSize)
    {
        var query = _context.Authors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(a => a.FirstName.ToLower().Contains(s) || a.LastName.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return new PaginatedResponse<AuthorDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AuthorDetailDto?> GetByIdAsync(int id)
    {
        var author = await _context.Authors
            .Include(a => a.BookAuthors)
                .ThenInclude(ba => ba.Book)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null) return null;

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

    public async Task<AuthorDto> CreateAsync(CreateAuthorDto dto)
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

        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created author {AuthorId}: {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);
        return MapToDto(author);
    }

    public async Task<AuthorDto?> UpdateAsync(int id, UpdateAuthorDto dto)
    {
        var author = await _context.Authors.FindAsync(id);
        if (author == null) return null;

        author.FirstName = dto.FirstName;
        author.LastName = dto.LastName;
        author.Biography = dto.Biography;
        author.BirthDate = dto.BirthDate;
        author.Country = dto.Country;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated author {AuthorId}", id);
        return MapToDto(author);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var author = await _context.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null) return (false, "Author not found");

        if (author.BookAuthors.Any())
            return (false, "Cannot delete author with associated books. Remove book associations first.");

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted author {AuthorId}", id);
        return (true, null);
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
