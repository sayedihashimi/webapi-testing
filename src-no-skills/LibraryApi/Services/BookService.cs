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

    public async Task<PagedResult<BookDto>> GetBooksAsync(string? search, string? category, bool? available, string? sortBy, string? sortOrder, PaginationParams pagination)
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
            query = query.Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == category.ToLower()));
        }

        if (available.HasValue)
        {
            query = available.Value
                ? query.Where(b => b.AvailableCopies > 0)
                : query.Where(b => b.AvailableCopies == 0);
        }

        var totalCount = await query.CountAsync();

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

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(b => new BookDto
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
            })
            .ToListAsync();

        return new PagedResult<BookDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<BookDto> GetBookByIdAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Book with ID {id} not found");

        return MapToDto(book);
    }

    public async Task<BookDto> CreateBookAsync(CreateBookDto dto)
    {
        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN))
            throw new BusinessRuleException($"A book with ISBN '{dto.ISBN}' already exists.", 409);

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
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new NotFoundException($"Author with ID {authorId} not found");
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }

        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new NotFoundException($"Category with ID {categoryId} not found");
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });
        }

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created book: {Title} (ID: {Id})", book.Title, book.Id);

        return await GetBookByIdAsync(book.Id);
    }

    public async Task<BookDto> UpdateBookAsync(int id, UpdateBookDto dto)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Book with ID {id} not found");

        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN && b.Id != id))
            throw new BusinessRuleException($"A book with ISBN '{dto.ISBN}' already exists.", 409);

        var activeLoans = await _db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        if (dto.TotalCopies < activeLoans)
            throw new BusinessRuleException($"Cannot set total copies below the number of active loans ({activeLoans}).");

        book.Title = dto.Title;
        book.ISBN = dto.ISBN;
        book.Publisher = dto.Publisher;
        book.PublicationYear = dto.PublicationYear;
        book.Description = dto.Description;
        book.PageCount = dto.PageCount;
        book.Language = dto.Language;
        book.AvailableCopies = dto.TotalCopies - activeLoans;
        book.TotalCopies = dto.TotalCopies;
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        _db.BookAuthors.RemoveRange(book.BookAuthors);
        foreach (var authorId in dto.AuthorIds)
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new NotFoundException($"Author with ID {authorId} not found");
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update categories
        _db.BookCategories.RemoveRange(book.BookCategories);
        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new NotFoundException($"Category with ID {categoryId} not found");
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await _db.SaveChangesAsync();

        return await GetBookByIdAsync(id);
    }

    public async Task DeleteBookAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.Loans)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Book with ID {id} not found");

        if (book.Loans.Any(l => l.Status == LoanStatus.Active))
            throw new BusinessRuleException("Cannot delete a book with active loans.", 409);

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted book: {Title} (ID: {Id})", book.Title, book.Id);
    }

    public async Task<PagedResult<LoanDto>> GetBookLoansAsync(int bookId, PaginationParams pagination)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new NotFoundException($"Book with ID {bookId} not found");

        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                PatronId = l.PatronId,
                PatronName = l.Patron.FirstName + " " + l.Patron.LastName,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                Status = l.Status,
                RenewalCount = l.RenewalCount,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<ReservationDto>> GetBookReservationsAsync(int bookId, PaginationParams pagination)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new NotFoundException($"Book with ID {bookId} not found");

        var query = _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(r => new ReservationDto
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
            .ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    private static BookDto MapToDto(Book book) => new()
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
