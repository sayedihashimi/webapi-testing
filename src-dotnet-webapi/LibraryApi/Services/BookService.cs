using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class BookService(LibraryDbContext db, ILogger<BookService> logger) : IBookService
{
    public async Task<PaginatedResponse<BookResponse>> GetAllAsync(
        string? search, int? categoryId, int? authorId,
        string? sortBy, string? sortDirection,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.Contains(term) ||
                (b.Publisher != null && b.Publisher.ToLower().Contains(term)));
        }

        if (categoryId.HasValue)
            query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId.Value));

        if (authorId.HasValue)
            query = query.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId.Value));

        var totalCount = await query.CountAsync(ct);

        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy?.ToLower()) switch
        {
            "year" => isDesc ? query.OrderByDescending(b => b.PublicationYear) : query.OrderBy(b => b.PublicationYear),
            "copies" => isDesc ? query.OrderByDescending(b => b.AvailableCopies) : query.OrderBy(b => b.AvailableCopies),
            _ => isDesc ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookResponse(
                b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear,
                b.Description, b.PageCount, b.Language,
                b.TotalCopies, b.AvailableCopies,
                b.CreatedAt, b.UpdatedAt,
                b.BookAuthors.Select(ba => $"{ba.Author.FirstName} {ba.Author.LastName}").ToList(),
                b.BookCategories.Select(bc => bc.Category.Name).ToList()))
            .ToListAsync(ct);

        return PaginatedResponse<BookResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<BookDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Books.AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BookDetailResponse(
                b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear,
                b.Description, b.PageCount, b.Language,
                b.TotalCopies, b.AvailableCopies,
                b.CreatedAt, b.UpdatedAt,
                b.BookAuthors.Select(ba => new BookAuthorResponse(ba.Author.Id, ba.Author.FirstName, ba.Author.LastName)).ToList(),
                b.BookCategories.Select(bc => new CategoryResponse(bc.Category.Id, bc.Category.Name, bc.Category.Description)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken ct)
    {
        var isbnExists = await db.Books.AsNoTracking().AnyAsync(b => b.ISBN == request.ISBN, ct);
        if (isbnExists)
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var authorId in request.AuthorIds)
        {
            var authorExists = await db.Authors.AsNoTracking().AnyAsync(a => a.Id == authorId, ct);
            if (!authorExists)
                throw new ArgumentException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }

        foreach (var categoryId in request.CategoryIds)
        {
            var categoryExists = await db.Categories.AsNoTracking().AnyAsync(c => c.Id == categoryId, ct);
            if (!categoryExists)
                throw new ArgumentException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });
        }

        db.Books.Add(book);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);

        var created = await db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == book.Id, ct);

        return MapToResponse(created);
    }

    public async Task<BookResponse?> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct)
    {
        var book = await db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null) return null;

        var isbnDuplicate = await db.Books.AsNoTracking().AnyAsync(b => b.Id != id && b.ISBN == request.ISBN, ct);
        if (isbnDuplicate)
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language ?? "English";

        // Adjust AvailableCopies based on TotalCopies change
        var copiesDiff = request.TotalCopies - book.TotalCopies;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = Math.Max(0, book.AvailableCopies + copiesDiff);
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        book.BookAuthors.Clear();
        foreach (var authorId in request.AuthorIds)
        {
            var authorExists = await db.Authors.AsNoTracking().AnyAsync(a => a.Id == authorId, ct);
            if (!authorExists)
                throw new ArgumentException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update categories
        book.BookCategories.Clear();
        foreach (var categoryId in request.CategoryIds)
        {
            var categoryExists = await db.Categories.AsNoTracking().AnyAsync(c => c.Id == categoryId, ct);
            if (!categoryExists)
                throw new ArgumentException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated book {BookId}", id);

        var updated = await db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == id, ct);

        return MapToResponse(updated);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var book = await db.Books
            .Include(b => b.Loans)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null)
            throw new KeyNotFoundException($"Book with ID {id} not found.");

        if (book.Loans.Any(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            throw new InvalidOperationException($"Cannot delete book with ID {id} because it has active loans.");

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted book {BookId}", id);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetLoansAsync(int bookId, int page, int pageSize, CancellationToken ct)
    {
        var exists = await db.Books.AsNoTracking().AnyAsync(b => b.Id == bookId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = db.Loans.AsNoTracking()
            .Where(l => l.BookId == bookId)
            .Include(l => l.Book)
            .Include(l => l.Patron);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<LoanResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<ReservationResponse>> GetReservationsAsync(int bookId, CancellationToken ct)
    {
        var exists = await db.Books.AsNoTracking().AnyAsync(b => b.Id == bookId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        return await db.Reservations.AsNoTracking()
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .OrderBy(r => r.QueuePosition)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync(ct);
    }

    private static BookResponse MapToResponse(Book b) =>
        new(b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear,
            b.Description, b.PageCount, b.Language,
            b.TotalCopies, b.AvailableCopies,
            b.CreatedAt, b.UpdatedAt,
            b.BookAuthors.Select(ba => $"{ba.Author.FirstName} {ba.Author.LastName}").ToList(),
            b.BookCategories.Select(bc => bc.Category.Name).ToList());
}
