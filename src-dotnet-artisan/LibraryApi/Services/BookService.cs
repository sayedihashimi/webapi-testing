using LibraryApi.Data;
using LibraryApi.DTOs;
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

    public async Task<PagedResult<BookDto>> GetAllAsync(string? search, bool? available, string? sortBy, string? sortDir, int page, int pageSize)
    {
        var query = _db.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(s) || b.ISBN.ToLower().Contains(s));
        }

        if (available == true)
            query = query.Where(b => b.AvailableCopies > 0);
        else if (available == false)
            query = query.Where(b => b.AvailableCopies == 0);

        query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("title", _) => query.OrderBy(b => b.Title),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            ("year", _) => query.OrderBy(b => b.PublicationYear),
            ("created", "desc") => query.OrderByDescending(b => b.CreatedAt),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapToDto(b))
            .ToListAsync();

        return new PagedResult<BookDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<BookDetailDto?> GetByIdAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) return null;

        return new BookDetailDto
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            Publisher = book.Publisher,
            PublicationYear = book.PublicationYear,
            Description = book.Description,
            PageCount = book.PageCount,
            Language = book.Language,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt,
            Authors = book.BookAuthors.Select(ba => new AuthorDto
            {
                Id = ba.Author.Id,
                FirstName = ba.Author.FirstName,
                LastName = ba.Author.LastName,
                Biography = ba.Author.Biography,
                BirthDate = ba.Author.BirthDate,
                Country = ba.Author.Country,
                CreatedAt = ba.Author.CreatedAt
            }).ToList(),
            Categories = book.BookCategories.Select(bc => new CategoryDto
            {
                Id = bc.Category.Id,
                Name = bc.Category.Name,
                Description = bc.Category.Description
            }).ToList()
        };
    }

    public async Task<BookDto> CreateAsync(BookCreateDto dto)
    {
        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN))
            throw new InvalidOperationException($"A book with ISBN '{dto.ISBN}' already exists");

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

        foreach (var authorId in dto.AuthorIds)
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new KeyNotFoundException($"Author with id {authorId} not found");
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }

        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new KeyNotFoundException($"Category with id {categoryId} not found");
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });
        }

        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created book {Id}: {Title}", book.Id, book.Title);

        return MapToDto(book);
    }

    public async Task<BookDto?> UpdateAsync(int id, BookUpdateDto dto)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) return null;

        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN && b.Id != id))
            throw new InvalidOperationException($"A book with ISBN '{dto.ISBN}' already exists");

        book.Title = dto.Title;
        book.ISBN = dto.ISBN;
        book.Publisher = dto.Publisher;
        book.PublicationYear = dto.PublicationYear;
        book.Description = dto.Description;
        book.PageCount = dto.PageCount;
        book.Language = dto.Language ?? "English";

        var activeLoans = await _db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        if (dto.TotalCopies < activeLoans)
            throw new InvalidOperationException($"Cannot set TotalCopies below {activeLoans} (active loans count)");

        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = dto.TotalCopies - activeLoans;
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        book.BookAuthors.Clear();
        foreach (var authorId in dto.AuthorIds)
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new KeyNotFoundException($"Author with id {authorId} not found");
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update categories
        book.BookCategories.Clear();
        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new KeyNotFoundException($"Category with id {categoryId} not found");
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated book {Id}", id);

        return MapToDto(book);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.Loans)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null)
            throw new KeyNotFoundException($"Book with id {id} not found");

        if (book.Loans.Any(l => l.Status == LoanStatus.Active))
            throw new InvalidOperationException($"Cannot delete book with id {id} because it has active loans");

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted book {Id}", id);

        return true;
    }

    public async Task<PagedResult<LoanDto>> GetLoansAsync(int bookId, int page, int pageSize)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new KeyNotFoundException($"Book with id {bookId} not found");

        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => LoanService.MapToDto(l))
            .ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<ReservationDto>> GetReservationsAsync(int bookId)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new KeyNotFoundException($"Book with id {bookId} not found");

        return await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .Select(r => ReservationService.MapToDto(r))
            .ToListAsync();
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
        UpdatedAt = b.UpdatedAt
    };
}
