using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class BookService(LibraryDbContext db, ILogger<BookService> logger) : IBookService
{
    public async Task<PagedResult<BookSummaryDto>> GetBooksAsync(
        string? search, string? category, bool? available,
        string? sortBy, string? sortDirection,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.Contains(term) ||
                b.BookAuthors.Any(ba =>
                    ba.Author.FirstName.ToLower().Contains(term) ||
                    ba.Author.LastName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim().ToLower();
            query = query.Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == cat));
        }

        if (available == true)
        {
            query = query.Where(b => b.AvailableCopies > 0);
        }
        else if (available == false)
        {
            query = query.Where(b => b.AvailableCopies == 0);
        }

        query = (sortBy?.ToLower(), sortDirection?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("title", _) => query.OrderBy(b => b.Title),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            ("year", _) => query.OrderBy(b => b.PublicationYear),
            ("available", "desc") => query.OrderByDescending(b => b.AvailableCopies),
            ("available", _) => query.OrderBy(b => b.AvailableCopies),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookSummaryDto(
                b.Id, b.Title, b.ISBN, b.Publisher,
                b.PublicationYear, b.Language, b.TotalCopies, b.AvailableCopies
            ))
            .ToListAsync(ct);

        return new PagedResult<BookSummaryDto>(items, totalCount, page, pageSize);
    }

    public async Task<BookDetailDto?> GetBookByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .Where(b => b.Id == id)
            .Select(b => new BookDetailDto(
                b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear,
                b.Description, b.PageCount, b.Language,
                b.TotalCopies, b.AvailableCopies,
                b.CreatedAt, b.UpdatedAt,
                b.BookAuthors.Select(ba => new AuthorSummaryDto(
                    ba.Author.Id, ba.Author.FirstName, ba.Author.LastName, ba.Author.Country
                )).ToList(),
                b.BookCategories.Select(bc => new CategoryDto(
                    bc.Category.Id, bc.Category.Name, bc.Category.Description
                )).ToList()
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<BookDetailDto> CreateBookAsync(CreateBookDto dto, CancellationToken ct = default)
    {
        var book = new Book
        {
            Title = dto.Title,
            ISBN = dto.ISBN,
            Publisher = dto.Publisher,
            PublicationYear = dto.PublicationYear,
            Description = dto.Description,
            PageCount = dto.PageCount,
            Language = dto.Language ?? "English",
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies
        };

        db.Books.Add(book);
        await db.SaveChangesAsync(ct);

        foreach (var authorId in dto.AuthorIds)
        {
            db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        foreach (var categoryId in dto.CategoryIds)
        {
            db.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);

        return (await GetBookByIdAsync(book.Id, ct))!;
    }

    public async Task<BookDetailDto?> UpdateBookAsync(int id, UpdateBookDto dto, CancellationToken ct = default)
    {
        var book = await db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null)
        {
            return null;
        }

        var activeLoans = await db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active, ct);
        var copiesDiff = dto.TotalCopies - book.TotalCopies;

        book.Title = dto.Title;
        book.ISBN = dto.ISBN;
        book.Publisher = dto.Publisher;
        book.PublicationYear = dto.PublicationYear;
        book.Description = dto.Description;
        book.PageCount = dto.PageCount;
        book.Language = dto.Language ?? "English";
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = Math.Max(0, book.AvailableCopies + copiesDiff);
        book.UpdatedAt = DateTime.UtcNow;

        db.BookAuthors.RemoveRange(book.BookAuthors);
        db.BookCategories.RemoveRange(book.BookCategories);

        foreach (var authorId in dto.AuthorIds)
        {
            db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        foreach (var categoryId in dto.CategoryIds)
        {
            db.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated book {BookId}", id);

        return await GetBookByIdAsync(id, ct);
    }

    public async Task<(bool Found, bool HasActiveLoans)> DeleteBookAsync(int id, CancellationToken ct = default)
    {
        var book = await db.Books.FindAsync([id], ct);
        if (book is null)
        {
            return (false, false);
        }

        var hasActiveLoans = await db.Loans.AnyAsync(l => l.BookId == id && l.Status == LoanStatus.Active, ct);
        if (hasActiveLoans)
        {
            return (true, true);
        }

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted book {BookId}", id);

        return (true, false);
    }

    public async Task<List<LoanDto>> GetBookLoansAsync(int bookId, CancellationToken ct = default)
    {
        return await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate)
            .Select(l => new LoanDto(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt
            ))
            .ToListAsync(ct);
    }

    public async Task<List<ReservationDto>> GetBookReservationsAsync(int bookId, CancellationToken ct = default)
    {
        return await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .Select(r => new ReservationDto(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt
            ))
            .ToListAsync(ct);
    }
}
