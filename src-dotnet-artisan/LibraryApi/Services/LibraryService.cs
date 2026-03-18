using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class LibraryService(LibraryDbContext db, ILogger<LibraryService> logger)
{
    // ==================== Business rule constants ====================
    private static readonly Dictionary<MembershipType, (int MaxLoans, int LoanDays)> _membershipRules = new()
    {
        [MembershipType.Standard] = (5, 14),
        [MembershipType.Premium] = (10, 21),
        [MembershipType.Student] = (3, 7)
    };

    private const decimal FinePerDay = 0.25m;
    private const decimal FineThreshold = 10.00m;
    private const int MaxRenewals = 2;
    private const int ReservationPickupDays = 3;

    // ==================== Author operations ====================
    public async Task<PaginatedResponse<AuthorResponse>> GetAuthorsAsync(string? search, int page, int pageSize)
    {
        var query = db.Authors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(a => a.FirstName.ToLower().Contains(term) || a.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AuthorResponse(a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<AuthorResponse>(items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<AuthorDetailResponse?> GetAuthorByIdAsync(int id)
    {
        return await db.Authors
            .Where(a => a.Id == id)
            .Select(a => new AuthorDetailResponse(
                a.Id, a.FirstName, a.LastName, a.Biography, a.BirthDate, a.Country, a.CreatedAt,
                a.BookAuthors.Select(ba => new BookSummaryResponse(
                    ba.Book.Id, ba.Book.Title, ba.Book.ISBN, ba.Book.Publisher,
                    ba.Book.PublicationYear, ba.Book.Language, ba.Book.TotalCopies, ba.Book.AvailableCopies
                )).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request)
    {
        var author = new Author
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Biography = request.Biography,
            BirthDate = request.BirthDate,
            Country = request.Country
        };
        db.Authors.Add(author);
        await db.SaveChangesAsync();
        logger.LogInformation("Created author {AuthorId}: {FirstName} {LastName}", author.Id, author.FirstName, author.LastName);
        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request)
    {
        var author = await db.Authors.FindAsync(id);
        if (author is null) { return null; }

        author.FirstName = request.FirstName;
        author.LastName = request.LastName;
        author.Biography = request.Biography;
        author.BirthDate = request.BirthDate;
        author.Country = request.Country;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated author {AuthorId}", id);
        return new AuthorResponse(author.Id, author.FirstName, author.LastName, author.Biography, author.BirthDate, author.Country, author.CreatedAt);
    }

    public async Task<(bool Success, string? Error)> DeleteAuthorAsync(int id)
    {
        var author = await db.Authors.Include(a => a.BookAuthors).FirstOrDefaultAsync(a => a.Id == id);
        if (author is null) { return (false, "Author not found"); }
        if (author.BookAuthors.Count > 0) { return (false, "Cannot delete author with associated books"); }

        db.Authors.Remove(author);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted author {AuthorId}", id);
        return (true, null);
    }

    // ==================== Category operations ====================
    public async Task<List<CategoryResponse>> GetCategoriesAsync()
    {
        return await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync();
    }

    public async Task<CategoryDetailResponse?> GetCategoryByIdAsync(int id)
    {
        return await db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailResponse(c.Id, c.Name, c.Description, c.BookCategories.Count))
            .FirstOrDefaultAsync();
    }

    public async Task<(CategoryResponse? Result, string? Error)> CreateCategoryAsync(CreateCategoryRequest request)
    {
        if (await db.Categories.AnyAsync(c => c.Name == request.Name))
        {
            return (null, "A category with this name already exists");
        }

        var category = new Category { Name = request.Name, Description = request.Description };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        logger.LogInformation("Created category {CategoryId}: {Name}", category.Id, category.Name);
        return (new CategoryResponse(category.Id, category.Name, category.Description), null);
    }

    public async Task<(CategoryResponse? Result, string? Error)> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) { return (null, "Category not found"); }

        if (await db.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id))
        {
            return (null, "A category with this name already exists");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated category {CategoryId}", id);
        return (new CategoryResponse(category.Id, category.Name, category.Description), null);
    }

    public async Task<(bool Success, string? Error)> DeleteCategoryAsync(int id)
    {
        var category = await db.Categories.Include(c => c.BookCategories).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) { return (false, "Category not found"); }
        if (category.BookCategories.Count > 0) { return (false, "Cannot delete category with associated books"); }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted category {CategoryId}", id);
        return (true, null);
    }

    // ==================== Book operations ====================
    public async Task<PaginatedResponse<BookSummaryResponse>> GetBooksAsync(
        string? search, string? category, bool? available, string? sortBy, string? sortOrder, int page, int pageSize)
    {
        var query = db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.Contains(term) ||
                b.BookAuthors.Any(ba => ba.Author.FirstName.ToLower().Contains(term) || ba.Author.LastName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == category.ToLower()));
        }

        if (available == true)
        {
            query = query.Where(b => b.AvailableCopies > 0);
        }
        else if (available == false)
        {
            query = query.Where(b => b.AvailableCopies == 0);
        }

        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("year", "asc") => query.OrderBy(b => b.PublicationYear),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => new BookSummaryResponse(b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear, b.Language, b.TotalCopies, b.AvailableCopies))
            .ToListAsync();

        return new PaginatedResponse<BookSummaryResponse>(items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<BookDetailResponse?> GetBookByIdAsync(int id)
    {
        return await db.Books
            .Where(b => b.Id == id)
            .Select(b => new BookDetailResponse(
                b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear, b.Description, b.PageCount,
                b.Language, b.TotalCopies, b.AvailableCopies, b.CreatedAt, b.UpdatedAt,
                b.BookAuthors.Select(ba => new AuthorResponse(
                    ba.Author.Id, ba.Author.FirstName, ba.Author.LastName, ba.Author.Biography, ba.Author.BirthDate, ba.Author.Country, ba.Author.CreatedAt)).ToList(),
                b.BookCategories.Select(bc => new CategoryResponse(bc.Category.Id, bc.Category.Name, bc.Category.Description)).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<(BookDetailResponse? Result, string? Error)> CreateBookAsync(CreateBookRequest request)
    {
        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN))
        {
            return (null, "A book with this ISBN already exists");
        }

        var authors = await db.Authors.Where(a => request.AuthorIds.Contains(a.Id)).ToListAsync();
        if (authors.Count != request.AuthorIds.Count)
        {
            return (null, "One or more author IDs are invalid");
        }

        var categories = await db.Categories.Where(c => request.CategoryIds.Contains(c.Id)).ToListAsync();
        if (categories.Count != request.CategoryIds.Count)
        {
            return (null, "One or more category IDs are invalid");
        }

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
            AvailableCopies = request.TotalCopies
        };

        db.Books.Add(book);
        await db.SaveChangesAsync();

        foreach (int authorId in request.AuthorIds)
        {
            db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }
        foreach (int categoryId in request.CategoryIds)
        {
            db.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
        }
        await db.SaveChangesAsync();

        logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);
        return (await GetBookByIdAsync(book.Id), null);
    }

    public async Task<(BookDetailResponse? Result, string? Error)> UpdateBookAsync(int id, UpdateBookRequest request)
    {
        var book = await db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) { return (null, "Book not found"); }

        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN && b.Id != id))
        {
            return (null, "A book with this ISBN already exists");
        }

        var activeLoanCount = await db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        var copyDiff = request.TotalCopies - book.TotalCopies;

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = Math.Max(0, book.AvailableCopies + copyDiff);
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        db.BookAuthors.RemoveRange(book.BookAuthors);
        foreach (int authorId in request.AuthorIds)
        {
            db.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update categories
        db.BookCategories.RemoveRange(book.BookCategories);
        foreach (int categoryId in request.CategoryIds)
        {
            db.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Updated book {BookId}", id);
        return (await GetBookByIdAsync(id), null);
    }

    public async Task<(bool Success, string? Error)> DeleteBookAsync(int id)
    {
        var book = await db.Books.FindAsync(id);
        if (book is null) { return (false, "Book not found"); }

        if (await db.Loans.AnyAsync(l => l.BookId == id && l.Status == LoanStatus.Active))
        {
            return (false, "Cannot delete book with active loans");
        }

        db.Books.Remove(book);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted book {BookId}", id);
        return (true, null);
    }

    public async Task<List<LoanResponse>> GetBookLoansAsync(int bookId)
    {
        return await db.Loans
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate)
            .Select(_loanProjection)
            .ToListAsync();
    }

    public async Task<List<ReservationResponse>> GetBookReservationsAsync(int bookId)
    {
        return await db.Reservations
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .Select(_reservationProjection)
            .ToListAsync();
    }

    // ==================== Patron operations ====================
    public async Task<PaginatedResponse<PatronResponse>> GetPatronsAsync(string? search, MembershipType? membershipType, int page, int pageSize)
    {
        var query = db.Patrons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        if (membershipType.HasValue)
        {
            query = query.Where(p => p.MembershipType == membershipType.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PatronResponse(p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.MembershipType, p.MembershipDate, p.IsActive))
            .ToListAsync();

        return new PaginatedResponse<PatronResponse>(items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PatronDetailResponse?> GetPatronByIdAsync(int id)
    {
        return await db.Patrons
            .Where(p => p.Id == id)
            .Select(p => new PatronDetailResponse(
                p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipType, p.MembershipDate, p.IsActive, p.CreatedAt, p.UpdatedAt,
                p.Loans.Count(l => l.Status == LoanStatus.Active),
                p.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount)))
            .FirstOrDefaultAsync();
    }

    public async Task<(PatronResponse? Result, string? Error)> CreatePatronAsync(CreatePatronRequest request)
    {
        if (await db.Patrons.AnyAsync(p => p.Email == request.Email))
        {
            return (null, "A patron with this email already exists");
        }

        var patron = new Patron
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            MembershipType = request.MembershipType
        };
        db.Patrons.Add(patron);
        await db.SaveChangesAsync();
        logger.LogInformation("Created patron {PatronId}: {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);
        return (new PatronResponse(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.Phone, patron.MembershipType, patron.MembershipDate, patron.IsActive), null);
    }

    public async Task<(PatronResponse? Result, string? Error)> UpdatePatronAsync(int id, UpdatePatronRequest request)
    {
        var patron = await db.Patrons.FindAsync(id);
        if (patron is null) { return (null, "Patron not found"); }

        if (await db.Patrons.AnyAsync(p => p.Email == request.Email && p.Id != id))
        {
            return (null, "A patron with this email already exists");
        }

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = request.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated patron {PatronId}", id);
        return (new PatronResponse(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.Phone, patron.MembershipType, patron.MembershipDate, patron.IsActive), null);
    }

    public async Task<(bool Success, string? Error)> DeletePatronAsync(int id)
    {
        var patron = await db.Patrons.FindAsync(id);
        if (patron is null) { return (false, "Patron not found"); }

        if (await db.Loans.AnyAsync(l => l.PatronId == id && l.Status == LoanStatus.Active))
        {
            return (false, "Cannot deactivate patron with active loans");
        }

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Deactivated patron {PatronId}", id);
        return (true, null);
    }

    public async Task<List<LoanResponse>> GetPatronLoansAsync(int patronId, LoanStatus? status)
    {
        var query = db.Loans.Where(l => l.PatronId == patronId);
        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }
        return await query.OrderByDescending(l => l.LoanDate)
            .Select(_loanProjection)
            .ToListAsync();
    }

    public async Task<List<ReservationResponse>> GetPatronReservationsAsync(int patronId)
    {
        return await db.Reservations
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate)
            .Select(_reservationProjection)
            .ToListAsync();
    }

    public async Task<List<FineResponse>> GetPatronFinesAsync(int patronId, FineStatus? status)
    {
        var query = db.Fines.Where(f => f.PatronId == patronId);
        if (status.HasValue)
        {
            query = query.Where(f => f.Status == status.Value);
        }
        return await query.OrderByDescending(f => f.IssuedDate)
            .Select(_fineProjection)
            .ToListAsync();
    }

    // ==================== Loan operations ====================
    public async Task<PaginatedResponse<LoanResponse>> GetLoansAsync(LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = db.Loans.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        if (overdue == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(l => l.ReturnDate == null && l.DueDate < now);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.LoanDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.LoanDate <= toDate.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(_loanProjection)
            .ToListAsync();

        return new PaginatedResponse<LoanResponse>(items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<LoanResponse?> GetLoanByIdAsync(int id)
    {
        return await db.Loans
            .Where(l => l.Id == id)
            .Select(_loanProjection)
            .FirstOrDefaultAsync();
    }

    public async Task<List<LoanResponse>> GetOverdueLoansAsync()
    {
        var now = DateTime.UtcNow;
        return await db.Loans
            .Where(l => l.ReturnDate == null && l.DueDate < now)
            .OrderBy(l => l.DueDate)
            .Select(_loanProjection)
            .ToListAsync();
    }

    public async Task<(LoanResponse? Result, string? Error)> CheckoutBookAsync(CreateLoanRequest request)
    {
        var book = await db.Books.FindAsync(request.BookId);
        if (book is null) { return (null, "Book not found"); }

        var patron = await db.Patrons.FindAsync(request.PatronId);
        if (patron is null) { return (null, "Patron not found"); }

        if (!patron.IsActive) { return (null, "Patron account is inactive"); }

        // Check unpaid fines
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);
        if (unpaidFines >= FineThreshold)
        {
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines (threshold: ${FineThreshold:F2})");
        }

        if (book.AvailableCopies <= 0) { return (null, "No copies available"); }

        // Check borrowing limit
        var rules = _membershipRules[patron.MembershipType];
        var activeLoans = await db.Loans.CountAsync(l => l.PatronId == patron.Id && l.Status == LoanStatus.Active);
        if (activeLoans >= rules.MaxLoans)
        {
            return (null, $"Patron has reached borrowing limit ({rules.MaxLoans} for {patron.MembershipType})");
        }

        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            DueDate = DateTime.UtcNow.AddDays(rules.LoanDays)
        };

        book.AvailableCopies--;
        db.Loans.Add(loan);
        await db.SaveChangesAsync();

        logger.LogInformation("Checkout: Book {BookId} to Patron {PatronId}, Loan {LoanId}, Due {DueDate}",
            book.Id, patron.Id, loan.Id, loan.DueDate);

        return (await GetLoanByIdAsync(loan.Id), null);
    }

    public async Task<(LoanResponse? Result, string? Error)> ReturnBookAsync(int loanId)
    {
        var loan = await db.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == loanId);
        if (loan is null) { return (null, "Loan not found"); }
        if (loan.Status == LoanStatus.Returned) { return (null, "Book already returned"); }

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;

        // Generate fine if overdue
        if (loan.DueDate < now)
        {
            var daysOverdue = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = daysOverdue * FinePerDay;
            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue: {daysOverdue} days late"
            };
            db.Fines.Add(fine);
            logger.LogInformation("Generated fine of ${Amount} for Loan {LoanId}", fineAmount, loanId);
        }

        // Check pending reservations for this book
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(ReservationPickupDays);
            logger.LogInformation("Reservation {ReservationId} for Book {BookId} is now Ready", nextReservation.Id, loan.BookId);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Return: Loan {LoanId}, Book {BookId}", loanId, loan.BookId);
        return (await GetLoanByIdAsync(loanId), null);
    }

    public async Task<(LoanResponse? Result, string? Error)> RenewLoanAsync(int loanId)
    {
        var loan = await db.Loans.Include(l => l.Patron).FirstOrDefaultAsync(l => l.Id == loanId);
        if (loan is null) { return (null, "Loan not found"); }
        if (loan.Status != LoanStatus.Active) { return (null, "Only active loans can be renewed"); }
        if (loan.RenewalCount >= MaxRenewals) { return (null, $"Maximum renewals ({MaxRenewals}) reached"); }

        // Check overdue
        if (loan.DueDate < DateTime.UtcNow) { return (null, "Cannot renew overdue loan"); }

        // Check unpaid fines
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);
        if (unpaidFines >= FineThreshold)
        {
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines (threshold: ${FineThreshold:F2})");
        }

        // Check pending reservations
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));
        if (hasPendingReservations)
        {
            return (null, "Cannot renew: there are pending reservations for this book");
        }

        var rules = _membershipRules[loan.Patron.MembershipType];
        loan.DueDate = DateTime.UtcNow.AddDays(rules.LoanDays);
        loan.RenewalCount++;
        await db.SaveChangesAsync();

        logger.LogInformation("Renewed Loan {LoanId}, new DueDate {DueDate}, renewal #{Count}", loanId, loan.DueDate, loan.RenewalCount);
        return (await GetLoanByIdAsync(loanId), null);
    }

    // ==================== Reservation operations ====================
    public async Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(ReservationStatus? status, int page, int pageSize)
    {
        var query = db.Reservations.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(_reservationProjection)
            .ToListAsync();

        return new PaginatedResponse<ReservationResponse>(items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<ReservationResponse?> GetReservationByIdAsync(int id)
    {
        return await db.Reservations
            .Where(r => r.Id == id)
            .Select(_reservationProjection)
            .FirstOrDefaultAsync();
    }

    public async Task<(ReservationResponse? Result, string? Error)> CreateReservationAsync(CreateReservationRequest request)
    {
        var book = await db.Books.FindAsync(request.BookId);
        if (book is null) { return (null, "Book not found"); }

        var patron = await db.Patrons.FindAsync(request.PatronId);
        if (patron is null) { return (null, "Patron not found"); }

        if (!patron.IsActive) { return (null, "Patron account is inactive"); }

        // Cannot reserve if patron already has active loan for this book
        var hasActiveLoan = await db.Loans.AnyAsync(l => l.BookId == request.BookId && l.PatronId == request.PatronId && l.Status == LoanStatus.Active);
        if (hasActiveLoan) { return (null, "Patron already has an active loan for this book"); }

        // Cannot reserve if already has pending/ready reservation
        var hasReservation = await db.Reservations.AnyAsync(r =>
            r.BookId == request.BookId && r.PatronId == request.PatronId &&
            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));
        if (hasReservation) { return (null, "Patron already has a reservation for this book"); }

        // Determine queue position
        var maxPosition = await db.Reservations
            .Where(r => r.BookId == request.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var reservation = new Reservation
        {
            BookId = request.BookId,
            PatronId = request.PatronId,
            QueuePosition = maxPosition + 1
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        logger.LogInformation("Created reservation {ReservationId} for Book {BookId}, Patron {PatronId}, Position {Position}",
            reservation.Id, book.Id, patron.Id, reservation.QueuePosition);

        return (await GetReservationByIdAsync(reservation.Id), null);
    }

    public async Task<(ReservationResponse? Result, string? Error)> CancelReservationAsync(int id)
    {
        var reservation = await db.Reservations.FindAsync(id);
        if (reservation is null) { return (null, "Reservation not found"); }

        if (reservation.Status is not (ReservationStatus.Pending or ReservationStatus.Ready))
        {
            return (null, "Only pending or ready reservations can be cancelled");
        }

        reservation.Status = ReservationStatus.Cancelled;

        // Re-sequence queue positions
        var laterReservations = await db.Reservations
            .Where(r => r.BookId == reservation.BookId && r.QueuePosition > reservation.QueuePosition &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync();

        foreach (var r in laterReservations)
        {
            r.QueuePosition--;
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Cancelled reservation {ReservationId}", id);
        return (await GetReservationByIdAsync(id), null);
    }

    public async Task<(LoanResponse? Result, string? Error)> FulfillReservationAsync(int id)
    {
        var reservation = await db.Reservations.Include(r => r.Book).Include(r => r.Patron).FirstOrDefaultAsync(r => r.Id == id);
        if (reservation is null) { return (null, "Reservation not found"); }
        if (reservation.Status != ReservationStatus.Ready) { return (null, "Only ready reservations can be fulfilled"); }

        if (reservation.Book.AvailableCopies <= 0) { return (null, "No copies available"); }

        // Check patron fine threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == reservation.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);
        if (unpaidFines >= FineThreshold)
        {
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines");
        }

        var rules = _membershipRules[reservation.Patron.MembershipType];
        var loan = new Loan
        {
            BookId = reservation.BookId,
            PatronId = reservation.PatronId,
            DueDate = DateTime.UtcNow.AddDays(rules.LoanDays)
        };

        reservation.Status = ReservationStatus.Fulfilled;
        reservation.Book.AvailableCopies--;
        db.Loans.Add(loan);

        // Re-sequence remaining
        var laterReservations = await db.Reservations
            .Where(r => r.BookId == reservation.BookId && r.QueuePosition > reservation.QueuePosition &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync();

        foreach (var r in laterReservations)
        {
            r.QueuePosition--;
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Fulfilled reservation {ReservationId}, created Loan {LoanId}", id, loan.Id);
        return (await GetLoanByIdAsync(loan.Id), null);
    }

    // ==================== Fine operations ====================
    public async Task<PaginatedResponse<FineResponse>> GetFinesAsync(FineStatus? status, int page, int pageSize)
    {
        var query = db.Fines.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(f => f.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(_fineProjection)
            .ToListAsync();

        return new PaginatedResponse<FineResponse>(items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<FineResponse?> GetFineByIdAsync(int id)
    {
        return await db.Fines
            .Where(f => f.Id == id)
            .Select(_fineProjection)
            .FirstOrDefaultAsync();
    }

    public async Task<(FineResponse? Result, string? Error)> PayFineAsync(int id)
    {
        var fine = await db.Fines.FindAsync(id);
        if (fine is null) { return (null, "Fine not found"); }
        if (fine.Status != FineStatus.Unpaid) { return (null, "Fine is not unpaid"); }

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Paid fine {FineId}, Amount ${Amount}", id, fine.Amount);
        return (await GetFineByIdAsync(id), null);
    }

    public async Task<(FineResponse? Result, string? Error)> WaiveFineAsync(int id)
    {
        var fine = await db.Fines.FindAsync(id);
        if (fine is null) { return (null, "Fine not found"); }
        if (fine.Status != FineStatus.Unpaid) { return (null, "Fine is not unpaid"); }

        fine.Status = FineStatus.Waived;
        await db.SaveChangesAsync();
        logger.LogInformation("Waived fine {FineId}, Amount ${Amount}", id, fine.Amount);
        return (await GetFineByIdAsync(id), null);
    }

    // ==================== Projection expressions (EF Core translatable) ====================
    private static readonly System.Linq.Expressions.Expression<Func<Loan, LoanResponse>> _loanProjection =
        l => new LoanResponse(
            l.Id, l.BookId, l.Book.Title, l.PatronId,
            l.Patron.FirstName + " " + l.Patron.LastName,
            l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt);

    private static readonly System.Linq.Expressions.Expression<Func<Reservation, ReservationResponse>> _reservationProjection =
        r => new ReservationResponse(
            r.Id, r.BookId, r.Book.Title, r.PatronId,
            r.Patron.FirstName + " " + r.Patron.LastName,
            r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt);

    private static readonly System.Linq.Expressions.Expression<Func<Fine, FineResponse>> _fineProjection =
        f => new FineResponse(
            f.Id, f.PatronId, f.Patron.FirstName + " " + f.Patron.LastName,
            f.LoanId, f.Loan.Book.Title, f.Amount, f.Reason,
            f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt);
}
