using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(LibraryDbContext db)
    {
        if (await db.Authors.AnyAsync())
        {
            return;
        }

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her six major novels.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom" },
            new() { Id = 2, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for science fiction.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States" },
            new() { Id = 3, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and professor at Hebrew University of Jerusalem.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel" },
            new() { Id = 4, FirstName = "Mary", LastName = "Shelley", Biography = "English novelist best known for Frankenstein.", BirthDate = new DateOnly(1797, 8, 30), Country = "United Kingdom" },
            new() { Id = 5, FirstName = "Neil", LastName = "deGrasse Tyson", Biography = "American astrophysicist, author, and science communicator.", BirthDate = new DateOnly(1958, 10, 5), Country = "United States" }
        };
        db.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science and technology" },
            new() { Id = 3, Name = "History", Description = "Non-fiction works about past events" },
            new() { Id = 4, Name = "Science", Description = "Non-fiction works about the natural world" },
            new() { Id = 5, Name = "Biography", Description = "Non-fiction account of a person's life" }
        };
        db.Categories.AddRange(categories);

        // Books
        var books = new List<Book>
        {
            new() { Id = 1, Title = "Pride and Prejudice", ISBN = "978-0-14-028329-7", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel following Elizabeth Bennet.", PageCount = 432, TotalCopies = 4, AvailableCopies = 3 },
            new() { Id = 2, Title = "Sense and Sensibility", ISBN = "978-0-14-143966-4", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "Story of the Dashwood sisters.", PageCount = 409, TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 3, Title = "Foundation", ISBN = "978-0-553-29335-7", Publisher = "Bantam Books", PublicationYear = 1951, Description = "A science fiction saga about the fall of a galactic empire.", PageCount = 244, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 4, Title = "I, Robot", ISBN = "978-0-553-29438-5", Publisher = "Bantam Books", PublicationYear = 1950, Description = "Collection of robot short stories.", PageCount = 224, TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 5, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0-06-231609-7", Publisher = "Harper", PublicationYear = 2011, Description = "Exploration of human history from the Stone Age.", PageCount = 443, TotalCopies = 5, AvailableCopies = 3 },
            new() { Id = 6, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0-06-246431-6", Publisher = "Harper", PublicationYear = 2015, Description = "Examines the future of humanity.", PageCount = 450, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 7, Title = "Frankenstein", ISBN = "978-0-14-143947-3", Publisher = "Penguin Classics", PublicationYear = 1818, Description = "The story of Victor Frankenstein's monstrous creation.", PageCount = 280, TotalCopies = 3, AvailableCopies = 3 },
            new() { Id = 8, Title = "Astrophysics for People in a Hurry", ISBN = "978-0-393-60939-4", Publisher = "W.W. Norton", PublicationYear = 2017, Description = "An accessible guide to the universe.", PageCount = 222, TotalCopies = 4, AvailableCopies = 3 },
            new() { Id = 9, Title = "The End of Everything", ISBN = "978-1-982-10380-7", Publisher = "Scribner", PublicationYear = 2020, Description = "How the universe will end according to astrophysics.", PageCount = 240, TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 10, Title = "21 Lessons for the 21st Century", ISBN = "978-0-525-51217-2", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "Explores current affairs and the near future of humanity.", PageCount = 372, TotalCopies = 3, AvailableCopies = 3 },
            new() { Id = 11, Title = "The Caves of Steel", ISBN = "978-0-553-29340-1", Publisher = "Bantam Books", PublicationYear = 1954, Description = "A detective story set in a future Earth.", PageCount = 206, TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 12, Title = "Emma", ISBN = "978-0-14-143958-9", Publisher = "Penguin Classics", PublicationYear = 1815, Description = "Follows the life of the charming but misguided Emma Woodhouse.", PageCount = 474, TotalCopies = 2, AvailableCopies = 2 }
        };
        db.Books.AddRange(books);

        // BookAuthors
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },
            new() { BookId = 2, AuthorId = 1 },
            new() { BookId = 12, AuthorId = 1 },
            new() { BookId = 3, AuthorId = 2 },
            new() { BookId = 4, AuthorId = 2 },
            new() { BookId = 11, AuthorId = 2 },
            new() { BookId = 5, AuthorId = 3 },
            new() { BookId = 6, AuthorId = 3 },
            new() { BookId = 10, AuthorId = 3 },
            new() { BookId = 7, AuthorId = 4 },
            new() { BookId = 8, AuthorId = 5 },
            new() { BookId = 9, AuthorId = 5 }
        };
        db.BookAuthors.AddRange(bookAuthors);

        // BookCategories
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 },
            new() { BookId = 2, CategoryId = 1 },
            new() { BookId = 12, CategoryId = 1 },
            new() { BookId = 3, CategoryId = 2 },
            new() { BookId = 4, CategoryId = 2 },
            new() { BookId = 11, CategoryId = 2 },
            new() { BookId = 7, CategoryId = 1 },
            new() { BookId = 7, CategoryId = 2 },
            new() { BookId = 5, CategoryId = 3 },
            new() { BookId = 6, CategoryId = 3 },
            new() { BookId = 10, CategoryId = 3 },
            new() { BookId = 8, CategoryId = 4 },
            new() { BookId = 9, CategoryId = 4 },
            new() { BookId = 5, CategoryId = 5 }
        };
        db.BookCategories.AddRange(bookCategories);

        // Patrons
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", MembershipType = MembershipType.Premium, MembershipDate = new DateOnly(2023, 1, 15) },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Maple Avenue", MembershipType = MembershipType.Standard, MembershipDate = new DateOnly(2023, 3, 20) },
            new() { Id = 3, FirstName = "Carol", LastName = "Davis", Email = "carol.davis@email.com", Phone = "555-0103", Address = "789 Pine Road", MembershipType = MembershipType.Student, MembershipDate = new DateOnly(2024, 9, 1) },
            new() { Id = 4, FirstName = "David", LastName = "Wilson", Email = "david.wilson@email.com", Phone = "555-0104", Address = "321 Elm Street", MembershipType = MembershipType.Standard, MembershipDate = new DateOnly(2023, 6, 10) },
            new() { Id = 5, FirstName = "Eva", LastName = "Martinez", Email = "eva.martinez@email.com", Phone = "555-0105", Address = "654 Birch Lane", MembershipType = MembershipType.Premium, MembershipDate = new DateOnly(2024, 1, 5) },
            new() { Id = 6, FirstName = "Frank", LastName = "Brown", Email = "frank.brown@email.com", Phone = "555-0106", Address = "987 Cedar Court", MembershipType = MembershipType.Standard, MembershipDate = new DateOnly(2023, 8, 22), IsActive = false }
        };
        db.Patrons.AddRange(patrons);

        var now = DateTime.UtcNow;

        // Loans (8 loans: Active, Returned, Overdue)
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active },
            new() { Id = 2, BookId = 3, PatronId = 2, LoanDate = now.AddDays(-10), DueDate = now.AddDays(4), Status = LoanStatus.Active },
            new() { Id = 3, BookId = 4, PatronId = 3, LoanDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = LoanStatus.Active },
            new() { Id = 4, BookId = 5, PatronId = 1, LoanDate = now.AddDays(-7), DueDate = now.AddDays(14), Status = LoanStatus.Active },
            // Returned loans
            new() { Id = 5, BookId = 8, PatronId = 2, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-18), Status = LoanStatus.Returned },
            new() { Id = 6, BookId = 5, PatronId = 5, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-4), ReturnDate = now.AddDays(-5), Status = LoanStatus.Returned },
            // Overdue loans
            new() { Id = 7, BookId = 5, PatronId = 4, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue },
            new() { Id = 8, BookId = 8, PatronId = 4, LoanDate = now.AddDays(-18), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue }
        };
        db.Loans.AddRange(loans);

        // Reservations
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 5, PatronId = 2, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 2, BookId = 5, PatronId = 3, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 2 },
            new() { Id = 3, BookId = 4, PatronId = 5, ReservationDate = now.AddDays(-3), Status = ReservationStatus.Ready, ExpirationDate = now.AddDays(2), QueuePosition = 1 }
        };
        db.Reservations.AddRange(reservations);

        // Fines
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 4, LoanId = 7, Amount = 1.50m, Reason = "Overdue: 6 days late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid },
            new() { Id = 2, PatronId = 4, LoanId = 8, Amount = 1.00m, Reason = "Overdue: 4 days late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid },
            new() { Id = 3, PatronId = 2, LoanId = 5, Amount = 0.50m, Reason = "Overdue: 2 days late", IssuedDate = now.AddDays(-18), PaidDate = now.AddDays(-17), Status = FineStatus.Paid }
        };
        db.Fines.AddRange(fines);

        await db.SaveChangesAsync();
    }
}
