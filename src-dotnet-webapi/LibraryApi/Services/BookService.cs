using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PaginatedResponse<BookResponse>> GetAllAsync(string? search, int? categoryId, bool? available, string? sortBy, int page, int pageSize, CancellationToken ct);
    Task<BookResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken ct);
    Task<BookResponse?> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<ReservationResponse>> GetBookReservationsAsync(int bookId, int page, int pageSize, CancellationToken ct);
}

public class BookService(LibraryDbContext db) : IBookService
{
    public async Task<PaginatedResponse<BookResponse>> GetAllAsync(
        string? search, int? categoryId, bool? available, string? sortBy, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

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

        if (categoryId.HasValue)
            query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId.Value));

        if (available == true)
            query = query.Where(b => b.AvailableCopies > 0);
        else if (available == false)
            query = query.Where(b => b.AvailableCopies == 0);

        var totalCount = await query.CountAsync(ct);

        query = sortBy?.ToLower() switch
        {
            "year" => query.OrderByDescending(b => b.PublicationYear),
            "title" => query.OrderBy(b => b.Title),
            _ => query.OrderBy(b => b.Title)
        };

        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => MapToResponse(b))
            .ToListAsync(ct);

        return PaginatedResponse<BookResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<BookResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .Where(b => b.Id == id)
            .Select(b => MapToResponse(b))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken ct)
    {
        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN, ct))
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
            CreatedAt = DateTime.UtcNow
        };

        db.Books.Add(book);

        foreach (var authorId in request.AuthorIds)
        {
            if (!await db.Authors.AnyAsync(a => a.Id == authorId, ct))
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            db.BookAuthors.Add(new BookAuthor { Book = book, AuthorId = authorId });
        }

        foreach (var categoryId in request.CategoryIds)
        {
            if (!await db.Categories.AnyAsync(c => c.Id == categoryId, ct))
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            db.BookCategories.Add(new BookCategory { Book = book, CategoryId = categoryId });
        }

        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(book.Id, ct))!;
    }

    public async Task<BookResponse?> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct)
    {
        var book = await db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null) return null;

        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN && b.Id != id, ct))
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");

        var activeLoans = await db.Loans.CountAsync(l => l.BookId == id && l.Status != LoanStatus.Returned, ct);
        if (request.TotalCopies < activeLoans)
            throw new InvalidOperationException($"Cannot reduce total copies below active loans count ({activeLoans}).");

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language ?? "English";
        book.AvailableCopies += request.TotalCopies - book.TotalCopies;
        book.TotalCopies = request.TotalCopies;
        book.UpdatedAt = DateTime.UtcNow;

        // Replace authors
        db.BookAuthors.RemoveRange(book.BookAuthors);
        foreach (var authorId in request.AuthorIds)
        {
            if (!await db.Authors.AnyAsync(a => a.Id == authorId, ct))
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        // Replace categories
        db.BookCategories.RemoveRange(book.BookCategories);
        foreach (var categoryId in request.CategoryIds)
        {
            if (!await db.Categories.AnyAsync(c => c.Id == categoryId, ct))
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            db.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
        }

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(book.Id, ct))!;
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Book with ID {id} not found.");

        if (await db.Loans.AnyAsync(l => l.BookId == id && l.Status != LoanStatus.Returned, ct))
            throw new InvalidOperationException("Cannot delete a book with active loans.");

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId, ct))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => LoanService.MapToResponse(l))
            .ToListAsync(ct);

        return PaginatedResponse<LoanResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetBookReservationsAsync(int bookId, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId, ct))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => ReservationService.MapToResponse(r))
            .ToListAsync(ct);

        return PaginatedResponse<ReservationResponse>.Create(items, page, pageSize, totalCount);
    }

    private static BookResponse MapToResponse(Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        ISBN = b.ISBN,
        Publisher = b.Publisher,
        PublicationYear = b.PublicationYear,
        Description = b.Description,
        PageCount = b.PageCount,
        Language = b.Language,
        TotalCopies = b.TotalCopies,
        AvailableCopies = b.AvailableCopies,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt,
        Authors = b.BookAuthors.Select(ba => new BookAuthorResponse
        {
            Id = ba.Author.Id,
            FirstName = ba.Author.FirstName,
            LastName = ba.Author.LastName
        }).ToList(),
        Categories = b.BookCategories.Select(bc => new BookCategoryResponse
        {
            Id = bc.Category.Id,
            Name = bc.Category.Name
        }).ToList()
    };
}
