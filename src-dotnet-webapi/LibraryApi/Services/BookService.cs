using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService(LibraryDbContext db) : IBookService
{
    public async Task<PagedResponse<BookResponse>> GetAllAsync(
        string? search, string? category, bool? available, string? sortBy,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(s) ||
                b.ISBN.ToLower().Contains(s) ||
                b.BookAuthors.Any(ba => ba.Author.FirstName.ToLower().Contains(s) || ba.Author.LastName.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.ToLower();
            query = query.Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower().Contains(cat)));
        }

        if (available == true)
            query = query.Where(b => b.AvailableCopies > 0);
        else if (available == false)
            query = query.Where(b => b.AvailableCopies == 0);

        query = sortBy?.ToLower() switch
        {
            "title" => query.OrderBy(b => b.Title),
            "title_desc" => query.OrderByDescending(b => b.Title),
            "year" => query.OrderBy(b => b.PublicationYear),
            "year_desc" => query.OrderByDescending(b => b.PublicationYear),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<BookResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            HasNextPage = page * pageSize < totalCount,
            HasPreviousPage = page > 1
        };
    }

    public async Task<BookResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var book = await db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return book is null ? null : MapToResponse(book);
    }

    public async Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken ct)
    {
        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN, ct))
            throw new ArgumentException($"A book with ISBN '{request.ISBN}' already exists.");

        var now = DateTime.UtcNow;
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
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Books.Add(book);
        await db.SaveChangesAsync(ct);

        // Add authors
        foreach (var authorId in request.AuthorIds)
        {
            if (!await db.Authors.AnyAsync(a => a.Id == authorId, ct))
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        // Add categories
        foreach (var categoryId in request.CategoryIds)
        {
            if (!await db.Categories.AnyAsync(c => c.Id == categoryId, ct))
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            db.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
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
            throw new ArgumentException($"A book with ISBN '{request.ISBN}' already exists.");

        var activeLoans = await db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active, ct);
        var newAvailable = request.TotalCopies - activeLoans;
        if (newAvailable < 0)
            throw new ArgumentException($"Cannot set TotalCopies to {request.TotalCopies} — there are {activeLoans} active loans.");

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

        // Update authors
        db.BookAuthors.RemoveRange(book.BookAuthors);
        foreach (var authorId in request.AuthorIds)
        {
            if (!await db.Authors.AnyAsync(a => a.Id == authorId, ct))
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        // Update categories
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
        var book = await db.Books.Include(b => b.Loans).FirstOrDefaultAsync(b => b.Id == id, ct);
        if (book is null) throw new KeyNotFoundException($"Book with ID {id} not found.");
        if (book.Loans.Any(l => l.Status == LoanStatus.Active))
            throw new InvalidOperationException("Cannot delete book with active loans.");

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<LoanResponse>> GetLoansAsync(int bookId, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId, ct))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => MapLoanToResponse(l))
            .ToListAsync(ct);

        return new PagedResponse<LoanResponse>
        {
            Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            HasNextPage = page * pageSize < totalCount, HasPreviousPage = page > 1
        };
    }

    public async Task<List<ReservationResponse>> GetReservationsAsync(int bookId, CancellationToken ct)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId, ct))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        return await db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .Select(r => new ReservationResponse
            {
                Id = r.Id,
                BookId = r.BookId,
                BookTitle = r.Book.Title,
                PatronId = r.PatronId,
                PatronName = r.Patron.FirstName + " " + r.Patron.LastName,
                ReservationDate = r.ReservationDate,
                ExpirationDate = r.ExpirationDate,
                Status = r.Status,
                QueuePosition = r.QueuePosition,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);
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
        Authors = b.BookAuthors.Select(ba => new AuthorSummaryResponse
        {
            Id = ba.Author.Id,
            FirstName = ba.Author.FirstName,
            LastName = ba.Author.LastName
        }).ToList(),
        Categories = b.BookCategories.Select(bc => new CategorySummaryResponse
        {
            Id = bc.Category.Id,
            Name = bc.Category.Name
        }).ToList()
    };

    private static LoanResponse MapLoanToResponse(Loan l) => new()
    {
        Id = l.Id,
        BookId = l.BookId,
        BookTitle = l.Book.Title,
        BookISBN = l.Book.ISBN,
        PatronId = l.PatronId,
        PatronName = l.Patron.FirstName + " " + l.Patron.LastName,
        LoanDate = l.LoanDate,
        DueDate = l.DueDate,
        ReturnDate = l.ReturnDate,
        Status = l.Status,
        RenewalCount = l.RenewalCount,
        CreatedAt = l.CreatedAt
    };
}
