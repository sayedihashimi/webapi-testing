using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<BookService> _logger;

    public BookService(LibraryDbContext context, ILogger<BookService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<BookDto>> GetAllAsync(string? search, string? category, bool? available, string? sortBy, string? sortOrder, int page, int pageSize)
    {
        var query = _context.Books
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

        if (available.HasValue)
        {
            query = available.Value
                ? query.Where(b => b.AvailableCopies > 0)
                : query.Where(b => b.AvailableCopies == 0);
        }

        // Sorting
        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("title", _) => query.OrderBy(b => b.Title),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            ("year", _) => query.OrderBy(b => b.PublicationYear),
            ("created", "desc") => query.OrderByDescending(b => b.CreatedAt),
            ("created", _) => query.OrderBy(b => b.CreatedAt),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<BookDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BookDto?> GetByIdAsync(int id)
    {
        var book = await _context.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        return book == null ? null : MapToDto(book);
    }

    public async Task<BookDto> CreateAsync(CreateBookDto dto)
    {
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
            AvailableCopies = dto.TotalCopies,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var authorId in dto.AuthorIds)
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });

        foreach (var categoryId in dto.CategoryIds)
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);

        // Reload with includes
        return (await GetByIdAsync(book.Id))!;
    }

    public async Task<BookDto?> UpdateAsync(int id, UpdateBookDto dto)
    {
        var book = await _context.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null) return null;

        // Track the difference in TotalCopies to adjust AvailableCopies
        var copyDifference = dto.TotalCopies - book.TotalCopies;

        book.Title = dto.Title;
        book.ISBN = dto.ISBN;
        book.Publisher = dto.Publisher;
        book.PublicationYear = dto.PublicationYear;
        book.Description = dto.Description;
        book.PageCount = dto.PageCount;
        book.Language = dto.Language;
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = Math.Max(0, book.AvailableCopies + copyDifference);
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        book.BookAuthors.Clear();
        foreach (var authorId in dto.AuthorIds)
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });

        // Update categories
        book.BookCategories.Clear();
        foreach (var categoryId in dto.CategoryIds)
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated book {BookId}", id);
        return (await GetByIdAsync(id))!;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var book = await _context.Books
            .Include(b => b.Loans)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null) return (false, "Book not found");

        if (book.Loans.Any(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            return (false, "Cannot delete book with active loans.");

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted book {BookId}", id);
        return (true, null);
    }

    public async Task<PaginatedResponse<LoanDto>> GetBookLoansAsync(int bookId, int page, int pageSize)
    {
        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<LoanDto>
        {
            Items = items.Select(LoanService.MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResponse<ReservationDto>> GetBookReservationsAsync(int bookId, int page, int pageSize)
    {
        var query = _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<ReservationDto>
        {
            Items = items.Select(ReservationService.MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static BookDto MapToDto(Book b) => new()
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
        Authors = b.BookAuthors.Select(ba => new AuthorDto
        {
            Id = ba.Author.Id,
            FirstName = ba.Author.FirstName,
            LastName = ba.Author.LastName,
            Biography = ba.Author.Biography,
            BirthDate = ba.Author.BirthDate,
            Country = ba.Author.Country,
            CreatedAt = ba.Author.CreatedAt
        }).ToList(),
        Categories = b.BookCategories.Select(bc => new CategoryDto
        {
            Id = bc.Category.Id,
            Name = bc.Category.Name,
            Description = bc.Category.Description
        }).ToList()
    };
}
