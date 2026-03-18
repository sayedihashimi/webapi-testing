using LibraryApi.Models;

namespace LibraryApi.Data;

public static class SeedData
{
    public static void Initialize(LibraryDbContext context)
    {
        if (context.Authors.Any()) return;

        var now = DateTime.UtcNow;

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, best known for Animal Farm and 1984.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom", CreatedAt = now },
            new() { Id = 2, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for his science fiction works.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = now },
            new() { Id = 3, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her commentary on the British landed gentry.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = now },
            new() { Id = 4, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and professor at the Hebrew University of Jerusalem.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel", CreatedAt = now },
            new() { Id = 5, FirstName = "Mary", LastName = "Shelley", Biography = "English novelist who wrote Frankenstein, considered the first science fiction novel.", BirthDate = new DateOnly(1797, 8, 30), Country = "United Kingdom", CreatedAt = now },
        };
        context.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Narrative literature created from the imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science and technology" },
            new() { Id = 3, Name = "History", Description = "Non-fiction accounts of past events" },
            new() { Id = 4, Name = "Science", Description = "Works exploring scientific concepts and discoveries" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of a person's life written by someone else" },
        };
        context.Categories.AddRange(categories);

        // Books — AvailableCopies will be set to match active/overdue loans below
        var books = new List<Book>
        {
            new() { Id = 1, Title = "1984", ISBN = "978-0451524935", Publisher = "Signet Classics", PublicationYear = 1949, Description = "A dystopian novel set in a totalitarian society.", PageCount = 328, Language = "English", TotalCopies = 3, AvailableCopies = 1, CreatedAt = now },
            new() { Id = 2, Title = "Animal Farm", ISBN = "978-0451526342", Publisher = "Signet Classics", PublicationYear = 1945, Description = "An allegorical novella about a group of farm animals.", PageCount = 112, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now },
            new() { Id = 3, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Spectra", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now },
            new() { Id = 4, Title = "I, Robot", ISBN = "978-0553294385", Publisher = "Bantam Spectra", PublicationYear = 1950, Description = "A collection of nine science fiction short stories.", PageCount = 224, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now },
            new() { Id = 5, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, Language = "English", TotalCopies = 4, AvailableCopies = 4, CreatedAt = now },
            new() { Id = 6, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about the Dashwood sisters.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now },
            new() { Id = 7, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A survey of the history of humankind.", PageCount = 464, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now },
            new() { Id = 8, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0062464316", Publisher = "Harper", PublicationYear = 2017, Description = "Explores the future of humanity.", PageCount = 464, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now },
            new() { Id = 9, Title = "Frankenstein", ISBN = "978-0486282114", Publisher = "Dover Publications", PublicationYear = 1818, Description = "The story of Victor Frankenstein and his creature.", PageCount = 166, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now },
            new() { Id = 10, Title = "The Last Man", ISBN = "978-0199552351", Publisher = "Oxford University Press", PublicationYear = 1826, Description = "A dystopian novel about the end of civilization.", PageCount = 480, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now },
            new() { Id = 11, Title = "Robot Dreams", ISBN = "978-0441731541", Publisher = "Ace Books", PublicationYear = 1986, Description = "A collection of robot-themed short stories.", PageCount = 368, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now },
            new() { Id = 12, Title = "21 Lessons for the 21st Century", ISBN = "978-0525512172", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "Examines current affairs and the future of humanity.", PageCount = 372, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now },
        };
        context.Books.AddRange(books);

        // BookAuthors
        context.BookAuthors.AddRange(
            new BookAuthor { BookId = 1, AuthorId = 1 },
            new BookAuthor { BookId = 2, AuthorId = 1 },
            new BookAuthor { BookId = 3, AuthorId = 2 },
            new BookAuthor { BookId = 4, AuthorId = 2 },
            new BookAuthor { BookId = 5, AuthorId = 3 },
            new BookAuthor { BookId = 6, AuthorId = 3 },
            new BookAuthor { BookId = 7, AuthorId = 4 },
            new BookAuthor { BookId = 8, AuthorId = 4 },
            new BookAuthor { BookId = 9, AuthorId = 5 },
            new BookAuthor { BookId = 10, AuthorId = 5 },
            new BookAuthor { BookId = 11, AuthorId = 2 },
            new BookAuthor { BookId = 12, AuthorId = 4 }
        );

        // BookCategories
        context.BookCategories.AddRange(
            new BookCategory { BookId = 1, CategoryId = 1 },   // 1984 → Fiction
            new BookCategory { BookId = 2, CategoryId = 1 },   // Animal Farm → Fiction
            new BookCategory { BookId = 3, CategoryId = 2 },   // Foundation → Sci-Fi
            new BookCategory { BookId = 4, CategoryId = 2 },   // I, Robot → Sci-Fi
            new BookCategory { BookId = 5, CategoryId = 1 },   // Pride → Fiction
            new BookCategory { BookId = 6, CategoryId = 1 },   // Sense → Fiction
            new BookCategory { BookId = 7, CategoryId = 3 },   // Sapiens → History
            new BookCategory { BookId = 7, CategoryId = 4 },   // Sapiens → Science
            new BookCategory { BookId = 8, CategoryId = 3 },   // Homo Deus → History
            new BookCategory { BookId = 8, CategoryId = 4 },   // Homo Deus → Science
            new BookCategory { BookId = 9, CategoryId = 1 },   // Frankenstein → Fiction
            new BookCategory { BookId = 9, CategoryId = 2 },   // Frankenstein → Sci-Fi
            new BookCategory { BookId = 10, CategoryId = 1 },  // Last Man → Fiction
            new BookCategory { BookId = 10, CategoryId = 2 },  // Last Man → Sci-Fi
            new BookCategory { BookId = 11, CategoryId = 2 },  // Robot Dreams → Sci-Fi
            new BookCategory { BookId = 12, CategoryId = 3 },  // 21 Lessons → History
            new BookCategory { BookId = 12, CategoryId = 4 }   // 21 Lessons → Science
        );

        // Patrons
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Maple Avenue", MembershipDate = new DateOnly(2022, 6, 1), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now },
            new() { Id = 3, FirstName = "Carol", LastName = "Davis", Email = "carol.davis@email.com", Phone = "555-0103", Address = "789 Pine Road", MembershipDate = new DateOnly(2024, 9, 1), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now },
            new() { Id = 4, FirstName = "David", LastName = "Wilson", Email = "david.wilson@email.com", Phone = "555-0104", Address = "321 Elm Street", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now },
            new() { Id = 5, FirstName = "Emma", LastName = "Brown", Email = "emma.brown@email.com", Phone = "555-0105", Address = "654 Cedar Lane", MembershipDate = new DateOnly(2022, 11, 10), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now },
            new() { Id = 6, FirstName = "Frank", LastName = "Miller", Email = "frank.miller@email.com", Phone = "555-0106", Address = "987 Birch Way", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Student, IsActive = false, CreatedAt = now },
        };
        context.Patrons.AddRange(patrons);

        // Loans
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-7), DueDate = now.AddDays(7), Status = LoanStatus.Active, CreatedAt = now },
            new() { Id = 2, BookId = 1, PatronId = 2, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active, CreatedAt = now },
            new() { Id = 3, BookId = 3, PatronId = 3, LoanDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = LoanStatus.Active, CreatedAt = now },
            new() { Id = 4, BookId = 9, PatronId = 2, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, CreatedAt = now },
            // Returned loans
            new() { Id = 5, BookId = 5, PatronId = 1, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-20), Status = LoanStatus.Returned, CreatedAt = now },
            new() { Id = 6, BookId = 7, PatronId = 4, LoanDate = now.AddDays(-45), DueDate = now.AddDays(-31), ReturnDate = now.AddDays(-30), Status = LoanStatus.Returned, CreatedAt = now },
            new() { Id = 7, BookId = 2, PatronId = 4, LoanDate = now.AddDays(-40), DueDate = now.AddDays(-26), ReturnDate = now.AddDays(-21), Status = LoanStatus.Returned, CreatedAt = now },
            new() { Id = 8, BookId = 8, PatronId = 5, LoanDate = now.AddDays(-60), DueDate = now.AddDays(-39), ReturnDate = now.AddDays(-2), Status = LoanStatus.Returned, CreatedAt = now },
            // Overdue loans
            new() { Id = 9, BookId = 4, PatronId = 5, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-9), Status = LoanStatus.Overdue, CreatedAt = now },
            new() { Id = 10, BookId = 7, PatronId = 3, LoanDate = now.AddDays(-14), DueDate = now.AddDays(-7), Status = LoanStatus.Overdue, CreatedAt = now },
        };
        context.Loans.AddRange(loans);

        // Reservations
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 4, PatronId = 1, ReservationDate = now.AddDays(-5), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now },
            new() { Id = 2, BookId = 1, PatronId = 4, ReservationDate = now.AddDays(-3), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now },
            new() { Id = 3, BookId = 4, PatronId = 3, ReservationDate = now.AddDays(-4), Status = ReservationStatus.Pending, QueuePosition = 2, CreatedAt = now },
        };
        context.Reservations.AddRange(reservations);

        // Fines
        var fines = new List<Fine>
        {
            // Loan 6: returned 1 day late ($0.25)
            new() { Id = 1, PatronId = 4, LoanId = 6, Amount = 0.25m, Reason = "Overdue return - 1 day late", IssuedDate = now.AddDays(-30), PaidDate = now.AddDays(-28), Status = FineStatus.Paid, CreatedAt = now },
            // Loan 7: returned 5 days late ($1.25)
            new() { Id = 2, PatronId = 4, LoanId = 7, Amount = 1.25m, Reason = "Overdue return - 5 days late", IssuedDate = now.AddDays(-21), Status = FineStatus.Unpaid, CreatedAt = now },
            // Loan 8: returned 37 days late ($9.25) — gives Patron 5 total $10 unpaid
            new() { Id = 3, PatronId = 5, LoanId = 8, Amount = 9.25m, Reason = "Overdue return - 37 days late", IssuedDate = now.AddDays(-2), Status = FineStatus.Unpaid, CreatedAt = now },
            // Additional small fine for Patron 5 to push total to ≥$10
            new() { Id = 4, PatronId = 5, LoanId = 8, Amount = 1.00m, Reason = "Processing fee for extended overdue", IssuedDate = now.AddDays(-2), Status = FineStatus.Unpaid, CreatedAt = now },
        };
        context.Fines.AddRange(fines);

        context.SaveChanges();
    }
}
