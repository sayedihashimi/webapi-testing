using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(LibraryDbContext db, CancellationToken ct = default)
    {
        if (await db.Authors.AnyAsync(ct))
        {
            return;
        }

        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, journalist and critic.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom" },
            new() { Id = 2, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known primarily for her six major novels.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom" },
            new() { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for science fiction.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States" },
            new() { Id = 4, FirstName = "Maya", LastName = "Angelou", Biography = "American poet, memoirist, and civil rights activist.", BirthDate = new DateOnly(1928, 4, 4), Country = "United States" },
            new() { Id = 5, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli public intellectual, historian and professor.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel" },
            new() { Id = 6, FirstName = "Carl", LastName = "Sagan", Biography = "American astronomer, planetary scientist, and science communicator.", BirthDate = new DateOnly(1934, 11, 9), Country = "United States" }
        };
        db.Authors.AddRange(authors);

        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works of imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science and technology" },
            new() { Id = 3, Name = "History", Description = "Non-fiction works about historical events" },
            new() { Id = 4, Name = "Science", Description = "Works about scientific discoveries and theories" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of someone's life written by someone else" },
            new() { Id = 6, Name = "Poetry", Description = "Literary works in verse form" }
        };
        db.Categories.AddRange(categories);

        var books = new List<Book>
        {
            new() { Id = 1, Title = "1984", ISBN = "978-0451524935", Publisher = "Signet Classics", PublicationYear = 1949, Description = "A dystopian social science fiction novel.", PageCount = 328, TotalCopies = 5, AvailableCopies = 3 },
            new() { Id = 2, Title = "Animal Farm", ISBN = "978-0451526342", Publisher = "Signet Classics", PublicationYear = 1945, Description = "An allegorical novella reflecting events leading to the Russian Revolution.", PageCount = 112, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 3, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, TotalCopies = 4, AvailableCopies = 3 },
            new() { Id = 4, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about the Dashwood sisters.", PageCount = 409, TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 5, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, TotalCopies = 3, AvailableCopies = 1 },
            new() { Id = 6, Title = "I, Robot", ISBN = "978-0553294385", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of nine science fiction short stories.", PageCount = 224, TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 7, Title = "I Know Why the Caged Bird Sings", ISBN = "978-0345514400", Publisher = "Ballantine Books", PublicationYear = 1969, Description = "The first of seven volumes of Maya Angelou's autobiography.", PageCount = 304, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 8, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A book about the history of the human species.", PageCount = 464, TotalCopies = 4, AvailableCopies = 2 },
            new() { Id = 9, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0062464316", Publisher = "Harper", PublicationYear = 2017, Description = "Examines the possible futures of humanity.", PageCount = 464, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 10, Title = "Cosmos", ISBN = "978-0345539434", Publisher = "Ballantine Books", PublicationYear = 1980, Description = "Explores the universe, its origins, and humanity's place within it.", PageCount = 432, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 11, Title = "The Pale Blue Dot", ISBN = "978-0345376596", Publisher = "Ballantine Books", PublicationYear = 1994, Description = "A vision of the human future in space.", PageCount = 429, TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 12, Title = "21 Lessons for the 21st Century", ISBN = "978-0525512196", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "Addresses the biggest challenges of the present moment.", PageCount = 372, TotalCopies = 3, AvailableCopies = 3 }
        };
        db.Books.AddRange(books);

        // Book-Author relationships
        db.BookAuthors.AddRange(
            new BookAuthor { BookId = 1, AuthorId = 1 },
            new BookAuthor { BookId = 2, AuthorId = 1 },
            new BookAuthor { BookId = 3, AuthorId = 2 },
            new BookAuthor { BookId = 4, AuthorId = 2 },
            new BookAuthor { BookId = 5, AuthorId = 3 },
            new BookAuthor { BookId = 6, AuthorId = 3 },
            new BookAuthor { BookId = 7, AuthorId = 4 },
            new BookAuthor { BookId = 8, AuthorId = 5 },
            new BookAuthor { BookId = 9, AuthorId = 5 },
            new BookAuthor { BookId = 10, AuthorId = 6 },
            new BookAuthor { BookId = 11, AuthorId = 6 },
            new BookAuthor { BookId = 12, AuthorId = 5 }
        );

        // Book-Category relationships
        db.BookCategories.AddRange(
            new BookCategory { BookId = 1, CategoryId = 1 },
            new BookCategory { BookId = 1, CategoryId = 2 },
            new BookCategory { BookId = 2, CategoryId = 1 },
            new BookCategory { BookId = 3, CategoryId = 1 },
            new BookCategory { BookId = 4, CategoryId = 1 },
            new BookCategory { BookId = 5, CategoryId = 2 },
            new BookCategory { BookId = 6, CategoryId = 2 },
            new BookCategory { BookId = 7, CategoryId = 5 },
            new BookCategory { BookId = 7, CategoryId = 6 },
            new BookCategory { BookId = 8, CategoryId = 3 },
            new BookCategory { BookId = 8, CategoryId = 4 },
            new BookCategory { BookId = 9, CategoryId = 3 },
            new BookCategory { BookId = 9, CategoryId = 4 },
            new BookCategory { BookId = 10, CategoryId = 4 },
            new BookCategory { BookId = 11, CategoryId = 4 },
            new BookCategory { BookId = 12, CategoryId = 3 }
        );

        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", MembershipType = MembershipType.Premium, MembershipDate = new DateOnly(2023, 1, 15) },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Elm Avenue", MembershipType = MembershipType.Standard, MembershipDate = new DateOnly(2023, 3, 20) },
            new() { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Road", MembershipType = MembershipType.Student, MembershipDate = new DateOnly(2023, 9, 1) },
            new() { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Maple Drive", MembershipType = MembershipType.Standard, MembershipDate = new DateOnly(2023, 6, 10) },
            new() { Id = 5, FirstName = "Eve", LastName = "Davis", Email = "eve.davis@email.com", Phone = "555-0105", Address = "654 Cedar Lane", MembershipType = MembershipType.Premium, MembershipDate = new DateOnly(2023, 2, 28) },
            new() { Id = 6, FirstName = "Frank", LastName = "Miller", Email = "frank.miller@email.com", Phone = "555-0106", Address = "987 Birch Court", MembershipType = MembershipType.Standard, IsActive = false, MembershipDate = new DateOnly(2022, 11, 5) }
        };
        db.Patrons.AddRange(patrons);

        var now = DateTime.UtcNow;
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active },
            new() { Id = 2, BookId = 1, PatronId = 2, LoanDate = now.AddDays(-5), DueDate = now.AddDays(9), Status = LoanStatus.Active },
            new() { Id = 3, BookId = 5, PatronId = 3, LoanDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = LoanStatus.Active },
            new() { Id = 4, BookId = 5, PatronId = 1, LoanDate = now.AddDays(-7), DueDate = now.AddDays(14), Status = LoanStatus.Active },
            new() { Id = 5, BookId = 8, PatronId = 4, LoanDate = now.AddDays(-12), DueDate = now.AddDays(2), Status = LoanStatus.Active },
            new() { Id = 6, BookId = 8, PatronId = 5, LoanDate = now.AddDays(-8), DueDate = now.AddDays(13), Status = LoanStatus.Active },
            // Returned loans
            new() { Id = 7, BookId = 3, PatronId = 2, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-18), Status = LoanStatus.Returned },
            // Overdue loans
            new() { Id = 8, BookId = 6, PatronId = 4, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue },
            new() { Id = 9, BookId = 11, PatronId = 2, LoanDate = now.AddDays(-18), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue },
            new() { Id = 10, BookId = 10, PatronId = 3, LoanDate = now.AddDays(-10), DueDate = now.AddDays(-3), Status = LoanStatus.Overdue }
        };
        db.Loans.AddRange(loans);

        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 5, PatronId = 2, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 2, BookId = 1, PatronId = 4, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 3, BookId = 8, PatronId = 3, ReservationDate = now.AddDays(-3), Status = ReservationStatus.Ready, ExpirationDate = now.AddDays(1), QueuePosition = 1 }
        };
        db.Reservations.AddRange(reservations);

        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 4, LoanId = 8, Amount = 3.50m, Reason = "Overdue: 14 days late", IssuedDate = now.AddDays(-5), Status = FineStatus.Unpaid },
            new() { Id = 2, PatronId = 2, LoanId = 7, Amount = 0.50m, Reason = "Overdue: 2 days late", IssuedDate = now.AddDays(-18), PaidDate = now.AddDays(-15), Status = FineStatus.Paid },
            new() { Id = 3, PatronId = 2, LoanId = 9, Amount = 1.00m, Reason = "Overdue: 4 days late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid }
        };
        db.Fines.AddRange(fines);

        await db.SaveChangesAsync(ct);
    }
}
