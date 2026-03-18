using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(LibraryDbContext context)
    {
        if (await context.Authors.AnyAsync())
            return; // Already seeded

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, journalist and critic, known for '1984' and 'Animal Farm'.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom" },
            new() { Id = 2, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known primarily for six major novels interpreting and critiquing the British landed gentry.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom" },
            new() { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for his works of science fiction and popular science.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States" },
            new() { Id = 4, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli public intellectual, historian, and professor at the Hebrew University of Jerusalem.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel" },
            new() { Id = 5, FirstName = "Agatha", LastName = "Christie", Biography = "English writer known for her 66 detective novels and 14 short story collections.", BirthDate = new DateOnly(1890, 9, 15), Country = "United Kingdom" },
            new() { Id = 6, FirstName = "Carl", LastName = "Sagan", Biography = "American astronomer, planetary scientist, cosmologist, astrophysicist, and author.", BirthDate = new DateOnly(1934, 11, 9), Country = "United States" },
        };
        context.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction based on imagined future scientific or technological advances" },
            new() { Id = 3, Name = "History", Description = "Non-fiction works about past events" },
            new() { Id = 4, Name = "Science", Description = "Non-fiction works about the natural world and scientific discoveries" },
            new() { Id = 5, Name = "Biography", Description = "Account of someone's life written by someone else" },
            new() { Id = 6, Name = "Mystery", Description = "Fiction dealing with the solution of a crime or puzzle" },
        };
        context.Categories.AddRange(categories);

        // Books
        var now = DateTime.UtcNow;
        var books = new List<Book>
        {
            new() { Id = 1, Title = "1984", ISBN = "978-0451524935", Publisher = "Signet Classics", PublicationYear = 1949, Description = "A dystopian novel set in a totalitarian society.", PageCount = 328, Language = "English", TotalCopies = 3, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "Animal Farm", ISBN = "978-0451526342", Publisher = "Signet Classics", PublicationYear = 1945, Description = "An allegorical novella about a group of farm animals.", PageCount = 112, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about two sisters navigating love and heartbreak.", PageCount = 409, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Isaac Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "I, Robot", ISBN = "978-0553294385", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of nine science fiction short stories.", PageCount = 224, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A book exploring the history of humankind from the Stone Age to the present.", PageCount = 464, Language = "English", TotalCopies = 4, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "Murder on the Orient Express", ISBN = "978-0062693662", Publisher = "William Morrow", PublicationYear = 1934, Description = "A detective novel featuring Hercule Poirot.", PageCount = 274, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "And Then There Were None", ISBN = "978-0062073488", Publisher = "William Morrow", PublicationYear = 1939, Description = "A mystery novel considered Christie's masterpiece.", PageCount = 272, Language = "English", TotalCopies = 1, AvailableCopies = 0, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "Cosmos", ISBN = "978-0345539434", Publisher = "Ballantine Books", PublicationYear = 1980, Description = "A popular science book exploring the universe.", PageCount = 396, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0062464316", Publisher = "Harper", PublicationYear = 2017, Description = "A look at the future of humanity.", PageCount = 464, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "The Pale Blue Dot", ISBN = "978-0345376596", Publisher = "Ballantine Books", PublicationYear = 1994, Description = "A vision of the human future in space.", PageCount = 429, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
        };
        context.Books.AddRange(books);

        // BookAuthors
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },  // 1984 - Orwell
            new() { BookId = 2, AuthorId = 1 },  // Animal Farm - Orwell
            new() { BookId = 3, AuthorId = 2 },  // Pride and Prejudice - Austen
            new() { BookId = 4, AuthorId = 2 },  // Sense and Sensibility - Austen
            new() { BookId = 5, AuthorId = 3 },  // Foundation - Asimov
            new() { BookId = 6, AuthorId = 3 },  // I, Robot - Asimov
            new() { BookId = 7, AuthorId = 4 },  // Sapiens - Harari
            new() { BookId = 8, AuthorId = 5 },  // Murder on the Orient Express - Christie
            new() { BookId = 9, AuthorId = 5 },  // And Then There Were None - Christie
            new() { BookId = 10, AuthorId = 6 }, // Cosmos - Sagan
            new() { BookId = 11, AuthorId = 4 }, // Homo Deus - Harari
            new() { BookId = 12, AuthorId = 6 }, // The Pale Blue Dot - Sagan
        };
        context.BookAuthors.AddRange(bookAuthors);

        // BookCategories
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 },  // 1984 - Fiction
            new() { BookId = 1, CategoryId = 2 },  // 1984 - Science Fiction
            new() { BookId = 2, CategoryId = 1 },  // Animal Farm - Fiction
            new() { BookId = 3, CategoryId = 1 },  // Pride and Prejudice - Fiction
            new() { BookId = 4, CategoryId = 1 },  // Sense and Sensibility - Fiction
            new() { BookId = 5, CategoryId = 2 },  // Foundation - Science Fiction
            new() { BookId = 6, CategoryId = 2 },  // I, Robot - Science Fiction
            new() { BookId = 7, CategoryId = 3 },  // Sapiens - History
            new() { BookId = 7, CategoryId = 4 },  // Sapiens - Science
            new() { BookId = 8, CategoryId = 1 },  // Murder on the Orient Express - Fiction
            new() { BookId = 8, CategoryId = 6 },  // Murder on the Orient Express - Mystery
            new() { BookId = 9, CategoryId = 1 },  // And Then There Were None - Fiction
            new() { BookId = 9, CategoryId = 6 },  // And Then There Were None - Mystery
            new() { BookId = 10, CategoryId = 4 }, // Cosmos - Science
            new() { BookId = 11, CategoryId = 3 }, // Homo Deus - History
            new() { BookId = 12, CategoryId = 4 }, // The Pale Blue Dot - Science
        };
        context.BookCategories.AddRange(bookCategories);

        // Patrons
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Main St, Springfield", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave, Springfield", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Rd, Springfield", MembershipDate = new DateOnly(2023, 6, 10), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Elm St, Springfield", MembershipDate = new DateOnly(2023, 9, 5), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Eva", LastName = "Martinez", Email = "eva.martinez@email.com", Phone = "555-0105", Address = "654 Maple Dr, Springfield", MembershipDate = new DateOnly(2024, 1, 12), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Frank", LastName = "Davis", Email = "frank.davis@email.com", Phone = "555-0106", Address = "987 Cedar Ln, Springfield", MembershipDate = new DateOnly(2023, 5, 25), MembershipType = MembershipType.Standard, IsActive = false, CreatedAt = now, UpdatedAt = now },
        };
        context.Patrons.AddRange(patrons);

        // Loans — mix of Active, Returned, Overdue
        // Book 1 (1984): 3 copies, 1 available → 2 active loans
        // Book 2 (Animal Farm): 2 copies, 1 available → 1 active loan
        // Book 5 (Foundation): 3 copies, 2 available → 1 active loan
        // Book 7 (Sapiens): 4 copies, 3 available → 1 active loan
        // Book 8 (Murder Orient Express): 2 copies, 1 available → 1 active loan
        // Book 9 (And Then There Were None): 1 copy, 0 available → 1 active loan
        // Book 11 (Homo Deus): 2 copies, 1 available → 1 active loan
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-10) },
            new() { Id = 2, BookId = 1, PatronId = 2, LoanDate = now.AddDays(-5), DueDate = now.AddDays(9), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-5) },
            new() { Id = 3, BookId = 2, PatronId = 3, LoanDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-3) },
            new() { Id = 4, BookId = 5, PatronId = 1, LoanDate = now.AddDays(-7), DueDate = now.AddDays(14), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-7) },
            new() { Id = 5, BookId = 8, PatronId = 4, LoanDate = now.AddDays(-12), DueDate = now.AddDays(2), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-12) },
            // Overdue loan
            new() { Id = 6, BookId = 9, PatronId = 2, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-20) },
            // Returned loans
            new() { Id = 7, BookId = 7, PatronId = 3, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-23), ReturnDate = now.AddDays(-20), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-30) },
            new() { Id = 8, BookId = 11, PatronId = 5, LoanDate = now.AddDays(-15), DueDate = now.AddDays(6), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-15) },
            // Active loan for Sapiens
            new() { Id = 9, BookId = 7, PatronId = 4, LoanDate = now.AddDays(-8), DueDate = now.AddDays(6), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-8) },
            // Returned loan with late return (generated a fine)
            new() { Id = 10, BookId = 10, PatronId = 1, LoanDate = now.AddDays(-40), DueDate = now.AddDays(-19), ReturnDate = now.AddDays(-15), Status = LoanStatus.Returned, RenewalCount = 1, CreatedAt = now.AddDays(-40) },
        };
        context.Loans.AddRange(loans);

        // Reservations
        var reservations = new List<Reservation>
        {
            // Pending reservation for book 9 (And Then There Were None) — no copies available
            new() { Id = 1, BookId = 9, PatronId = 4, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-2) },
            new() { Id = 2, BookId = 9, PatronId = 5, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 2, CreatedAt = now.AddDays(-1) },
            // Ready reservation for book 1 (1984) — someone returned a copy
            new() { Id = 3, BookId = 1, PatronId = 3, ReservationDate = now.AddDays(-5), ExpirationDate = now.AddDays(2), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now.AddDays(-5) },
        };
        context.Reservations.AddRange(reservations);

        // Fines
        var fines = new List<Fine>
        {
            // Unpaid fine for overdue return (loan 10: 4 days overdue = $1.00)
            new() { Id = 1, PatronId = 1, LoanId = 10, Amount = 1.00m, Reason = "Overdue return — 4 days late", IssuedDate = now.AddDays(-15), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-15) },
            // Paid fine
            new() { Id = 2, PatronId = 3, LoanId = 7, Amount = 0.75m, Reason = "Overdue return — 3 days late", IssuedDate = now.AddDays(-20), PaidDate = now.AddDays(-18), Status = FineStatus.Paid, CreatedAt = now.AddDays(-20) },
            // Unpaid fine for currently overdue loan 6
            new() { Id = 3, PatronId = 2, LoanId = 6, Amount = 1.50m, Reason = "Overdue return — 6 days late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-1) },
            // Waived fine
            new() { Id = 4, PatronId = 4, LoanId = 7, Amount = 0.50m, Reason = "Damaged book cover", IssuedDate = now.AddDays(-19), Status = FineStatus.Waived, CreatedAt = now.AddDays(-19) },
        };
        context.Fines.AddRange(fines);

        await context.SaveChangesAsync();
    }
}
