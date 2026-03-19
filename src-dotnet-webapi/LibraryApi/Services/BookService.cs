using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class BookService(LibraryDbContext db, ILogger<BookService> logger) : IBookService
{
    public async Task<PaginatedResponse<BookResponse>> GetAllAsync(
        string? search, string? category, bool? available, string? sortBy, string? sortOrder,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Books.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.Contains(term) ||
                b.BookAuthors.Any(ba =>
                    ba.Author.FirstName.ToLower().Contains(term) ||
                    ba.Author.LastName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.BookCategories.Any(bc => bc.Category.Name == category));
        }

        if (available.HasValue)
        {
            query = available.Value
                ? query.Where(b => b.AvailableCopies > 0)
                : query.Where(b => b.AvailableCopies == 0);
        }

        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("title", _) => query.OrderBy(b => b.Title),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            ("year", _) => query.OrderBy(b => b.PublicationYear),
            (_, "desc") => query.OrderByDescending(b => b.Title),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookResponse(
                b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear,
                b.Description, b.PageCount, b.Language, b.TotalCopies,
                b.AvailableCopies, b.CreatedAt, b.UpdatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<BookResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<BookDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Books.AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BookDetailResponse(
                b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear,
                b.Description, b.PageCount, b.Language, b.TotalCopies,
                b.AvailableCopies, b.CreatedAt, b.UpdatedAt,
                b.BookAuthors.Select(ba => new BookAuthorResponse(ba.Author.Id, ba.Author.FirstName, ba.Author.LastName)).ToList(),
                b.BookCategories.Select(bc => new BookCategoryResponse(bc.Category.Id, bc.Category.Name)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<BookDetailResponse> CreateAsync(CreateBookRequest request, CancellationToken ct)
    {
        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN, ct))
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");

        var authors = await db.Authors.Where(a => request.AuthorIds.Contains(a.Id)).ToListAsync(ct);
        if (authors.Count != request.AuthorIds.Count)
            throw new ArgumentException("One or more author IDs are invalid.");

        var categories = await db.Categories.Where(c => request.CategoryIds.Contains(c.Id)).ToListAsync(ct);
        if (categories.Count != request.CategoryIds.Count)
            throw new ArgumentException("One or more category IDs are invalid.");

        var book = new Book
        {
            Title = request.Title,
            ISBN = request.ISBN,
            Publisher = request.Publisher,
            PublicationYear = request.PublicationYear,
            Description = request.Description,
            PageCount = request.PageCount,
            Language = request.Language ?? "English",
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies,
            BookAuthors = request.AuthorIds.Select(aid => new BookAuthor { AuthorId = aid }).ToList(),
            BookCategories = request.CategoryIds.Select(cid => new BookCategory { CategoryId = cid }).ToList()
        };

        db.Books.Add(book);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);

        return new BookDetailResponse(
            book.Id, book.Title, book.ISBN, book.Publisher, book.PublicationYear,
            book.Description, book.PageCount, book.Language, book.TotalCopies,
            book.AvailableCopies, book.CreatedAt, book.UpdatedAt,
            authors.Select(a => new BookAuthorResponse(a.Id, a.FirstName, a.LastName)).ToList(),
            categories.Select(c => new BookCategoryResponse(c.Id, c.Name)).ToList());
    }

    public async Task<BookDetailResponse> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct)
    {
        var book = await db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Book with ID {id} not found.");

        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN && b.Id != id, ct))
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");

        var authors = await db.Authors.Where(a => request.AuthorIds.Contains(a.Id)).ToListAsync(ct);
        if (authors.Count != request.AuthorIds.Count)
            throw new ArgumentException("One or more author IDs are invalid.");

        var categories = await db.Categories.Where(c => request.CategoryIds.Contains(c.Id)).ToListAsync(ct);
        if (categories.Count != request.CategoryIds.Count)
            throw new ArgumentException("One or more category IDs are invalid.");

        var activeLoans = await db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active, ct);
        var copiesDiff = request.TotalCopies - book.TotalCopies;
        var newAvailable = book.AvailableCopies + copiesDiff;
        if (newAvailable < 0)
            throw new ArgumentException($"Cannot reduce total copies below active loans ({activeLoans}).");

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language ?? "English";
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = newAvailable;
        book.UpdatedAt = DateTime.UtcNow;

        book.BookAuthors.Clear();
        foreach (var aid in request.AuthorIds)
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = aid });

        book.BookCategories.Clear();
        foreach (var cid in request.CategoryIds)
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = cid });

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated book {BookId}", book.Id);

        return new BookDetailResponse(
            book.Id, book.Title, book.ISBN, book.Publisher, book.PublicationYear,
            book.Description, book.PageCount, book.Language, book.TotalCopies,
            book.AvailableCopies, book.CreatedAt, book.UpdatedAt,
            authors.Select(a => new BookAuthorResponse(a.Id, a.FirstName, a.LastName)).ToList(),
            categories.Select(c => new BookCategoryResponse(c.Id, c.Name)).ToList());
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Book with ID {id} not found.");

        var hasActiveLoans = await db.Loans.AnyAsync(l => l.BookId == id && l.Status == LoanStatus.Active, ct);
        if (hasActiveLoans)
            throw new InvalidOperationException($"Cannot delete book with ID {id} because it has active loans.");

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted book {BookId}", id);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetLoansAsync(int bookId, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId, ct))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = db.Loans.AsNoTracking()
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status,
                l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<LoanResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<ReservationResponse>> GetReservationsAsync(int bookId, CancellationToken ct)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId, ct))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        return await db.Reservations.AsNoTracking()
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status,
                r.QueuePosition, r.CreatedAt))
            .ToListAsync(ct);
    }
}
