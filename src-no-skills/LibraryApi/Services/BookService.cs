using LibraryApi.Data;
using LibraryApi.DTOs.Book;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loan;
using LibraryApi.DTOs.Reservation;
using LibraryApi.Models;
using LibraryApi.Models.Enums;
using LibraryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<BookService> _logger;

    public BookService(LibraryDbContext db, ILogger<BookService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<BookListDto>> GetAllAsync(
        string? search, int? categoryId, int? authorId,
        string? sortBy, string? sortOrder, PaginationParams pagination)
    {
        var query = _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(s) || b.ISBN.Contains(s));
        }

        if (categoryId.HasValue)
            query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId.Value));

        if (authorId.HasValue)
            query = query.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId.Value));

        var totalCount = await query.CountAsync();

        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("year", "asc") => query.OrderBy(b => b.PublicationYear),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            ("created", "desc") => query.OrderByDescending(b => b.CreatedAt),
            _ => query.OrderBy(b => b.Title)
        };

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(b => new BookListDto(
                b.Id, b.Title, b.ISBN, b.Publisher,
                b.PublicationYear, b.Language, b.TotalCopies, b.AvailableCopies,
                b.BookAuthors.Select(ba => $"{ba.Author.FirstName} {ba.Author.LastName}").ToList(),
                b.BookCategories.Select(bc => bc.Category.Name).ToList()))
            .ToListAsync();

        return new PagedResult<BookListDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<BookDetailDto> GetByIdAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Book with ID {id} not found.");

        return new BookDetailDto(
            book.Id, book.Title, book.ISBN, book.Publisher,
            book.PublicationYear, book.Description, book.PageCount,
            book.Language, book.TotalCopies, book.AvailableCopies,
            book.CreatedAt, book.UpdatedAt,
            book.BookAuthors.Select(ba => new BookAuthorDto(ba.Author.Id, ba.Author.FirstName, ba.Author.LastName)).ToList(),
            book.BookCategories.Select(bc => new BookCategoryDto(bc.Category.Id, bc.Category.Name)).ToList());
    }

    public async Task<BookDetailDto> CreateAsync(CreateBookDto dto)
    {
        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN))
            throw new ArgumentException($"A book with ISBN '{dto.ISBN}' already exists.");

        if (dto.PageCount.HasValue && dto.PageCount <= 0)
            throw new ArgumentException("PageCount must be positive.");

        var book = new Book
        {
            Title = dto.Title,
            ISBN = dto.ISBN,
            Publisher = dto.Publisher,
            PublicationYear = dto.PublicationYear,
            Description = dto.Description,
            PageCount = dto.PageCount,
            Language = dto.Language,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        // Add author relationships
        foreach (var authorId in dto.AuthorIds)
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            _db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        // Add category relationships
        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            _db.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);

        return await GetByIdAsync(book.Id);
    }

    public async Task<BookDetailDto> UpdateAsync(int id, UpdateBookDto dto)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Book with ID {id} not found.");

        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN && b.Id != id))
            throw new ArgumentException($"A book with ISBN '{dto.ISBN}' already exists.");

        var activeLoanCount = await _db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        var copiesDiff = dto.TotalCopies - book.TotalCopies;

        book.Title = dto.Title;
        book.ISBN = dto.ISBN;
        book.Publisher = dto.Publisher;
        book.PublicationYear = dto.PublicationYear;
        book.Description = dto.Description;
        book.PageCount = dto.PageCount;
        book.Language = dto.Language;
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = Math.Max(0, book.AvailableCopies + copiesDiff);
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        _db.BookAuthors.RemoveRange(book.BookAuthors);
        foreach (var authorId in dto.AuthorIds)
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            _db.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update categories
        _db.BookCategories.RemoveRange(book.BookCategories);
        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            _db.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated book {BookId}", id);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var book = await _db.Books.FindAsync(id)
            ?? throw new KeyNotFoundException($"Book with ID {id} not found.");

        var activeLoans = await _db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        if (activeLoans > 0)
            throw new InvalidOperationException($"Cannot delete book with ID {id} because it has {activeLoans} active loan(s).");

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted book {BookId}", id);
    }

    public async Task<PagedResult<LoanListDto>> GetBookLoansAsync(int bookId, PaginationParams pagination)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(l => new LoanListDto(
                l.Id, l.Book.Title, $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync();

        return new PagedResult<LoanListDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<List<ReservationListDto>> GetBookReservationsAsync(int bookId)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        return await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .Select(r => new ReservationListDto(
                r.Id, r.Book.Title, $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync();
    }
}
