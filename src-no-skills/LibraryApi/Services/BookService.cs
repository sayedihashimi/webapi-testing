using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService
{
    private readonly LibraryDbContext _db;

    public BookService(LibraryDbContext db) => _db = db;

    public async Task<PaginatedResponse<BookSummaryDto>> GetAllAsync(
        string? search, string? category, bool? available, string? sortBy, string? sortDir,
        int page = 1, int pageSize = 10)
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

        query = (sortBy?.ToLower()) switch
        {
            "year" => sortDir == "desc" ? query.OrderByDescending(b => b.PublicationYear) : query.OrderBy(b => b.PublicationYear),
            "copies" => sortDir == "desc" ? query.OrderByDescending(b => b.TotalCopies) : query.OrderBy(b => b.TotalCopies),
            _ => sortDir == "desc" ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync();
        var books = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = books.Select(b => new BookSummaryDto
        {
            Id = b.Id, Title = b.Title, ISBN = b.ISBN, PublicationYear = b.PublicationYear,
            TotalCopies = b.TotalCopies, AvailableCopies = b.AvailableCopies,
            Authors = b.BookAuthors.Select(ba => $"{ba.Author.FirstName} {ba.Author.LastName}").ToList(),
            Categories = b.BookCategories.Select(bc => bc.Category.Name).ToList()
        }).ToList();

        return new PaginatedResponse<BookSummaryDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<BookDetailDto> GetByIdAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Book with ID {id} not found.");

        return MapToDetailDto(book);
    }

    public async Task<BookDetailDto> CreateAsync(CreateBookDto dto)
    {
        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN))
            throw new BusinessRuleException("A book with this ISBN already exists.", 409);

        var book = new Book
        {
            Title = dto.Title, ISBN = dto.ISBN, Publisher = dto.Publisher,
            PublicationYear = dto.PublicationYear, Description = dto.Description,
            PageCount = dto.PageCount, Language = dto.Language ?? "English",
            TotalCopies = dto.TotalCopies, AvailableCopies = dto.TotalCopies
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

        return await GetByIdAsync(book.Id);
    }

    public async Task<BookDetailDto> UpdateAsync(int id, UpdateBookDto dto)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors).Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Book with ID {id} not found.");

        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN && b.Id != id))
            throw new BusinessRuleException("A book with this ISBN already exists.", 409);

        var activeLoans = await _db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        var newAvailable = dto.TotalCopies - activeLoans;
        if (newAvailable < 0)
            throw new BusinessRuleException($"Cannot set TotalCopies to {dto.TotalCopies}: there are {activeLoans} active loans.");

        book.Title = dto.Title; book.ISBN = dto.ISBN; book.Publisher = dto.Publisher;
        book.PublicationYear = dto.PublicationYear; book.Description = dto.Description;
        book.PageCount = dto.PageCount; book.Language = dto.Language ?? "English";
        book.TotalCopies = dto.TotalCopies; book.AvailableCopies = newAvailable;
        book.UpdatedAt = DateTime.UtcNow;

        book.BookAuthors.Clear();
        foreach (var authorId in dto.AuthorIds)
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new NotFoundException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        book.BookCategories.Clear();
        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new NotFoundException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var book = await _db.Books.FindAsync(id)
            ?? throw new NotFoundException($"Book with ID {id} not found.");

        var hasActiveLoans = await _db.Loans.AnyAsync(l => l.BookId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        if (hasActiveLoans)
            throw new BusinessRuleException("Cannot delete book with active or overdue loans.", 409);

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
    }

    public async Task<PaginatedResponse<LoanDto>> GetBookLoansAsync(int bookId, int page = 1, int pageSize = 10)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new NotFoundException($"Book with ID {bookId} not found.");

        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.BookId == bookId).OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => MapLoanDto(l)).ToListAsync();

        return new PaginatedResponse<LoanDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<PaginatedResponse<ReservationDto>> GetBookReservationsAsync(int bookId, int page = 1, int pageSize = 10)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new NotFoundException($"Book with ID {bookId} not found.");

        var query = _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.BookId == bookId).OrderBy(r => r.QueuePosition);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => MapReservationDto(r)).ToListAsync();

        return new PaginatedResponse<ReservationDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    private static BookDetailDto MapToDetailDto(Book b) => new()
    {
        Id = b.Id, Title = b.Title, ISBN = b.ISBN, Publisher = b.Publisher,
        PublicationYear = b.PublicationYear, Description = b.Description,
        PageCount = b.PageCount, Language = b.Language,
        TotalCopies = b.TotalCopies, AvailableCopies = b.AvailableCopies,
        CreatedAt = b.CreatedAt, UpdatedAt = b.UpdatedAt,
        Authors = b.BookAuthors.Select(ba => new AuthorDto
        {
            Id = ba.Author.Id, FirstName = ba.Author.FirstName, LastName = ba.Author.LastName,
            Biography = ba.Author.Biography, BirthDate = ba.Author.BirthDate,
            Country = ba.Author.Country, CreatedAt = ba.Author.CreatedAt
        }).ToList(),
        Categories = b.BookCategories.Select(bc => new CategoryDto
        {
            Id = bc.Category.Id, Name = bc.Category.Name, Description = bc.Category.Description
        }).ToList()
    };

    internal static LoanDto MapLoanDto(Loan l) => new()
    {
        Id = l.Id, BookId = l.BookId, BookTitle = l.Book.Title,
        PatronId = l.PatronId, PatronName = $"{l.Patron.FirstName} {l.Patron.LastName}",
        LoanDate = l.LoanDate, DueDate = l.DueDate, ReturnDate = l.ReturnDate,
        Status = l.Status.ToString(), RenewalCount = l.RenewalCount, CreatedAt = l.CreatedAt
    };

    internal static ReservationDto MapReservationDto(Reservation r) => new()
    {
        Id = r.Id, BookId = r.BookId, BookTitle = r.Book.Title,
        PatronId = r.PatronId, PatronName = $"{r.Patron.FirstName} {r.Patron.LastName}",
        ReservationDate = r.ReservationDate, ExpirationDate = r.ExpirationDate,
        Status = r.Status.ToString(), QueuePosition = r.QueuePosition, CreatedAt = r.CreatedAt
    };
}
