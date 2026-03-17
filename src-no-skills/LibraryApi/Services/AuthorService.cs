using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class AuthorService
{
    private readonly LibraryDbContext _db;

    public AuthorService(LibraryDbContext db) => _db = db;

    public async Task<PaginatedResponse<AuthorDto>> GetAllAsync(string? search, int page = 1, int pageSize = 10)
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

        return new PaginatedResponse<AuthorDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<AuthorDetailDto> GetByIdAsync(int id)
    {
        var author = await _db.Authors
            .Include(a => a.BookAuthors).ThenInclude(ba => ba.Book)
                .ThenInclude(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(a => a.BookAuthors).ThenInclude(ba => ba.Book)
                .ThenInclude(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException($"Author with ID {id} not found.");

        return new AuthorDetailDto
        {
            Id = author.Id, FirstName = author.FirstName, LastName = author.LastName,
            Biography = author.Biography, BirthDate = author.BirthDate, Country = author.Country,
            CreatedAt = author.CreatedAt,
            Books = author.BookAuthors.Select(ba => new BookSummaryDto
            {
                Id = ba.Book.Id, Title = ba.Book.Title, ISBN = ba.Book.ISBN,
                PublicationYear = ba.Book.PublicationYear,
                TotalCopies = ba.Book.TotalCopies, AvailableCopies = ba.Book.AvailableCopies,
                Authors = ba.Book.BookAuthors.Select(x => $"{x.Author.FirstName} {x.Author.LastName}").ToList(),
                Categories = ba.Book.BookCategories.Select(x => x.Category.Name).ToList()
            }).ToList()
        };
    }

    public async Task<AuthorDto> CreateAsync(CreateAuthorDto dto)
    {
        var author = new Author
        {
            FirstName = dto.FirstName, LastName = dto.LastName,
            Biography = dto.Biography, BirthDate = dto.BirthDate, Country = dto.Country
        };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();
        return MapToDto(author);
    }

    public async Task<AuthorDto> UpdateAsync(int id, UpdateAuthorDto dto)
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

    public async Task DeleteAsync(int id)
    {
        var author = await _db.Authors.Include(a => a.BookAuthors).FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException($"Author with ID {id} not found.");

        if (author.BookAuthors.Any())
            throw new BusinessRuleException("Cannot delete author with associated books.", 409);

        _db.Authors.Remove(author);
        await _db.SaveChangesAsync();
    }

    private static AuthorDto MapToDto(Author a) => new()
    {
        Id = a.Id, FirstName = a.FirstName, LastName = a.LastName,
        Biography = a.Biography, BirthDate = a.BirthDate, Country = a.Country,
        CreatedAt = a.CreatedAt
    };
}
