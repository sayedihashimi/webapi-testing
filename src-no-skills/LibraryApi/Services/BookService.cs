using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Middleware;
using LibraryApi.Models;
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

    public async Task<PagedResult<BookDto>> GetBooksAsync(string? search, string? category, bool? available, string? sortBy, string? sortOrder, int page, int pageSize)
    {
        var query = _db.Books
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
            var c = category.ToLower();
            query = query.Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == c));
        }

        if (available == true)
            query = query.Where(b => b.AvailableCopies > 0);
        else if (available == false)
            query = query.Where(b => b.AvailableCopies == 0);

        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("title", _) => query.OrderBy(b => b.Title),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            ("year", _) => query.OrderBy(b => b.PublicationYear),
            ("created", "desc") => query.OrderByDescending(b => b.CreatedAt),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync();
        var books = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<BookDto>
        {
            Items = books.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BookDetailDto> GetBookByIdAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Book with ID {id} not found.");

        var dto = MapToDto(book);
        return new BookDetailDto
        {
            Id = dto.Id, Title = dto.Title, ISBN = dto.ISBN, Publisher = dto.Publisher,
            PublicationYear = dto.PublicationYear, Description = dto.Description, PageCount = dto.PageCount,
            Language = dto.Language, TotalCopies = dto.TotalCopies, AvailableCopies = dto.AvailableCopies,
            CreatedAt = dto.CreatedAt, UpdatedAt = dto.UpdatedAt,
            Authors = dto.Authors, Categories = dto.Categories
        };
    }

    public async Task<BookDto> CreateBookAsync(BookCreateDto dto)
    {
        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN))
            throw new ConflictException($"A book with ISBN '{dto.ISBN}' already exists.");

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
            AvailableCopies = dto.TotalCopies,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var authorId in dto.AuthorIds)
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new NotFoundException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }

        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new NotFoundException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });
        }

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Book created: {BookId} - {Title}", book.Id, book.Title);

        return await ReloadBookDto(book.Id);
    }

    public async Task<BookDto> UpdateBookAsync(int id, BookUpdateDto dto)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Book with ID {id} not found.");

        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN && b.Id != id))
            throw new ConflictException($"A book with ISBN '{dto.ISBN}' already exists.");

        var activeLoans = await _db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        var newAvailable = dto.TotalCopies - activeLoans;
        if (newAvailable < 0)
            throw new BusinessRuleException($"Cannot set TotalCopies to {dto.TotalCopies} because there are {activeLoans} active loans.");

        book.Title = dto.Title;
        book.ISBN = dto.ISBN;
        book.Publisher = dto.Publisher;
        book.PublicationYear = dto.PublicationYear;
        book.Description = dto.Description;
        book.PageCount = dto.PageCount;
        book.Language = dto.Language ?? "English";
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = newAvailable;
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        book.BookAuthors.Clear();
        foreach (var authorId in dto.AuthorIds)
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new NotFoundException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update categories
        book.BookCategories.Clear();
        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new NotFoundException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await _db.SaveChangesAsync();
        return await ReloadBookDto(id);
    }

    public async Task DeleteBookAsync(int id)
    {
        var book = await _db.Books.FindAsync(id)
            ?? throw new NotFoundException($"Book with ID {id} not found.");

        var hasActiveLoans = await _db.Loans.AnyAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans)
            throw new ConflictException($"Cannot delete book with ID {id} because it has active loans.");

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Book deleted: {BookId}", id);
    }

    public async Task<PagedResult<LoanDto>> GetBookLoansAsync(int bookId, int page, int pageSize)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new NotFoundException($"Book with ID {bookId} not found.");

        var query = _db.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(LoanService.MapToDto).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReservationDto>> GetBookReservationsAsync(int bookId, int page, int pageSize)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new NotFoundException($"Book with ID {bookId} not found.");

        var query = _db.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items.Select(ReservationService.MapToDto).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    private async Task<BookDto> ReloadBookDto(int bookId)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == bookId);
        return MapToDto(book);
    }

    internal static BookDto MapToDto(Book b) => new()
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
        Authors = b.BookAuthors.Select(ba => new AuthorSummaryDto
        {
            Id = ba.Author.Id,
            FirstName = ba.Author.FirstName,
            LastName = ba.Author.LastName
        }).ToList(),
        Categories = b.BookCategories.Select(bc => new CategorySummaryDto
        {
            Id = bc.Category.Id,
            Name = bc.Category.Name
        }).ToList()
    };
}
