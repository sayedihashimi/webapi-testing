using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(LibraryDbContext db)
    {
        if (await db.Authors.AnyAsync())
            return; // Already seeded

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her six major novels, which interpret, critique, and comment upon the British landed gentry at the end of the 18th century.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "George", LastName = "Orwell", Biography = "English novelist, essayist, journalist, and critic. His work is characterized by lucid prose, social criticism, and opposition to totalitarianism.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom", CreatedAt = DateTime.UtcNow },
            new() { Id = 3, FirstName = "Gabriel", LastName = "García Márquez", Biography = "Colombian novelist, short-story writer, and journalist, known affectionately as Gabo throughout Latin America. A master of magical realism.", BirthDate = new DateOnly(1927, 3, 6), Country = "Colombia", CreatedAt = DateTime.UtcNow },
            new() { Id = 4, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for his works of science fiction and popular science.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = DateTime.UtcNow },
            new() { Id = 5, FirstName = "Toni", LastName = "Morrison", Biography = "American novelist, essayist, editor, and professor emeritus at Princeton University. Won the Nobel Prize in Literature in 1993.", BirthDate = new DateOnly(1931, 2, 18), Country = "United States", CreatedAt = DateTime.UtcNow },
            new() { Id = 6, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli public intellectual, historian, and professor at the Hebrew University of Jerusalem. Author of popular science bestsellers.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel", CreatedAt = DateTime.UtcNow },
        };
        db.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction based on imagined future scientific or technological advances" },
            new() { Id = 3, Name = "History", Description = "Books about historical events, periods, and figures" },
            new() { Id = 4, Name = "Science", Description = "Books about scientific topics and discoveries" },
            new() { Id = 5, Name = "Biography", Description = "Books about the lives of real people" },
            new() { Id = 6, Name = "Classic Literature", Description = "Enduring works of literary fiction from past centuries" },
        };
        db.Categories.AddRange(categories);

        var now = DateTime.UtcNow;

        // Books (12+ books)
        var books = new List<Book>
        {
            new() { Id = 1, Title = "Pride and Prejudice", ISBN = "9780141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel following the Bennet family.", PageCount = 432, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "1984", ISBN = "9780451524935", Publisher = "Signet Classic", PublicationYear = 1949, Description = "A dystopian novel set in Airstrip One, a province of the superstate Oceania.", PageCount = 328, Language = "English", TotalCopies = 4, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Animal Farm", ISBN = "9780451526342", Publisher = "Signet Classic", PublicationYear = 1945, Description = "An allegorical novella reflecting events leading up to the Russian Revolution.", PageCount = 112, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "One Hundred Years of Solitude", ISBN = "9780060883287", Publisher = "Harper Perennial", PublicationYear = 1967, Description = "The multi-generational story of the Buendía family in Macondo.", PageCount = 417, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "Foundation", ISBN = "9780553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series about the fall of the Galactic Empire.", PageCount = 244, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "I, Robot", ISBN = "9780553294385", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of nine science fiction short stories about robots.", PageCount = 224, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "Beloved", ISBN = "9781400033416", Publisher = "Vintage", PublicationYear = 1987, Description = "A novel inspired by the story of Margaret Garner, an African American who escaped slavery.", PageCount = 324, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "Song of Solomon", ISBN = "9781400033423", Publisher = "Vintage", PublicationYear = 1977, Description = "A novel about an African-American man's search for identity.", PageCount = 337, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "Sapiens: A Brief History of Humankind", ISBN = "9780062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A book that explores the history of the human species from the Stone Age to the present.", PageCount = 464, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "Sense and Sensibility", ISBN = "9780141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about the Dashwood sisters and their experiences of love and heartbreak.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "Love in the Time of Cholera", ISBN = "9780307389732", Publisher = "Vintage", PublicationYear = 1985, Description = "A love story spanning over fifty years in a Caribbean coastal city.", PageCount = 348, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "9780062464316", Publisher = "Harper", PublicationYear = 2017, Description = "A book examining what might happen to the world when old myths are coupled with new godlike technologies.", PageCount = 464, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
        };
        db.Books.AddRange(books);

        // BookAuthor join records
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },   // Pride and Prejudice - Jane Austen
            new() { BookId = 10, AuthorId = 1 },   // Sense and Sensibility - Jane Austen
            new() { BookId = 2, AuthorId = 2 },    // 1984 - George Orwell
            new() { BookId = 3, AuthorId = 2 },    // Animal Farm - George Orwell
            new() { BookId = 4, AuthorId = 3 },    // One Hundred Years - García Márquez
            new() { BookId = 11, AuthorId = 3 },   // Love in the Time of Cholera - García Márquez
            new() { BookId = 5, AuthorId = 4 },    // Foundation - Isaac Asimov
            new() { BookId = 6, AuthorId = 4 },    // I, Robot - Isaac Asimov
            new() { BookId = 7, AuthorId = 5 },    // Beloved - Toni Morrison
            new() { BookId = 8, AuthorId = 5 },    // Song of Solomon - Toni Morrison
            new() { BookId = 9, AuthorId = 6 },    // Sapiens - Yuval Noah Harari
            new() { BookId = 12, AuthorId = 6 },   // Homo Deus - Yuval Noah Harari
        };
        db.BookAuthors.AddRange(bookAuthors);

        // BookCategory join records
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 },  // Pride and Prejudice - Fiction
            new() { BookId = 1, CategoryId = 6 },  // Pride and Prejudice - Classic Literature
            new() { BookId = 2, CategoryId = 1 },  // 1984 - Fiction
            new() { BookId = 2, CategoryId = 2 },  // 1984 - Science Fiction
            new() { BookId = 2, CategoryId = 6 },  // 1984 - Classic Literature
            new() { BookId = 3, CategoryId = 1 },  // Animal Farm - Fiction
            new() { BookId = 3, CategoryId = 6 },  // Animal Farm - Classic Literature
            new() { BookId = 4, CategoryId = 1 },  // One Hundred Years - Fiction
            new() { BookId = 4, CategoryId = 6 },  // One Hundred Years - Classic Literature
            new() { BookId = 5, CategoryId = 2 },  // Foundation - Science Fiction
            new() { BookId = 6, CategoryId = 2 },  // I, Robot - Science Fiction
            new() { BookId = 7, CategoryId = 1 },  // Beloved - Fiction
            new() { BookId = 8, CategoryId = 1 },  // Song of Solomon - Fiction
            new() { BookId = 9, CategoryId = 3 },  // Sapiens - History
            new() { BookId = 9, CategoryId = 4 },  // Sapiens - Science
            new() { BookId = 10, CategoryId = 1 }, // Sense and Sensibility - Fiction
            new() { BookId = 10, CategoryId = 6 }, // Sense and Sensibility - Classic Literature
            new() { BookId = 11, CategoryId = 1 }, // Love in Time of Cholera - Fiction
            new() { BookId = 12, CategoryId = 3 }, // Homo Deus - History
            new() { BookId = 12, CategoryId = 4 }, // Homo Deus - Science
        };
        db.BookCategories.AddRange(bookCategories);

        // Patrons (6+, mixed membership types)
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Main St, Springfield", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave, Springfield", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Clara", LastName = "Davis", Email = "clara.davis@university.edu", Phone = "555-0103", Address = "789 College Blvd, Springfield", MembershipDate = new DateOnly(2023, 9, 1), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "David", LastName = "Wilson", Email = "david.wilson@email.com", Phone = "555-0104", Address = "321 Pine Rd, Springfield", MembershipDate = new DateOnly(2022, 6, 10), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Emma", LastName = "Brown", Email = "emma.brown@email.com", Phone = "555-0105", Address = "654 Elm St, Springfield", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Frank", LastName = "Taylor", Email = "frank.taylor@email.com", Phone = "555-0106", Address = "987 Maple Dr, Springfield", MembershipDate = new DateOnly(2023, 11, 15), MembershipType = MembershipType.Student, IsActive = false, CreatedAt = now, UpdatedAt = now },
        };
        db.Patrons.AddRange(patrons);

        await db.SaveChangesAsync();

        // Loans (8+ in various states)
        // Active loans: reduce available copies accordingly
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-10) },
            new() { Id = 2, BookId = 2, PatronId = 2, LoanDate = now.AddDays(-7), DueDate = now.AddDays(7), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-7) },
            new() { Id = 3, BookId = 2, PatronId = 3, LoanDate = now.AddDays(-5), DueDate = now.AddDays(2), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-5) },
            new() { Id = 4, BookId = 4, PatronId = 1, LoanDate = now.AddDays(-12), DueDate = now.AddDays(9), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-12) },

            // Overdue loans
            new() { Id = 5, BookId = 3, PatronId = 4, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-20) },
            new() { Id = 6, BookId = 7, PatronId = 2, LoanDate = now.AddDays(-18), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue, RenewalCount = 1, CreatedAt = now.AddDays(-18) },

            // Returned loans
            new() { Id = 7, BookId = 5, PatronId = 5, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-9), ReturnDate = now.AddDays(-8), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-30) },
            new() { Id = 8, BookId = 9, PatronId = 3, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-4), ReturnDate = now.AddDays(-10), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-25) },
            new() { Id = 9, BookId = 11, PatronId = 4, LoanDate = now.AddDays(-35), DueDate = now.AddDays(-21), ReturnDate = now.AddDays(-18), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-35) },
        };
        db.Loans.AddRange(loans);

        // AvailableCopies adjustments for active/overdue loans:
        // Book 1: 3 total, 1 active loan => 2 available ✓
        // Book 2: 4 total, 2 active loans => 2 available ✓
        // Book 3: 2 total, 1 overdue loan => 1 available ✓
        // Book 4: 2 total, 1 active loan => 1 available ✓
        // Book 5: 3 total, 0 active => 3 available (loan returned) - wait, seed says 2 available
        // Let's fix: Book 5 has an active loan from patron 1
        // Actually let me keep it simpler - the available copies above are already set correctly for the active/overdue loans

        // Fix: Book 5 should have 2 available (returned loan already handled), let me adjust
        // Book 5: 3 copies, loan 7 is returned => 3 available. But seed set 2. Let me add one more active loan.
        // Actually the seed data for books already sets the correct available copies. Let me verify:
        // Book 1: 3 total, loan 1 active => available = 2 ✓
        // Book 2: 4 total, loans 2,3 active => available = 2 ✓
        // Book 3: 2 total, loan 5 overdue => available = 1 ✓
        // Book 4: 2 total, loan 4 active => available = 1 ✓
        // Book 5: 3 total, loan 7 returned => available = 3, but seed says 2. Fix: add active loan
        // Book 7: 2 total, loan 6 overdue => available = 1 ✓
        // Book 9: 3 total, loan 8 returned => available = 3, but seed says 2. Fix: add active loan
        // Book 11: 2 total, loan 9 returned => available = 2, but seed says 1. Fix: add active loan

        // Correction: Update available copies for books where all loans are returned
        var book5 = await db.Books.FindAsync(5);
        book5!.AvailableCopies = 3; // All loans returned
        var book9 = await db.Books.FindAsync(9);
        book9!.AvailableCopies = 3; // All loans returned
        var book11 = await db.Books.FindAsync(11);
        book11!.AvailableCopies = 2; // All loans returned

        await db.SaveChangesAsync();

        // Reservations (3+: Pending and Ready)
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 3, PatronId = 1, ReservationDate = now.AddDays(-3), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-3) },
            new() { Id = 2, BookId = 2, PatronId = 5, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-2) },
            new() { Id = 3, BookId = 7, PatronId = 3, ReservationDate = now.AddDays(-5), ExpirationDate = now.AddDays(1), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now.AddDays(-5) },
        };
        db.Reservations.AddRange(reservations);

        // Fines (3+: Unpaid and Paid)
        // Loan 9 was returned 3 days late (due -21, returned -18), so fine = 3 * 0.25 = $0.75
        // Loan 5 is overdue by 6 days (but still not returned - fine will be generated on return)
        // Let's create fines for previously returned overdue loans and one for a damaged book
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 4, LoanId = 9, Amount = 0.75m, Reason = "Overdue return - 3 day(s) late", IssuedDate = now.AddDays(-18), Status = FineStatus.Paid, PaidDate = now.AddDays(-15), CreatedAt = now.AddDays(-18) },
            new() { Id = 2, PatronId = 4, LoanId = 5, Amount = 5.00m, Reason = "Damaged book - water damage to pages", IssuedDate = now.AddDays(-5), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-5) },
            new() { Id = 3, PatronId = 2, LoanId = 6, Amount = 8.50m, Reason = "Damaged book - torn cover and missing pages", IssuedDate = now.AddDays(-2), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-2) },
        };
        db.Fines.AddRange(fines);

        await db.SaveChangesAsync();
    }
}
