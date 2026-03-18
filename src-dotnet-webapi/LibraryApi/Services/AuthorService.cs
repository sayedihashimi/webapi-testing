using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class AuthorService(LibraryDbContext db) : IAuthorService
{
    public async Task<PagedResponse<AuthorResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Authors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(a => a.FirstName.ToLower().Contains(s) || a.LastName.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => MapToResponse(a))
            .ToListAsync(ct);

        return Paginate(items, page, pageSize, totalCount);
    }

    public async Task<AuthorDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var author = await db.Authors.AsNoTracking()
            .Include(a => a.BookAuthors).ThenInclude(ba => ba.Book)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (author is null) return null;

        return new AuthorDetailResponse
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            Biography = author.Biography,
            BirthDate = author.BirthDate,
            Country = author.Country,
            CreatedAt = author.CreatedAt,
            Books = author.BookAuthors.Select(ba => new AuthorBookResponse
            {
                Id = ba.Book.Id,
                Title = ba.Book.Title,
                ISBN = ba.Book.ISBN
            }).ToList()
        };
    }

    public async Task<AuthorResponse> CreateAsync(CreateAuthorRequest request, CancellationToken ct)
    {
        var author = new Author
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Biography = request.Biography,
            BirthDate = request.BirthDate,
            Country = request.Country,
            CreatedAt = DateTime.UtcNow
        };

        db.Authors.Add(author);
        await db.SaveChangesAsync(ct);

        return MapToResponse(author);
    }

    public async Task<AuthorResponse?> UpdateAsync(int id, UpdateAuthorRequest request, CancellationToken ct)
    {
        var author = await db.Authors.FindAsync([id], ct);
        if (author is null) return null;

        author.FirstName = request.FirstName;
        author.LastName = request.LastName;
        author.Biography = request.Biography;
        author.BirthDate = request.BirthDate;
        author.Country = request.Country;

        await db.SaveChangesAsync(ct);
        return MapToResponse(author);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var author = await db.Authors.Include(a => a.BookAuthors).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (author is null) throw new KeyNotFoundException($"Author with ID {id} not found.");
        if (author.BookAuthors.Count != 0) throw new InvalidOperationException("Cannot delete author with associated books.");

        db.Authors.Remove(author);
        await db.SaveChangesAsync(ct);
    }

    private static AuthorResponse MapToResponse(Author a) => new()
    {
        Id = a.Id,
        FirstName = a.FirstName,
        LastName = a.LastName,
        Biography = a.Biography,
        BirthDate = a.BirthDate,
        Country = a.Country,
        CreatedAt = a.CreatedAt
    };

    private static PagedResponse<T> Paginate<T>(List<T> items, int page, int pageSize, int totalCount) => new()
    {
        Items = items,
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        HasNextPage = page * pageSize < totalCount,
        HasPreviousPage = page > 1
    };
}
