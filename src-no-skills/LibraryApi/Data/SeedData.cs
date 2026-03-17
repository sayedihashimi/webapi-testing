using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class SeedData
{
    public static void Initialize(LibraryDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Authors.Any()) return;

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her commentary on the British landed gentry.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom" },
            new() { Id = 2, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for science fiction and popular science.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States" },
            new() { Id = 3, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and professor at the Hebrew University of Jerusalem.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel" },
            new() { Id = 4, FirstName = "Marie", LastName = "Curie", Biography = "Polish-French physicist and chemist who conducted pioneering research on radioactivity.", BirthDate = new DateOnly(1867, 11, 7), Country = "Poland" },
            new() { Id = 5, FirstName = "Gabriel", LastName = "García Márquez", Biography = "Colombian novelist and Nobel Prize laureate, known for magical realism.", BirthDate = new DateOnly(1927, 3, 6), Country = "Colombia" }
        };
        context.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science and technology" },
            new() { Id = 3, Name = "History", Description = "Non-fiction accounts of past events" },
            new() { Id = 4, Name = "Science", Description = "Books about scientific discoveries and principles" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of real people's lives" }
        };
        context.Categories.AddRange(categories);

        // Books (12 books)
        var books = new List<Book>
        {
            new() { Id = 1, Title = "Pride and Prejudice", ISBN = "978-0-14-028329-7", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel about the Bennet family.", PageCount = 432, Language = "English", TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 2, Title = "Sense and Sensibility", ISBN = "978-0-14-143966-4", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "The story of the Dashwood sisters.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 3, Title = "Foundation", ISBN = "978-0-553-29335-7", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 4, AvailableCopies = 3 },
            new() { Id = 4, Title = "I, Robot", ISBN = "978-0-553-29438-5", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of robot short stories.", PageCount = 224, Language = "English", TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 5, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0-06-231609-7", Publisher = "Harper", PublicationYear = 2014, Description = "A narrative history of humankind.", PageCount = 464, Language = "English", TotalCopies = 5, AvailableCopies = 3 },
            new() { Id = 6, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0-06-246431-6", Publisher = "Harper", PublicationYear = 2017, Description = "An exploration of humanity's future.", PageCount = 464, Language = "English", TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 7, Title = "Radioactive: Marie & Pierre Curie", ISBN = "978-0-06-220143-1", Publisher = "HarperCollins", PublicationYear = 2010, Description = "A visual biography of Marie and Pierre Curie.", PageCount = 208, Language = "English", TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 8, Title = "One Hundred Years of Solitude", ISBN = "978-0-06-088328-7", Publisher = "Harper Perennial", PublicationYear = 1967, Description = "The multi-generational story of the Buendía family.", PageCount = 417, Language = "English", TotalCopies = 4, AvailableCopies = 3 },
            new() { Id = 9, Title = "Love in the Time of Cholera", ISBN = "978-0-14-024990-3", Publisher = "Penguin Books", PublicationYear = 1985, Description = "A love story spanning fifty years.", PageCount = 348, Language = "English", TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 10, Title = "The Caves of Steel", ISBN = "978-0-553-29340-1", Publisher = "Bantam Books", PublicationYear = 1954, Description = "A science fiction detective novel.", PageCount = 206, Language = "English", TotalCopies = 3, AvailableCopies = 3 },
            new() { Id = 11, Title = "21 Lessons for the 21st Century", ISBN = "978-0-525-51217-2", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "Exploring the challenges of the modern world.", PageCount = 372, Language = "English", TotalCopies = 3, AvailableCopies = 3 },
            new() { Id = 12, Title = "Emma", ISBN = "978-0-14-143958-9", Publisher = "Penguin Classics", PublicationYear = 1815, Description = "A comedy of manners about youthful self-importance.", PageCount = 474, Language = "English", TotalCopies = 2, AvailableCopies = 2 }
        };
        context.Books.AddRange(books);

        // BookAuthors
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },
            new() { BookId = 2, AuthorId = 1 },
            new() { BookId = 12, AuthorId = 1 },
            new() { BookId = 3, AuthorId = 2 },
            new() { BookId = 4, AuthorId = 2 },
            new() { BookId = 10, AuthorId = 2 },
            new() { BookId = 5, AuthorId = 3 },
            new() { BookId = 6, AuthorId = 3 },
            new() { BookId = 11, AuthorId = 3 },
            new() { BookId = 7, AuthorId = 4 },
            new() { BookId = 8, AuthorId = 5 },
            new() { BookId = 9, AuthorId = 5 }
        };
        context.BookAuthors.AddRange(bookAuthors);

        // BookCategories
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 },
            new() { BookId = 2, CategoryId = 1 },
            new() { BookId = 12, CategoryId = 1 },
            new() { BookId = 3, CategoryId = 2 },
            new() { BookId = 4, CategoryId = 2 },
            new() { BookId = 10, CategoryId = 2 },
            new() { BookId = 5, CategoryId = 3 },
            new() { BookId = 6, CategoryId = 3 },
            new() { BookId = 11, CategoryId = 3 },
            new() { BookId = 7, CategoryId = 5 },
            new() { BookId = 7, CategoryId = 4 },
            new() { BookId = 8, CategoryId = 1 },
            new() { BookId = 9, CategoryId = 1 }
        };
        context.BookCategories.AddRange(bookCategories);

        // Patrons
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Main St", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true },
            new() { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.w@email.com", Phone = "555-0103", Address = "789 Pine Rd", MembershipDate = new DateOnly(2023, 6, 10), MembershipType = MembershipType.Student, IsActive = true },
            new() { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.b@email.com", Phone = "555-0104", Address = "321 Elm St", MembershipDate = new DateOnly(2023, 9, 1), MembershipType = MembershipType.Standard, IsActive = true },
            new() { Id = 5, FirstName = "Eva", LastName = "Martinez", Email = "eva.m@email.com", Phone = "555-0105", Address = "654 Birch Ln", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Premium, IsActive = true },
            new() { Id = 6, FirstName = "Frank", LastName = "Taylor", Email = "frank.t@email.com", Phone = "555-0106", Address = "987 Cedar Dr", MembershipDate = new DateOnly(2024, 2, 14), MembershipType = MembershipType.Student, IsActive = false }
        };
        context.Patrons.AddRange(patrons);

        context.SaveChanges();

        var now = DateTime.UtcNow;

        // Loans: 8 total (some Active, Returned, Overdue)
        // Book 1 (3 copies, 2 avail) = 1 active loan
        // Book 3 (4 copies, 3 avail) = 1 active loan
        // Book 4 (2 copies, 1 avail) = 1 active loan
        // Book 5 (5 copies, 3 avail) = 2 active loans (but one overdue)
        // Book 8 (4 copies, 3 avail) = 1 active loan (overdue)
        // Book 9 (2 copies, 1 avail) = 1 active loan
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active },
            new() { Id = 2, BookId = 3, PatronId = 2, LoanDate = now.AddDays(-10), DueDate = now.AddDays(4), Status = LoanStatus.Active },
            new() { Id = 3, BookId = 4, PatronId = 3, LoanDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = LoanStatus.Active },
            new() { Id = 4, BookId = 9, PatronId = 5, LoanDate = now.AddDays(-7), DueDate = now.AddDays(14), Status = LoanStatus.Active },
            // Returned loans
            new() { Id = 5, BookId = 5, PatronId = 1, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-9), ReturnDate = now.AddDays(-12), Status = LoanStatus.Returned },
            new() { Id = 6, BookId = 8, PatronId = 4, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-11), ReturnDate = now.AddDays(-8), Status = LoanStatus.Returned },
            // Overdue loans
            new() { Id = 7, BookId = 5, PatronId = 2, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue },
            new() { Id = 8, BookId = 8, PatronId = 4, LoanDate = now.AddDays(-18), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue }
        };
        context.Loans.AddRange(loans);

        // Reservations
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 4, PatronId = 2, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 2, BookId = 9, PatronId = 4, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 3, BookId = 1, PatronId = 3, ReservationDate = now.AddDays(-1), ExpirationDate = now.AddDays(2), Status = ReservationStatus.Ready, QueuePosition = 1 }
        };
        context.Reservations.AddRange(reservations);

        // Fines (matching overdue days)
        // Loan 6: returned 3 days late → $0.75
        // Loan 7: overdue by 6 days → $1.50
        // Loan 8: overdue by 4 days → $1.00
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 4, LoanId = 6, Amount = 0.75m, Reason = "Overdue return - 3 days late", IssuedDate = now.AddDays(-8), PaidDate = now.AddDays(-7), Status = FineStatus.Paid },
            new() { Id = 2, PatronId = 2, LoanId = 7, Amount = 1.50m, Reason = "Overdue - 6 days past due date", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid },
            new() { Id = 3, PatronId = 4, LoanId = 8, Amount = 1.00m, Reason = "Overdue - 4 days past due date", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid }
        };
        context.Fines.AddRange(fines);

        context.SaveChanges();
    }
}
