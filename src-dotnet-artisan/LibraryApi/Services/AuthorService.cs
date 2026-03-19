using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class AuthorService(LibraryDbContext context, ILogger<AuthorService> logger) : IAuthorService
{
    public async Task<PaginatedResponse<AuthorResponse>> GetAuthorsAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Authors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(a => a.FirstName.ToLower().Contains(term) || a.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AuthorResponse(a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<AuthorResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    public async Task<AuthorDetailResponse?> GetAuthorByIdAsync(int id, CancellationToken ct)
    {
        var author = await context.Authors
            .Include(a => a.BookAuthors).ThenInclude(ba => ba.Book)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (author is null)
        {
            return null;
        }

        var books = author.BookAuthors.Select(ba => new AuthorBookResponse(ba.Book.Id, ba.Book.Title, ba.Book.ISBN)).ToList();
        return new AuthorDetailResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt, books);
    }

    public async Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request, CancellationToken ct)
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

        context.Authors.Add(author);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created author {AuthorId}: {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);
        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request, CancellationToken ct)
    {
        var author = await context.Authors.FindAsync([id], ct);
        if (author is null)
        {
            return null;
        }

        author.FirstName = request.FirstName;
        author.LastName = request.LastName;
        author.Biography = request.Biography;
        author.BirthDate = request.BirthDate;
        author.Country = request.Country;

        await context.SaveChangesAsync(ct);
        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<(bool Found, bool HasBooks)> DeleteAuthorAsync(int id, CancellationToken ct)
    {
        var author = await context.Authors.Include(a => a.BookAuthors).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (author is null)
        {
            return (false, false);
        }

        if (author.BookAuthors.Count > 0)
        {
            return (true, true);
        }

        context.Authors.Remove(author);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Deleted author {AuthorId}", id);
        return (true, false);
    }
}
