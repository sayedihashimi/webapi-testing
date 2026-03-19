using LibraryApi.Models;
using LibraryApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(LibraryDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Authors.AnyAsync())
            return;

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist known for works on social injustice.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom" },
            new() { Id = 2, FirstName = "Harper", LastName = "Lee", Biography = "American novelist best known for To Kill a Mockingbird.", BirthDate = new DateOnly(1926, 4, 28), Country = "United States" },
            new() { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "Prolific science fiction author and biochemistry professor.", BirthDate = new DateOnly(1920, 1, 2), Country = "Russia" },
            new() { Id = 4, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her wit and social commentary.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom" },
            new() { Id = 5, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and author of popular science bestsellers.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel" },
            new() { Id = 6, FirstName = "Walter", LastName = "Isaacson", Biography = "American author and journalist known for biographies of great innovators.", BirthDate = new DateOnly(1952, 5, 20), Country = "United States" },
        };
        context.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Novels and fictional works" },
            new() { Id = 2, Name = "Science Fiction", Description = "Science fiction and speculative fiction" },
            new() { Id = 3, Name = "History", Description = "Historical non-fiction works" },
            new() { Id = 4, Name = "Science", Description = "Scientific works and popular science" },
            new() { Id = 5, Name = "Biography", Description = "Biographical and autobiographical works" },
            new() { Id = 6, Name = "Classic Literature", Description = "Classic works of literature" },
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Books
        var books = new List<Book>
        {
            new() { Id = 1, Title = "1984", ISBN = "978-0451524935", Publisher = "Signet Classic", PublicationYear = 1949, Description = "A dystopian novel set in a totalitarian society.", PageCount = 328, TotalCopies = 4, AvailableCopies = 2 },
            new() { Id = 2, Title = "Animal Farm", ISBN = "978-0451526342", Publisher = "Signet Classic", PublicationYear = 1945, Description = "A satirical allegory about Soviet totalitarianism.", PageCount = 112, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 3, Title = "To Kill a Mockingbird", ISBN = "978-0061120084", Publisher = "Harper Perennial", PublicationYear = 1960, Description = "A novel about racial injustice in the American South.", PageCount = 336, TotalCopies = 5, AvailableCopies = 3 },
            new() { Id = 4, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in the Foundation series.", PageCount = 244, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 5, Title = "I, Robot", ISBN = "978-0553294385", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of interconnected short stories about robots.", PageCount = 224, TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 6, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, TotalCopies = 4, AvailableCopies = 3 },
            new() { Id = 7, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about two sisters and their romantic entanglements.", PageCount = 409, TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 8, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A narrative of humanity's creation and evolution.", PageCount = 464, TotalCopies = 3, AvailableCopies = 1 },
            new() { Id = 9, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0062464316", Publisher = "Harper", PublicationYear = 2017, Description = "An exploration of humanity's future.", PageCount = 464, TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 10, Title = "Steve Jobs", ISBN = "978-1451648539", Publisher = "Simon & Schuster", PublicationYear = 2011, Description = "The biography of Apple co-founder Steve Jobs.", PageCount = 656, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 11, Title = "21 Lessons for the 21st Century", ISBN = "978-0525512172", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "Addressing today's most urgent issues.", PageCount = 372, TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 12, Title = "Einstein: His Life and Universe", ISBN = "978-0743264747", Publisher = "Simon & Schuster", PublicationYear = 2007, Description = "A biography of Albert Einstein.", PageCount = 704, TotalCopies = 2, AvailableCopies = 1 },
        };
        context.Books.AddRange(books);
        await context.SaveChangesAsync();

        // BookAuthors
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 }, // 1984 - Orwell
            new() { BookId = 2, AuthorId = 1 }, // Animal Farm - Orwell
            new() { BookId = 3, AuthorId = 2 }, // Mockingbird - Lee
            new() { BookId = 4, AuthorId = 3 }, // Foundation - Asimov
            new() { BookId = 5, AuthorId = 3 }, // I,Robot - Asimov
            new() { BookId = 6, AuthorId = 4 }, // Pride - Austen
            new() { BookId = 7, AuthorId = 4 }, // Sense - Austen
            new() { BookId = 8, AuthorId = 5 }, // Sapiens - Harari
            new() { BookId = 9, AuthorId = 5 }, // Homo Deus - Harari
            new() { BookId = 10, AuthorId = 6 }, // Steve Jobs - Isaacson
            new() { BookId = 11, AuthorId = 5 }, // 21 Lessons - Harari
            new() { BookId = 12, AuthorId = 6 }, // Einstein - Isaacson
        };
        context.BookAuthors.AddRange(bookAuthors);

        // BookCategories
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 }, new() { BookId = 1, CategoryId = 6 }, // 1984 - Fiction, Classic
            new() { BookId = 2, CategoryId = 1 }, new() { BookId = 2, CategoryId = 6 }, // Animal Farm - Fiction, Classic
            new() { BookId = 3, CategoryId = 1 }, new() { BookId = 3, CategoryId = 6 }, // Mockingbird - Fiction, Classic
            new() { BookId = 4, CategoryId = 2 }, // Foundation - Sci-Fi
            new() { BookId = 5, CategoryId = 2 }, // I,Robot - Sci-Fi
            new() { BookId = 6, CategoryId = 1 }, new() { BookId = 6, CategoryId = 6 }, // Pride - Fiction, Classic
            new() { BookId = 7, CategoryId = 1 }, new() { BookId = 7, CategoryId = 6 }, // Sense - Fiction, Classic
            new() { BookId = 8, CategoryId = 3 }, new() { BookId = 8, CategoryId = 4 }, // Sapiens - History, Science
            new() { BookId = 9, CategoryId = 4 }, // Homo Deus - Science
            new() { BookId = 10, CategoryId = 5 }, // Steve Jobs - Biography
            new() { BookId = 11, CategoryId = 3 }, new() { BookId = 11, CategoryId = 4 }, // 21 Lessons - History, Science
            new() { BookId = 12, CategoryId = 5 }, // Einstein - Biography
        };
        context.BookCategories.AddRange(bookCategories);
        await context.SaveChangesAsync();

        // Patrons
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak St", MembershipType = MembershipType.Premium, MembershipDate = new DateOnly(2023, 1, 15) },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Elm St", MembershipType = MembershipType.Standard, MembershipDate = new DateOnly(2023, 3, 20) },
            new() { Id = 3, FirstName = "Charlie", LastName = "Brown", Email = "charlie.brown@email.com", Phone = "555-0103", Address = "789 Pine Ave", MembershipType = MembershipType.Student, MembershipDate = new DateOnly(2023, 9, 1) },
            new() { Id = 4, FirstName = "Diana", LastName = "Prince", Email = "diana.prince@email.com", Phone = "555-0104", Address = "321 Maple Dr", MembershipType = MembershipType.Premium, MembershipDate = new DateOnly(2022, 6, 10) },
            new() { Id = 5, FirstName = "Edward", LastName = "Norton", Email = "edward.norton@email.com", Phone = "555-0105", Address = "654 Birch Ln", MembershipType = MembershipType.Standard, MembershipDate = new DateOnly(2024, 1, 5) },
            new() { Id = 6, FirstName = "Fiona", LastName = "Green", Email = "fiona.green@email.com", Phone = "555-0106", Address = "987 Cedar Ct", MembershipType = MembershipType.Student, IsActive = false, MembershipDate = new DateOnly(2023, 2, 14) },
        };
        context.Patrons.AddRange(patrons);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;

        // Loans: mix of Active, Returned, Overdue
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active },
            new() { Id = 2, BookId = 3, PatronId = 1, LoanDate = now.AddDays(-3), DueDate = now.AddDays(18), Status = LoanStatus.Active },
            new() { Id = 3, BookId = 4, PatronId = 2, LoanDate = now.AddDays(-10), DueDate = now.AddDays(4), Status = LoanStatus.Active },
            new() { Id = 4, BookId = 8, PatronId = 3, LoanDate = now.AddDays(-5), DueDate = now.AddDays(2), Status = LoanStatus.Active },
            // Returned loans
            new() { Id = 5, BookId = 5, PatronId = 4, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-9), ReturnDate = now.AddDays(-10), Status = LoanStatus.Returned },
            new() { Id = 6, BookId = 6, PatronId = 2, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), ReturnDate = now.AddDays(-7), Status = LoanStatus.Returned },
            // Overdue loans
            new() { Id = 7, BookId = 9, PatronId = 4, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue },
            new() { Id = 8, BookId = 12, PatronId = 5, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue },
            new() { Id = 9, BookId = 1, PatronId = 2, LoanDate = now.AddDays(-18), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue },
            new() { Id = 10, BookId = 3, PatronId = 4, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-4), ReturnDate = now.AddDays(-1), Status = LoanStatus.Returned, RenewalCount = 1 },
        };
        context.Loans.AddRange(loans);
        await context.SaveChangesAsync();

        // Reservations
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 1, PatronId = 3, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 2, BookId = 9, PatronId = 2, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 3, BookId = 8, PatronId = 5, ReservationDate = now.AddDays(-3), Status = ReservationStatus.Ready, QueuePosition = 1, ExpirationDate = now.AddDays(2) },
        };
        context.Reservations.AddRange(reservations);
        await context.SaveChangesAsync();

        // Fines
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 4, LoanId = 5, Amount = 2.75m, Reason = "Overdue return - 11 days late", IssuedDate = now.AddDays(-10), Status = FineStatus.Paid, PaidDate = now.AddDays(-8) },
            new() { Id = 2, PatronId = 4, LoanId = 7, Amount = 1.00m, Reason = "Overdue - 4 days late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid },
            new() { Id = 3, PatronId = 5, LoanId = 8, Amount = 1.50m, Reason = "Overdue - 6 days late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid },
            new() { Id = 4, PatronId = 2, LoanId = 9, Amount = 1.00m, Reason = "Overdue - 4 days late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid },
        };
        context.Fines.AddRange(fines);
        await context.SaveChangesAsync();
    }
}
