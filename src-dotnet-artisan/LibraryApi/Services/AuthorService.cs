using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class AuthorService(LibraryDbContext db, ILogger<AuthorService> logger) : IAuthorService
{
    public async Task<PagedResult<AuthorSummaryDto>> GetAuthorsAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Authors.AsQueryable();

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
            .Select(a => new AuthorSummaryDto(a.Id, a.FirstName, a.LastName, a.Country))
            .ToListAsync(ct);

        return new PagedResult<AuthorSummaryDto>(items, totalCount, page, pageSize);
    }

    public async Task<AuthorDetailDto?> GetAuthorByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Authors
            .Include(a => a.BookAuthors)
            .ThenInclude(ba => ba.Book)
            .Where(a => a.Id == id)
            .Select(a => new AuthorDetailDto(
                a.Id,
                a.FirstName,
                a.LastName,
                a.Biography,
                a.BirthDate,
                a.Country,
                a.CreatedAt,
                a.BookAuthors.Select(ba => new BookSummaryDto(
                    ba.Book.Id, ba.Book.Title, ba.Book.ISBN, ba.Book.Publisher,
                    ba.Book.PublicationYear, ba.Book.Language, ba.Book.TotalCopies, ba.Book.AvailableCopies
                )).ToList()
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AuthorDetailDto> CreateAuthorAsync(CreateAuthorDto dto, CancellationToken ct = default)
    {
        var author = new Author
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Biography = dto.Biography,
            BirthDate = dto.BirthDate,
            Country = dto.Country
        };

        db.Authors.Add(author);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created author {AuthorId}: {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);

        return new AuthorDetailDto(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt, []);
    }

    public async Task<AuthorDetailDto?> UpdateAuthorAsync(int id, UpdateAuthorDto dto, CancellationToken ct = default)
    {
        var author = await db.Authors.FindAsync([id], ct);
        if (author is null)
        {
            return null;
        }

        author.FirstName = dto.FirstName;
        author.LastName = dto.LastName;
        author.Biography = dto.Biography;
        author.BirthDate = dto.BirthDate;
        author.Country = dto.Country;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated author {AuthorId}", id);

        return await GetAuthorByIdAsync(id, ct);
    }

    public async Task<(bool Found, bool HasBooks)> DeleteAuthorAsync(int id, CancellationToken ct = default)
    {
        var author = await db.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (author is null)
        {
            return (false, false);
        }

        if (author.BookAuthors.Count > 0)
        {
            return (true, true);
        }

        db.Authors.Remove(author);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted author {AuthorId}", id);

        return (true, false);
    }
}
