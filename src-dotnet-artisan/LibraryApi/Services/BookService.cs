using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class BookService(LibraryDbContext context, ILogger<BookService> logger) : IBookService
{
    public async Task<PaginatedResponse<BookResponse>> GetBooksAsync(
        string? search, string? category, bool? available, string? sortBy,
        int page, int pageSize, CancellationToken ct)
    {
        var query = context.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.ToLower().Contains(term) ||
                b.BookAuthors.Any(ba => ba.Author.FirstName.ToLower().Contains(term) || ba.Author.LastName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == category.ToLower()));
        }

        if (available.HasValue)
        {
            query = available.Value
                ? query.Where(b => b.AvailableCopies > 0)
                : query.Where(b => b.AvailableCopies == 0);
        }

        query = sortBy?.ToLower() switch
        {
            "title" => query.OrderBy(b => b.Title),
            "year" => query.OrderByDescending(b => b.PublicationYear),
            "recent" => query.OrderByDescending(b => b.CreatedAt),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => new BookResponse(
                b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear, b.Language,
                b.TotalCopies, b.AvailableCopies,
                b.BookAuthors.Select(ba => ba.Author.FirstName + " " + ba.Author.LastName).ToList(),
                b.BookCategories.Select(bc => bc.Category.Name).ToList()))
            .ToListAsync(ct);

        return new PaginatedResponse<BookResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    public async Task<BookDetailResponse?> GetBookByIdAsync(int id, CancellationToken ct)
    {
        var book = await context.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null)
        {
            return null;
        }

        return MapToDetail(book);
    }

    public async Task<BookDetailResponse> CreateBookAsync(CreateBookRequest request, CancellationToken ct)
    {
        var book = new Book
        {
            Title = request.Title,
            ISBN = request.ISBN,
            Publisher = request.Publisher,
            PublicationYear = request.PublicationYear,
            Description = request.Description,
            PageCount = request.PageCount,
            Language = request.Language,
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Books.Add(book);
        await context.SaveChangesAsync(ct);

        // Add authors
        foreach (var authorId in request.AuthorIds)
        {
            context.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        // Add categories
        foreach (var categoryId in request.CategoryIds)
        {
            context.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
        }

        await context.SaveChangesAsync(ct);

        // Reload with includes
        var created = await context.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == book.Id, ct);

        logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);
        return MapToDetail(created);
    }

    public async Task<BookDetailResponse?> UpdateBookAsync(int id, UpdateBookRequest request, CancellationToken ct)
    {
        var book = await context.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null)
        {
            return null;
        }

        var activeLoans = await context.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active, ct);
        var newAvailable = request.TotalCopies - activeLoans;

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = Math.Max(0, newAvailable);
        book.UpdatedAt = DateTime.UtcNow;

        // Replace authors
        context.BookAuthors.RemoveRange(book.BookAuthors);
        foreach (var authorId in request.AuthorIds)
        {
            context.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Replace categories
        context.BookCategories.RemoveRange(book.BookCategories);
        foreach (var categoryId in request.CategoryIds)
        {
            context.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await context.SaveChangesAsync(ct);

        var updated = await context.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == id, ct);

        return MapToDetail(updated);
    }

    public async Task<(bool Found, bool HasActiveLoans)> DeleteBookAsync(int id, CancellationToken ct)
    {
        var book = await context.Books.FindAsync([id], ct);
        if (book is null)
        {
            return (false, false);
        }

        var hasActiveLoans = await context.Loans.AnyAsync(l => l.BookId == id && l.Status == LoanStatus.Active, ct);
        if (hasActiveLoans)
        {
            return (true, true);
        }

        context.Books.Remove(book);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Deleted book {BookId}", id);
        return (true, false);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync(ct);

        return new PaginatedResponse<LoanResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetBookReservationsAsync(int bookId, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync(ct);

        return new PaginatedResponse<ReservationResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    private static BookDetailResponse MapToDetail(Book book) =>
        new(book.Id, book.Title, book.ISBN, book.Publisher, book.PublicationYear,
            book.Description, book.PageCount, book.Language, book.TotalCopies, book.AvailableCopies,
            book.CreatedAt, book.UpdatedAt,
            book.BookAuthors.Select(ba => new AuthorResponse(ba.Author.Id, ba.Author.FirstName, ba.Author.LastName, ba.Author.Biography, ba.Author.BirthDate, ba.Author.Country, ba.Author.CreatedAt)).ToList(),
            book.BookCategories.Select(bc => new CategoryResponse(bc.Category.Id, bc.Category.Name, bc.Category.Description)).ToList());
}
