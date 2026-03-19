using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(LibraryDbContext context)
    {
        if (await context.Authors.AnyAsync())
        {
            return;
        }

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "Harper", LastName = "Lee", Biography = "American novelist known for To Kill a Mockingbird.", BirthDate = new DateOnly(1926, 4, 28), Country = "United States" },
            new() { Id = 2, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist known for dystopian works.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom" },
            new() { Id = 3, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for social commentary and realism.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom" },
            new() { Id = 4, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, prolific science fiction author.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States" },
            new() { Id = 5, FirstName = "Doris", LastName = "Kearns Goodwin", Biography = "American biographer and historian.", BirthDate = new DateOnly(1943, 1, 4), Country = "United States" },
            new() { Id = 6, FirstName = "Carl", LastName = "Sagan", Biography = "American astronomer, cosmologist, and science communicator.", BirthDate = new DateOnly(1934, 11, 9), Country = "United States" },
        };
        context.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science and technology" },
            new() { Id = 3, Name = "History", Description = "Non-fiction accounts of past events" },
            new() { Id = 4, Name = "Science", Description = "Non-fiction works about scientific topics" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of a person's life written by someone else" },
            new() { Id = 6, Name = "Classic Literature", Description = "Enduring works of literary merit" },
        };
        context.Categories.AddRange(categories);

        // Books (12+)
        var now = DateTime.UtcNow;
        var books = new List<Book>
        {
            new() { Id = 1, Title = "To Kill a Mockingbird", ISBN = "978-0061120084", Publisher = "Harper Perennial", PublicationYear = 1960, Description = "A novel about racial injustice in the Deep South.", PageCount = 336, TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "1984", ISBN = "978-0451524935", Publisher = "Signet Classics", PublicationYear = 1949, Description = "A dystopian social science fiction novel.", PageCount = 328, TotalCopies = 4, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Animal Farm", ISBN = "978-0451526342", Publisher = "Signet Classics", PublicationYear = 1945, Description = "An allegorical novella reflecting events leading to the Russian Revolution.", PageCount = 112, TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, TotalCopies = 3, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about the Dashwood sisters.", PageCount = 409, TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "I, Robot", ISBN = "978-0553382563", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of robot short stories.", PageCount = 224, TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "The End of Eternity", ISBN = "978-0765319197", Publisher = "Tor Books", PublicationYear = 1955, Description = "A science fiction novel about time travel.", PageCount = 191, TotalCopies = 1, AvailableCopies = 0, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "Team of Rivals", ISBN = "978-0743270755", Publisher = "Simon & Schuster", PublicationYear = 2005, Description = "The political genius of Abraham Lincoln.", PageCount = 944, TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "Cosmos", ISBN = "978-0345539434", Publisher = "Ballantine Books", PublicationYear = 1980, Description = "A personal voyage through the universe.", PageCount = 396, TotalCopies = 3, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "The Demon-Haunted World", ISBN = "978-0345409461", Publisher = "Ballantine Books", PublicationYear = 1995, Description = "Science as a candle in the dark.", PageCount = 457, TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "Emma", ISBN = "978-0141439587", Publisher = "Penguin Classics", PublicationYear = 1815, Description = "A comic novel about youthful hubris and romantic misunderstandings.", PageCount = 474, TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
        };
        context.Books.AddRange(books);

        // BookAuthors
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },   // To Kill a Mockingbird - Harper Lee
            new() { BookId = 2, AuthorId = 2 },   // 1984 - Orwell
            new() { BookId = 3, AuthorId = 2 },   // Animal Farm - Orwell
            new() { BookId = 4, AuthorId = 3 },   // Pride and Prejudice - Austen
            new() { BookId = 5, AuthorId = 3 },   // Sense and Sensibility - Austen
            new() { BookId = 6, AuthorId = 4 },   // Foundation - Asimov
            new() { BookId = 7, AuthorId = 4 },   // I, Robot - Asimov
            new() { BookId = 8, AuthorId = 4 },   // End of Eternity - Asimov
            new() { BookId = 9, AuthorId = 5 },   // Team of Rivals - Goodwin
            new() { BookId = 10, AuthorId = 6 },  // Cosmos - Sagan
            new() { BookId = 11, AuthorId = 6 },  // Demon-Haunted World - Sagan
            new() { BookId = 12, AuthorId = 3 },  // Emma - Austen
        };
        context.BookAuthors.AddRange(bookAuthors);

        // BookCategories
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 },  // Mockingbird - Fiction
            new() { BookId = 1, CategoryId = 6 },  // Mockingbird - Classic
            new() { BookId = 2, CategoryId = 1 },  // 1984 - Fiction
            new() { BookId = 2, CategoryId = 2 },  // 1984 - Sci-Fi
            new() { BookId = 2, CategoryId = 6 },  // 1984 - Classic
            new() { BookId = 3, CategoryId = 1 },  // Animal Farm - Fiction
            new() { BookId = 3, CategoryId = 6 },  // Animal Farm - Classic
            new() { BookId = 4, CategoryId = 1 },  // P&P - Fiction
            new() { BookId = 4, CategoryId = 6 },  // P&P - Classic
            new() { BookId = 5, CategoryId = 1 },  // S&S - Fiction
            new() { BookId = 5, CategoryId = 6 },  // S&S - Classic
            new() { BookId = 6, CategoryId = 2 },  // Foundation - Sci-Fi
            new() { BookId = 7, CategoryId = 2 },  // I, Robot - Sci-Fi
            new() { BookId = 8, CategoryId = 2 },  // End of Eternity - Sci-Fi
            new() { BookId = 9, CategoryId = 3 },  // Team of Rivals - History
            new() { BookId = 9, CategoryId = 5 },  // Team of Rivals - Biography
            new() { BookId = 10, CategoryId = 4 }, // Cosmos - Science
            new() { BookId = 11, CategoryId = 4 }, // Demon-Haunted World - Science
            new() { BookId = 12, CategoryId = 1 }, // Emma - Fiction
            new() { BookId = 12, CategoryId = 6 }, // Emma - Classic
        };
        context.BookCategories.AddRange(bookCategories);

        // Patrons
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Main St", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Rd", MembershipDate = new DateOnly(2023, 6, 10), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Elm St", MembershipDate = new DateOnly(2023, 9, 1), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Eve", LastName = "Davis", Email = "eve.davis@email.com", Phone = "555-0105", Address = "654 Birch Ln", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Frank", LastName = "Miller", Email = "frank.miller@email.com", Phone = "555-0106", Address = "987 Cedar Dr", MembershipDate = new DateOnly(2024, 2, 14), MembershipType = MembershipType.Student, IsActive = false, CreatedAt = now, UpdatedAt = now },
        };
        context.Patrons.AddRange(patrons);

        // Loans (8+): mix of Active, Returned, Overdue
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active, CreatedAt = now },
            new() { Id = 2, BookId = 2, PatronId = 2, LoanDate = now.AddDays(-10), DueDate = now.AddDays(4), Status = LoanStatus.Active, CreatedAt = now },
            new() { Id = 3, BookId = 3, PatronId = 3, LoanDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = LoanStatus.Active, CreatedAt = now },
            new() { Id = 4, BookId = 6, PatronId = 1, LoanDate = now.AddDays(-7), DueDate = now.AddDays(14), Status = LoanStatus.Active, CreatedAt = now },
            // Overdue loans
            new() { Id = 5, BookId = 2, PatronId = 4, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue, CreatedAt = now },
            new() { Id = 6, BookId = 7, PatronId = 5, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue, CreatedAt = now },
            // Returned loans
            new() { Id = 7, BookId = 8, PatronId = 2, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-14), Status = LoanStatus.Returned, CreatedAt = now },
            new() { Id = 8, BookId = 4, PatronId = 1, LoanDate = now.AddDays(-40), DueDate = now.AddDays(-19), ReturnDate = now.AddDays(-22), Status = LoanStatus.Returned, CreatedAt = now },
            // Another returned - returned late (for fine)
            new() { Id = 9, BookId = 10, PatronId = 4, LoanDate = now.AddDays(-35), DueDate = now.AddDays(-21), ReturnDate = now.AddDays(-18), Status = LoanStatus.Returned, CreatedAt = now },
        };
        context.Loans.AddRange(loans);

        // Reservations
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 8, PatronId = 1, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now },
            new() { Id = 2, BookId = 8, PatronId = 3, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 2, CreatedAt = now },
            new() { Id = 3, BookId = 2, PatronId = 5, ReservationDate = now.AddDays(-3), ExpirationDate = now.AddDays(1), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now },
        };
        context.Reservations.AddRange(reservations);

        // Fines
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 4, LoanId = 9, Amount = 0.75m, Reason = "Overdue return — 3 days late", IssuedDate = now.AddDays(-18), Status = FineStatus.Unpaid, CreatedAt = now },
            new() { Id = 2, PatronId = 2, LoanId = 7, Amount = 0.50m, Reason = "Overdue return — 2 days late", IssuedDate = now.AddDays(-14), PaidDate = now.AddDays(-10), Status = FineStatus.Paid, CreatedAt = now },
            new() { Id = 3, PatronId = 4, LoanId = 5, Amount = 1.50m, Reason = "Overdue return — 6 days and counting", IssuedDate = now.AddDays(-6), Status = FineStatus.Unpaid, CreatedAt = now },
        };
        context.Fines.AddRange(fines);

        await context.SaveChangesAsync();
    }
}
