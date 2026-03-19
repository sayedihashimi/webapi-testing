using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class AuthorService(LibraryDbContext db, ILogger<AuthorService> logger) : IAuthorService
{
    public async Task<PaginatedResponse<AuthorResponse>> GetAllAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Authors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a =>
                a.FirstName.ToLower().Contains(term) ||
                a.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuthorResponse(
                a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<AuthorResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<AuthorDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Authors.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AuthorDetailResponse(
                a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt,
                a.BookAuthors.Select(ba => new AuthorBookResponse(ba.Book.Id, ba.Book.Title, ba.Book.ISBN)).ToList()))
            .FirstOrDefaultAsync(ct);
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
        logger.LogInformation("Created author {AuthorId}: {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);

        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
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
        logger.LogInformation("Updated author {AuthorId}", id);

        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var author = await db.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (author is null)
            throw new KeyNotFoundException($"Author with ID {id} not found.");

        if (author.BookAuthors.Count > 0)
            throw new InvalidOperationException($"Cannot delete author with ID {id} because they have associated books.");

        db.Authors.Remove(author);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted author {AuthorId}", id);
    }
}
