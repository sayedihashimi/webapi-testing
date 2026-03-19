using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public static class DataSeeder
{
    public static async Task SeedAsync(LibraryDbContext db)
    {
        if (await db.Authors.AnyAsync())
            return; // Already seeded

        // Authors (5+)
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, best known for '1984' and 'Animal Farm'.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her social commentary and works such as 'Pride and Prejudice'.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = DateTime.UtcNow },
            new() { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for his works of science fiction and popular science.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = DateTime.UtcNow },
            new() { Id = 4, FirstName = "Gabriel", LastName = "García Márquez", Biography = "Colombian novelist and Nobel Prize laureate, known for 'One Hundred Years of Solitude'.", BirthDate = new DateOnly(1927, 3, 6), Country = "Colombia", CreatedAt = DateTime.UtcNow },
            new() { Id = 5, FirstName = "Toni", LastName = "Morrison", Biography = "American novelist and Nobel Prize laureate, known for 'Beloved' and 'Song of Solomon'.", BirthDate = new DateOnly(1931, 2, 18), Country = "United States", CreatedAt = DateTime.UtcNow },
            new() { Id = 6, FirstName = "Stephen", LastName = "Hawking", Biography = "English theoretical physicist and cosmologist, author of 'A Brief History of Time'.", BirthDate = new DateOnly(1942, 1, 8), Country = "United Kingdom", CreatedAt = DateTime.UtcNow },
        };
        db.Authors.AddRange(authors);

        // Categories (5+)
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science and technology" },
            new() { Id = 3, Name = "History", Description = "Books about historical events and periods" },
            new() { Id = 4, Name = "Science", Description = "Books about scientific topics and discoveries" },
            new() { Id = 5, Name = "Biography", Description = "Books about the lives of real people" },
            new() { Id = 6, Name = "Romance", Description = "Fiction focused on romantic relationships" },
            new() { Id = 7, Name = "Classic Literature", Description = "Enduring works of literary fiction" },
        };
        db.Categories.AddRange(categories);

        var now = DateTime.UtcNow;

        // Books (12+) — varying copies and availability
        var books = new List<Book>
        {
            new() { Id = 1, Title = "1984", ISBN = "978-0451524935", Publisher = "Signet Classics", PublicationYear = 1949, Description = "A dystopian novel set in a totalitarian society.", PageCount = 328, Language = "English", TotalCopies = 3, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "Animal Farm", ISBN = "978-0451526342", Publisher = "Signet Classics", PublicationYear = 1945, Description = "A satirical allegorical novella about a group of farm animals.", PageCount = 112, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel that satirizes society and manners.", PageCount = 432, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about the Dashwood sisters and their romantic pursuits.", PageCount = 409, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series about the fall of a galactic empire.", PageCount = 244, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "I, Robot", ISBN = "978-0553294385", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of short stories about robots and the Three Laws of Robotics.", PageCount = 224, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "One Hundred Years of Solitude", ISBN = "978-0060883287", Publisher = "Harper Perennial", PublicationYear = 1967, Description = "A landmark novel of magical realism about the Buendía family.", PageCount = 417, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "Love in the Time of Cholera", ISBN = "978-0307389732", Publisher = "Vintage", PublicationYear = 1985, Description = "A love story spanning over fifty years.", PageCount = 348, Language = "English", TotalCopies = 1, AvailableCopies = 0, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "Beloved", ISBN = "978-1400033416", Publisher = "Vintage", PublicationYear = 1987, Description = "A powerful novel about the legacy of slavery.", PageCount = 324, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "Song of Solomon", ISBN = "978-1400033423", Publisher = "Vintage", PublicationYear = 1977, Description = "A novel following Macon 'Milkman' Dead III on a journey of self-discovery.", PageCount = 337, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "A Brief History of Time", ISBN = "978-0553380163", Publisher = "Bantam Books", PublicationYear = 1988, Description = "A landmark book on cosmology for the general reader.", PageCount = 212, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "The Universe in a Nutshell", ISBN = "978-0553802023", Publisher = "Bantam Books", PublicationYear = 2001, Description = "An exploration of the frontiers of physics.", PageCount = 216, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
        };
        db.Books.AddRange(books);

        // BookAuthors
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },   // 1984 -> Orwell
            new() { BookId = 2, AuthorId = 1 },   // Animal Farm -> Orwell
            new() { BookId = 3, AuthorId = 2 },   // Pride and Prejudice -> Austen
            new() { BookId = 4, AuthorId = 2 },   // Sense and Sensibility -> Austen
            new() { BookId = 5, AuthorId = 3 },   // Foundation -> Asimov
            new() { BookId = 6, AuthorId = 3 },   // I, Robot -> Asimov
            new() { BookId = 7, AuthorId = 4 },   // 100 Years -> García Márquez
            new() { BookId = 8, AuthorId = 4 },   // Love in the Time -> García Márquez
            new() { BookId = 9, AuthorId = 5 },   // Beloved -> Morrison
            new() { BookId = 10, AuthorId = 5 },  // Song of Solomon -> Morrison
            new() { BookId = 11, AuthorId = 6 },  // Brief History -> Hawking
            new() { BookId = 12, AuthorId = 6 },  // Universe Nutshell -> Hawking
        };
        db.BookAuthors.AddRange(bookAuthors);

        // BookCategories
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 },  // 1984 -> Fiction
            new() { BookId = 1, CategoryId = 2 },  // 1984 -> Science Fiction
            new() { BookId = 1, CategoryId = 7 },  // 1984 -> Classic Literature
            new() { BookId = 2, CategoryId = 1 },  // Animal Farm -> Fiction
            new() { BookId = 2, CategoryId = 7 },  // Animal Farm -> Classic Literature
            new() { BookId = 3, CategoryId = 1 },  // Pride and Prejudice -> Fiction
            new() { BookId = 3, CategoryId = 6 },  // Pride and Prejudice -> Romance
            new() { BookId = 3, CategoryId = 7 },  // Pride and Prejudice -> Classic Literature
            new() { BookId = 4, CategoryId = 1 },  // Sense and Sensibility -> Fiction
            new() { BookId = 4, CategoryId = 6 },  // Sense and Sensibility -> Romance
            new() { BookId = 5, CategoryId = 2 },  // Foundation -> Science Fiction
            new() { BookId = 6, CategoryId = 2 },  // I, Robot -> Science Fiction
            new() { BookId = 7, CategoryId = 1 },  // 100 Years -> Fiction
            new() { BookId = 7, CategoryId = 7 },  // 100 Years -> Classic Literature
            new() { BookId = 8, CategoryId = 1 },  // Love in the Time -> Fiction
            new() { BookId = 8, CategoryId = 6 },  // Love in the Time -> Romance
            new() { BookId = 9, CategoryId = 1 },  // Beloved -> Fiction
            new() { BookId = 9, CategoryId = 7 },  // Beloved -> Classic Literature
            new() { BookId = 10, CategoryId = 1 }, // Song of Solomon -> Fiction
            new() { BookId = 11, CategoryId = 4 }, // Brief History -> Science
            new() { BookId = 12, CategoryId = 4 }, // Universe Nutshell -> Science
        };
        db.BookCategories.AddRange(bookCategories);

        // Patrons (6+)
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Main St, Springfield", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave, Springfield", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Rd, Springfield", MembershipDate = new DateOnly(2023, 6, 10), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Elm St, Springfield", MembershipDate = new DateOnly(2023, 9, 1), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Emma", LastName = "Davis", Email = "emma.davis@email.com", Phone = "555-0105", Address = "654 Birch Ln, Springfield", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Frank", LastName = "Wilson", Email = "frank.wilson@email.com", Phone = "555-0106", Address = "987 Cedar Dr, Springfield", MembershipDate = new DateOnly(2024, 2, 14), MembershipType = MembershipType.Student, IsActive = false, CreatedAt = now, UpdatedAt = now },
        };
        db.Patrons.AddRange(patrons);

        // Loans (8+) — mix of Active, Returned, and Overdue
        // Active loans must match AvailableCopies:
        // Book 1 (1984): TotalCopies=3, AvailableCopies=1 → 2 active loans
        // Book 3 (P&P): TotalCopies=2, AvailableCopies=1 → 1 active loan
        // Book 5 (Foundation): TotalCopies=3, AvailableCopies=2 → 1 active loan
        // Book 6 (I, Robot): TotalCopies=2, AvailableCopies=1 → 1 active loan
        // Book 8 (Love in the Time): TotalCopies=1, AvailableCopies=0 → 1 active loan
        // Book 11 (Brief History): TotalCopies=3, AvailableCopies=2 → 1 active loan
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-10) },
            new() { Id = 2, BookId = 1, PatronId = 2, LoanDate = now.AddDays(-5), DueDate = now.AddDays(9), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-5) },
            new() { Id = 3, BookId = 3, PatronId = 3, LoanDate = now.AddDays(-6), DueDate = now.AddDays(1), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-6) },
            new() { Id = 4, BookId = 5, PatronId = 1, LoanDate = now.AddDays(-3), DueDate = now.AddDays(18), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-3) },
            new() { Id = 5, BookId = 8, PatronId = 4, LoanDate = now.AddDays(-12), DueDate = now.AddDays(2), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-12) },
            new() { Id = 6, BookId = 6, PatronId = 5, LoanDate = now.AddDays(-7), DueDate = now.AddDays(14), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-7) },

            // Returned loans
            new() { Id = 7, BookId = 2, PatronId = 2, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-18), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-30) },

            // Overdue loan (active but past due — for Book 11)
            new() { Id = 8, BookId = 11, PatronId = 4, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue, RenewalCount = 1, CreatedAt = now.AddDays(-20) },
        };
        db.Loans.AddRange(loans);

        // Reservations (3+)
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 8, PatronId = 1, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-2) },
            new() { Id = 2, BookId = 8, PatronId = 5, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 2, CreatedAt = now.AddDays(-1) },
            new() { Id = 3, BookId = 1, PatronId = 4, ReservationDate = now.AddDays(-3), ExpirationDate = now.AddDays(1), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now.AddDays(-3) },
        };
        db.Reservations.AddRange(reservations);

        // Fines (3+) — overdue fine on loan 8 (Overdue), and a paid fine on returned loan
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 4, LoanId = 8, Amount = 1.50m, Reason = "Overdue return - 6 day(s) late", IssuedDate = now, Status = FineStatus.Unpaid, CreatedAt = now },
            new() { Id = 2, PatronId = 2, LoanId = 7, Amount = 0.50m, Reason = "Overdue return - 2 day(s) late", IssuedDate = now.AddDays(-18), PaidDate = now.AddDays(-15), Status = FineStatus.Paid, CreatedAt = now.AddDays(-18) },
            new() { Id = 3, PatronId = 3, LoanId = 3, Amount = 0.75m, Reason = "Damaged book cover", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-1) },
        };
        db.Fines.AddRange(fines);

        await db.SaveChangesAsync();
    }
}
