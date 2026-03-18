using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class DataSeeder(LibraryDbContext db, ILogger<DataSeeder> logger)
{
    public async Task SeedAsync()
    {
        if (await db.Authors.AnyAsync())
        {
            logger.LogInformation("Database already seeded, skipping.");
            return;
        }

        logger.LogInformation("Seeding database...");

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "Gabriel", LastName = "García Márquez", Biography = "Colombian novelist and Nobel Prize laureate, known for magical realism.", BirthDate = new DateOnly(1927, 3, 6), Country = "Colombia", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "Isaac", LastName = "Asimov", Biography = "American author and professor of biochemistry, known for science fiction.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = DateTime.UtcNow },
            new() { Id = 3, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her commentary on the British landed gentry.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = DateTime.UtcNow },
            new() { Id = 4, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and professor, author of Sapiens.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel", CreatedAt = DateTime.UtcNow },
            new() { Id = 5, FirstName = "Marie", LastName = "Curie", Biography = "Physicist and chemist who conducted pioneering research on radioactivity.", BirthDate = new DateOnly(1867, 11, 7), Country = "Poland", CreatedAt = DateTime.UtcNow },
            new() { Id = 6, FirstName = "Toni", LastName = "Morrison", Biography = "American novelist and Nobel Prize laureate, known for African-American literature.", BirthDate = new DateOnly(1931, 2, 18), Country = "United States", CreatedAt = DateTime.UtcNow }
        };
        db.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science and technology" },
            new() { Id = 3, Name = "History", Description = "Non-fiction works about past events" },
            new() { Id = 4, Name = "Science", Description = "Works about the natural and physical world" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of a person's life written by someone else" },
            new() { Id = 6, Name = "Classic Literature", Description = "Timeless literary works of enduring significance" }
        };
        db.Categories.AddRange(categories);

        var now = DateTime.UtcNow;

        // Books (12+)
        var books = new List<Book>
        {
            new() { Id = 1, Title = "One Hundred Years of Solitude", ISBN = "978-0060883287", Publisher = "Harper Perennial", PublicationYear = 1967, Description = "The multi-generational story of the Buendía family.", PageCount = 417, Language = "English", TotalCopies = 4, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "Love in the Time of Cholera", ISBN = "978-0307389732", Publisher = "Vintage", PublicationYear = 1985, Description = "A love story spanning over fifty years.", PageCount = 368, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 5, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "I, Robot", ISBN = "978-0553382563", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of nine science fiction short stories.", PageCount = 224, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, Language = "English", TotalCopies = 6, AvailableCopies = 4, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about the Dashwood sisters.", PageCount = 409, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2011, Description = "A narrative history of the human species.", PageCount = 464, Language = "English", TotalCopies = 4, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0062464316", Publisher = "Harper", PublicationYear = 2017, Description = "Examines what might happen when mythology merges with technology.", PageCount = 464, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "Beloved", ISBN = "978-1400033416", Publisher = "Vintage", PublicationYear = 1987, Description = "A powerful story about the legacy of slavery.", PageCount = 324, Language = "English", TotalCopies = 4, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "Song of Solomon", ISBN = "978-1400033423", Publisher = "Vintage", PublicationYear = 1977, Description = "A novel about an African-American man's search for identity.", PageCount = 337, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "The Foundation Trilogy", ISBN = "978-0307292063", Publisher = "Everyman's Library", PublicationYear = 1951, Description = "The complete Foundation trilogy in one volume.", PageCount = 880, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "Radioactive: Marie & Pierre Curie", ISBN = "978-0062025104", Publisher = "HarperCollins", PublicationYear = 2010, Description = "A visual biography of the Curies and radioactivity.", PageCount = 208, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 13, Title = "Emma", ISBN = "978-0141439587", Publisher = "Penguin Classics", PublicationYear = 1815, Description = "A comic novel about youthful hubris and romantic misunderstandings.", PageCount = 474, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now }
        };
        db.Books.AddRange(books);
        await db.SaveChangesAsync();

        // BookAuthors
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },
            new() { BookId = 2, AuthorId = 1 },
            new() { BookId = 3, AuthorId = 2 },
            new() { BookId = 4, AuthorId = 2 },
            new() { BookId = 5, AuthorId = 3 },
            new() { BookId = 6, AuthorId = 3 },
            new() { BookId = 7, AuthorId = 4 },
            new() { BookId = 8, AuthorId = 4 },
            new() { BookId = 9, AuthorId = 6 },
            new() { BookId = 10, AuthorId = 6 },
            new() { BookId = 11, AuthorId = 2 },
            new() { BookId = 12, AuthorId = 5 },
            new() { BookId = 13, AuthorId = 3 },
        };
        db.BookAuthors.AddRange(bookAuthors);

        // BookCategories
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 }, new() { BookId = 1, CategoryId = 6 },
            new() { BookId = 2, CategoryId = 1 },
            new() { BookId = 3, CategoryId = 2 },
            new() { BookId = 4, CategoryId = 2 },
            new() { BookId = 5, CategoryId = 1 }, new() { BookId = 5, CategoryId = 6 },
            new() { BookId = 6, CategoryId = 1 }, new() { BookId = 6, CategoryId = 6 },
            new() { BookId = 7, CategoryId = 3 }, new() { BookId = 7, CategoryId = 4 },
            new() { BookId = 8, CategoryId = 3 }, new() { BookId = 8, CategoryId = 4 },
            new() { BookId = 9, CategoryId = 1 },
            new() { BookId = 10, CategoryId = 1 },
            new() { BookId = 11, CategoryId = 2 },
            new() { BookId = 12, CategoryId = 5 }, new() { BookId = 12, CategoryId = 4 },
            new() { BookId = 13, CategoryId = 1 }, new() { BookId = 13, CategoryId = 6 },
        };
        db.BookCategories.AddRange(bookCategories);

        // Patrons
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Elm Avenue", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Charlie", LastName = "Davis", Email = "charlie.davis@university.edu", Phone = "555-0103", Address = "789 Campus Drive", MembershipDate = new DateOnly(2023, 9, 1), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "Diana", LastName = "Wilson", Email = "diana.wilson@email.com", Phone = "555-0104", Address = "321 Pine Road", MembershipDate = new DateOnly(2022, 6, 10), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Edward", LastName = "Brown", Email = "edward.brown@email.com", Phone = "555-0105", Address = "654 Maple Lane", MembershipDate = new DateOnly(2023, 11, 5), MembershipType = MembershipType.Standard, IsActive = false, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Fiona", LastName = "Garcia", Email = "fiona.garcia@email.com", Phone = "555-0106", Address = "987 Cedar Court", MembershipDate = new DateOnly(2024, 1, 8), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, FirstName = "George", LastName = "Martinez", Email = "george.martinez@email.com", Phone = "555-0107", Address = "147 Birch Way", MembershipDate = new DateOnly(2024, 2, 14), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now }
        };
        db.Patrons.AddRange(patrons);
        await db.SaveChangesAsync();

        // Loans (mix of Active, Returned, Overdue)
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            new() { Id = 2, BookId = 3, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            new() { Id = 3, BookId = 5, PatronId = 2, LoanDate = now.AddDays(-7), DueDate = now.AddDays(7), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            new() { Id = 4, BookId = 7, PatronId = 3, LoanDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            new() { Id = 5, BookId = 11, PatronId = 4, LoanDate = now.AddDays(-12), DueDate = now.AddDays(9), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            // Returned loans
            new() { Id = 6, BookId = 9, PatronId = 2, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-18), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now },
            new() { Id = 7, BookId = 4, PatronId = 3, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-13), ReturnDate = now.AddDays(-14), Status = LoanStatus.Returned, RenewalCount = 1, CreatedAt = now },
            // Overdue loans
            new() { Id = 8, BookId = 1, PatronId = 7, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            new() { Id = 9, BookId = 3, PatronId = 6, LoanDate = now.AddDays(-12), DueDate = now.AddDays(-5), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            new() { Id = 10, BookId = 7, PatronId = 2, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-11), ReturnDate = now.AddDays(-8), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now },
        };
        db.Loans.AddRange(loans);

        // Reservations
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 1, PatronId = 4, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now },
            new() { Id = 2, BookId = 3, PatronId = 7, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now },
            new() { Id = 3, BookId = 5, PatronId = 6, ReservationDate = now.AddDays(-3), ExpirationDate = now.AddDays(1), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now },
        };
        db.Reservations.AddRange(reservations);

        // Fines
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 2, LoanId = 10, Amount = 0.75m, Reason = "Overdue by 3 day(s) at $0.25/day", IssuedDate = now.AddDays(-8), Status = FineStatus.Paid, PaidDate = now.AddDays(-7), CreatedAt = now },
            new() { Id = 2, PatronId = 7, LoanId = 8, Amount = 12.50m, Reason = "Overdue by 50 day(s) at $0.25/day — placeholder fine for testing", IssuedDate = now.AddDays(-2), Status = FineStatus.Unpaid, CreatedAt = now },
            new() { Id = 3, PatronId = 6, LoanId = 9, Amount = 1.25m, Reason = "Overdue by 5 day(s) at $0.25/day", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid, CreatedAt = now },
            new() { Id = 4, PatronId = 4, LoanId = 5, Amount = 2.00m, Reason = "Damaged book cover", IssuedDate = now.AddDays(-5), Status = FineStatus.Waived, CreatedAt = now },
        };
        db.Fines.AddRange(fines);

        await db.SaveChangesAsync();
        logger.LogInformation("Database seeded successfully.");
    }
}
