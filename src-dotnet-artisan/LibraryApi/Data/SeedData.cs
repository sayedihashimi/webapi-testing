using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(LibraryDbContext db)
    {
        if (await db.Authors.AnyAsync())
            return;

        // Authors
        var authors = new[]
        {
            new Author { Id = 1, FirstName = "Harper", LastName = "Lee", Biography = "American novelist best known for 'To Kill a Mockingbird'.", BirthDate = new DateOnly(1926, 4, 28), Country = "United States" },
            new Author { Id = 2, FirstName = "Frank", LastName = "Herbert", Biography = "American science fiction author best known for the novel Dune.", BirthDate = new DateOnly(1920, 10, 8), Country = "United States" },
            new Author { Id = 3, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and professor at the Hebrew University of Jerusalem.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel" },
            new Author { Id = 4, FirstName = "Carl", LastName = "Sagan", Biography = "American astronomer, planetary scientist, and science communicator.", BirthDate = new DateOnly(1934, 11, 9), Country = "United States" },
            new Author { Id = 5, FirstName = "Walter", LastName = "Isaacson", Biography = "American journalist and biographer known for works on Steve Jobs and Einstein.", BirthDate = new DateOnly(1952, 5, 20), Country = "United States" }
        };
        db.Authors.AddRange(authors);

        // Categories
        var categories = new[]
        {
            new Category { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new Category { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science and technology" },
            new Category { Id = 3, Name = "History", Description = "Non-fiction works about past events" },
            new Category { Id = 4, Name = "Science", Description = "Non-fiction works about natural and physical sciences" },
            new Category { Id = 5, Name = "Biography", Description = "Accounts of someone's life written by another person" }
        };
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        // Books (12 books)
        var books = new[]
        {
            new Book { Id = 1, Title = "To Kill a Mockingbird", ISBN = "978-0-06-112008-4", Publisher = "J. B. Lippincott & Co.", PublicationYear = 1960, Description = "A novel about racial injustice in the American South.", PageCount = 281, Language = "English", TotalCopies = 4, AvailableCopies = 2 },
            new Book { Id = 2, Title = "Dune", ISBN = "978-0-441-17271-9", Publisher = "Chilton Books", PublicationYear = 1965, Description = "A science fiction epic about politics, religion, and ecology on a desert planet.", PageCount = 412, Language = "English", TotalCopies = 3, AvailableCopies = 1 },
            new Book { Id = 3, Title = "Dune Messiah", ISBN = "978-0-593-09880-5", Publisher = "Putnam", PublicationYear = 1969, Description = "The sequel to Dune, continuing Paul Atreides' story.", PageCount = 256, Language = "English", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Id = 4, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0-06-231609-7", Publisher = "Harper", PublicationYear = 2014, Description = "A narrative of humanity's creation and evolution.", PageCount = 443, Language = "English", TotalCopies = 5, AvailableCopies = 3 },
            new Book { Id = 5, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0-06-246431-6", Publisher = "Harper", PublicationYear = 2017, Description = "Examines what might happen when mythology merges with technology.", PageCount = 464, Language = "English", TotalCopies = 3, AvailableCopies = 2 },
            new Book { Id = 6, Title = "Cosmos", ISBN = "978-0-345-53943-4", Publisher = "Random House", PublicationYear = 1980, Description = "Explores the universe, the history of science, and humanity's place in it.", PageCount = 365, Language = "English", TotalCopies = 3, AvailableCopies = 2 },
            new Book { Id = 7, Title = "Pale Blue Dot", ISBN = "978-0-345-37659-6", Publisher = "Random House", PublicationYear = 1994, Description = "A vision of the human future in space.", PageCount = 429, Language = "English", TotalCopies = 2, AvailableCopies = 1 },
            new Book { Id = 8, Title = "Steve Jobs", ISBN = "978-1-4516-4853-9", Publisher = "Simon & Schuster", PublicationYear = 2011, Description = "The biography of Apple co-founder Steve Jobs.", PageCount = 656, Language = "English", TotalCopies = 4, AvailableCopies = 3 },
            new Book { Id = 9, Title = "Einstein: His Life and Universe", ISBN = "978-0-7432-6473-0", Publisher = "Simon & Schuster", PublicationYear = 2007, Description = "Biography of Albert Einstein.", PageCount = 675, Language = "English", TotalCopies = 2, AvailableCopies = 1 },
            new Book { Id = 10, Title = "21 Lessons for the 21st Century", ISBN = "978-0-525-51217-2", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "Addresses the biggest questions of our time.", PageCount = 372, Language = "English", TotalCopies = 3, AvailableCopies = 3 },
            new Book { Id = 11, Title = "Go Set a Watchman", ISBN = "978-0-06-240985-0", Publisher = "HarperCollins", PublicationYear = 2015, Description = "Harper Lee's second published novel set in the 1950s.", PageCount = 278, Language = "English", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Id = 12, Title = "Children of Dune", ISBN = "978-0-593-09882-9", Publisher = "Putnam", PublicationYear = 1976, Description = "The third book in the Dune saga.", PageCount = 408, Language = "English", TotalCopies = 2, AvailableCopies = 2 }
        };
        db.Books.AddRange(books);
        await db.SaveChangesAsync();

        // Book-Author relationships
        db.BookAuthors.AddRange(
            new BookAuthor { BookId = 1, AuthorId = 1 },
            new BookAuthor { BookId = 2, AuthorId = 2 },
            new BookAuthor { BookId = 3, AuthorId = 2 },
            new BookAuthor { BookId = 4, AuthorId = 3 },
            new BookAuthor { BookId = 5, AuthorId = 3 },
            new BookAuthor { BookId = 6, AuthorId = 4 },
            new BookAuthor { BookId = 7, AuthorId = 4 },
            new BookAuthor { BookId = 8, AuthorId = 5 },
            new BookAuthor { BookId = 9, AuthorId = 5 },
            new BookAuthor { BookId = 10, AuthorId = 3 },
            new BookAuthor { BookId = 11, AuthorId = 1 },
            new BookAuthor { BookId = 12, AuthorId = 2 }
        );

        // Book-Category relationships
        db.BookCategories.AddRange(
            new BookCategory { BookId = 1, CategoryId = 1 },   // Mockingbird → Fiction
            new BookCategory { BookId = 2, CategoryId = 2 },   // Dune → Sci-Fi
            new BookCategory { BookId = 2, CategoryId = 1 },   // Dune → Fiction
            new BookCategory { BookId = 3, CategoryId = 2 },   // Dune Messiah → Sci-Fi
            new BookCategory { BookId = 4, CategoryId = 3 },   // Sapiens → History
            new BookCategory { BookId = 4, CategoryId = 4 },   // Sapiens → Science
            new BookCategory { BookId = 5, CategoryId = 3 },   // Homo Deus → History
            new BookCategory { BookId = 5, CategoryId = 4 },   // Homo Deus → Science
            new BookCategory { BookId = 6, CategoryId = 4 },   // Cosmos → Science
            new BookCategory { BookId = 7, CategoryId = 4 },   // Pale Blue Dot → Science
            new BookCategory { BookId = 8, CategoryId = 5 },   // Steve Jobs → Biography
            new BookCategory { BookId = 9, CategoryId = 5 },   // Einstein → Biography
            new BookCategory { BookId = 10, CategoryId = 3 },  // 21 Lessons → History
            new BookCategory { BookId = 11, CategoryId = 1 },  // Go Set a Watchman → Fiction
            new BookCategory { BookId = 12, CategoryId = 2 }   // Children of Dune → Sci-Fi
        );
        await db.SaveChangesAsync();

        // Patrons
        var patrons = new[]
        {
            new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak St, Springfield", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true },
            new Patron { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Maple Ave, Springfield", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true },
            new Patron { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Rd, Springfield", MembershipDate = new DateOnly(2023, 6, 1), MembershipType = MembershipType.Student, IsActive = true },
            new Patron { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Elm St, Springfield", MembershipDate = new DateOnly(2023, 8, 10), MembershipType = MembershipType.Standard, IsActive = true },
            new Patron { Id = 5, FirstName = "Eve", LastName = "Davis", Email = "eve.davis@email.com", Phone = "555-0105", Address = "654 Birch Ln, Springfield", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Premium, IsActive = true },
            new Patron { Id = 6, FirstName = "Frank", LastName = "Miller", Email = "frank.miller@email.com", Phone = "555-0106", Address = "987 Cedar Dr, Springfield", MembershipDate = new DateOnly(2022, 11, 20), MembershipType = MembershipType.Standard, IsActive = false }
        };
        db.Patrons.AddRange(patrons);
        await db.SaveChangesAsync();

        // Loans (8 total: some active, some returned, some overdue)
        // Active loans must match AvailableCopies reductions above
        var now = DateTime.UtcNow;
        var loans = new[]
        {
            // Active loans — these account for reduced AvailableCopies
            new Loan { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, RenewalCount = 0 },     // Book 1: 4 total, 2 active → 2 available
            new Loan { Id = 2, BookId = 1, PatronId = 2, LoanDate = now.AddDays(-5), DueDate = now.AddDays(9), Status = LoanStatus.Active, RenewalCount = 0 },
            new Loan { Id = 3, BookId = 2, PatronId = 3, LoanDate = now.AddDays(-6), DueDate = now.AddDays(1), Status = LoanStatus.Active, RenewalCount = 0 },       // Book 2: 3 total, 2 active → 1 available
            new Loan { Id = 4, BookId = 2, PatronId = 1, LoanDate = now.AddDays(-3), DueDate = now.AddDays(18), Status = LoanStatus.Active, RenewalCount = 0 },
            new Loan { Id = 5, BookId = 4, PatronId = 4, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Active, RenewalCount = 0 },     // Book 4: 5 total, 2 active → 3 available (OVERDUE)
            new Loan { Id = 6, BookId = 4, PatronId = 5, LoanDate = now.AddDays(-8), DueDate = now.AddDays(13), Status = LoanStatus.Active, RenewalCount = 1 },
            // Returned loans
            new Loan { Id = 7, BookId = 7, PatronId = 2, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-14), Status = LoanStatus.Returned, RenewalCount = 0 },  // Book 7: returned, was overdue by 2 days
            new Loan { Id = 8, BookId = 9, PatronId = 1, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-4), ReturnDate = now.AddDays(-6), Status = LoanStatus.Returned, RenewalCount = 0 },     // Book 9: returned on time — but 9 has 2 total, 1 avail means 1 active needed... let me fix
        };

        // Fix: Book 9 has TotalCopies=2, AvailableCopies=1 → needs 1 active loan
        // Loan 8 is returned. Let's add a separate active loan for book 9.
        // Actually let me recalculate:
        // Book 1: 4 total, loans 1+2 active = 2 active → available = 2 ✓
        // Book 2: 3 total, loans 3+4 active = 2 active → available = 1 ✓
        // Book 4: 5 total, loans 5+6 active = 2 active → available = 3 ✓
        // Book 5: 3 total, 0 active → available = 3... but we said 2. Fix: set AvailableCopies to 3 for book 5
        // Book 7: 2 total, 0 active → available = 2... but we said 1. Need 1 active loan for book 7
        // Book 9: 2 total, 0 active → available = 2... but we said 1. Need 1 active loan for book 9
        // Book 6: 3 total, 0 active → available = 3... but we said 2. Need 1 active loan for book 6

        // Let me adjust available copies in the books above to match these 8 loans exactly:
        // The correct available copies with just these loans:
        // Book 1: 4 - 2 = 2 ✓
        // Book 2: 3 - 2 = 1 ✓
        // Book 4: 5 - 2 = 3 ✓
        // All others: totalCopies = availableCopies ✓
        // So I need to update Books 5,6,7,9 available copies. Let me fix them:

        db.Loans.AddRange(loans);
        await db.SaveChangesAsync();

        // Fix available copies to match: Books 5,6,7,9 should have avail = total (no active loans)
        var book5 = await db.Books.FindAsync(5);
        if (book5 != null) book5.AvailableCopies = 3; // was 2, no active loans
        var book6 = await db.Books.FindAsync(6);
        if (book6 != null) book6.AvailableCopies = 3; // was 2, no active loans
        var book7 = await db.Books.FindAsync(7);
        if (book7 != null) book7.AvailableCopies = 2; // was 1, no active loans (loan 7 returned)
        var book9 = await db.Books.FindAsync(9);
        if (book9 != null) book9.AvailableCopies = 2; // was 1, no active loans (loan 8 returned)
        await db.SaveChangesAsync();

        // Reservations (3: Pending and Ready)
        var reservations = new[]
        {
            new Reservation { Id = 1, BookId = 2, PatronId = 5, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new Reservation { Id = 2, BookId = 1, PatronId = 4, ReservationDate = now.AddDays(-1), ExpirationDate = now.AddDays(2), Status = ReservationStatus.Ready, QueuePosition = 1 },
            new Reservation { Id = 3, BookId = 2, PatronId = 4, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 2 }
        };
        db.Reservations.AddRange(reservations);
        await db.SaveChangesAsync();

        // Fines (3: Unpaid, Paid, Unpaid)
        var fines = new[]
        {
            new Fine { Id = 1, PatronId = 2, LoanId = 7, Amount = 0.50m, Reason = "Overdue by 2 day(s) at $0.25/day", IssuedDate = now.AddDays(-14), Status = FineStatus.Paid, PaidDate = now.AddDays(-13) },
            new Fine { Id = 2, PatronId = 4, LoanId = 5, Amount = 1.50m, Reason = "Overdue by 6 day(s) at $0.25/day", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid },
            new Fine { Id = 3, PatronId = 4, LoanId = 5, Amount = 12.00m, Reason = "Damaged book cover", IssuedDate = now.AddDays(-2), Status = FineStatus.Unpaid }
        };
        db.Fines.AddRange(fines);
        await db.SaveChangesAsync();
    }
}
