using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options)
    : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<BookCategory> BookCategories => Set<BookCategory>();
    public DbSet<Patron> Patrons => Set<Patron>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Fine> Fines => Set<Fine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Author
        modelBuilder.Entity<Author>(e =>
        {
            e.Property(a => a.FirstName).HasMaxLength(100).IsRequired();
            e.Property(a => a.LastName).HasMaxLength(100).IsRequired();
            e.Property(a => a.Biography).HasMaxLength(2000);
            e.Property(a => a.Country).HasMaxLength(100);
        });

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Description).HasMaxLength(500);
        });

        // Book
        modelBuilder.Entity<Book>(e =>
        {
            e.Property(b => b.Title).HasMaxLength(300).IsRequired();
            e.Property(b => b.ISBN).HasMaxLength(20).IsRequired();
            e.HasIndex(b => b.ISBN).IsUnique();
            e.Property(b => b.Publisher).HasMaxLength(200);
            e.Property(b => b.Description).HasMaxLength(2000);
            e.Property(b => b.Language).HasMaxLength(50).HasDefaultValue("English");
        });

        // BookAuthor join
        modelBuilder.Entity<BookAuthor>(e =>
        {
            e.HasKey(ba => new { ba.BookId, ba.AuthorId });
            e.HasOne(ba => ba.Book).WithMany(b => b.BookAuthors).HasForeignKey(ba => ba.BookId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ba => ba.Author).WithMany(a => a.BookAuthors).HasForeignKey(ba => ba.AuthorId).OnDelete(DeleteBehavior.Cascade);
        });

        // BookCategory join
        modelBuilder.Entity<BookCategory>(e =>
        {
            e.HasKey(bc => new { bc.BookId, bc.CategoryId });
            e.HasOne(bc => bc.Book).WithMany(b => b.BookCategories).HasForeignKey(bc => bc.BookId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(bc => bc.Category).WithMany(c => c.BookCategories).HasForeignKey(bc => bc.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        // Patron
        modelBuilder.Entity<Patron>(e =>
        {
            e.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            e.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            e.Property(p => p.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(p => p.Email).IsUnique();
            e.Property(p => p.Phone).HasMaxLength(20);
            e.Property(p => p.Address).HasMaxLength(500);
            e.Property(p => p.MembershipType).HasConversion<string>().HasMaxLength(20);
        });

        // Loan
        modelBuilder.Entity<Loan>(e =>
        {
            e.HasOne(l => l.Book).WithMany(b => b.Loans).HasForeignKey(l => l.BookId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(l => l.Patron).WithMany(p => p.Loans).HasForeignKey(l => l.PatronId).OnDelete(DeleteBehavior.Restrict);
            e.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        });

        // Reservation
        modelBuilder.Entity<Reservation>(e =>
        {
            e.HasOne(r => r.Book).WithMany(b => b.Reservations).HasForeignKey(r => r.BookId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Patron).WithMany(p => p.Reservations).HasForeignKey(r => r.PatronId).OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        });

        // Fine
        modelBuilder.Entity<Fine>(e =>
        {
            e.HasOne(f => f.Patron).WithMany(p => p.Fines).HasForeignKey(f => f.PatronId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.Loan).WithMany(l => l.Fines).HasForeignKey(f => f.LoanId).OnDelete(DeleteBehavior.Restrict);
            e.Property(f => f.Amount).HasColumnType("decimal(10,2)");
            e.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(f => f.Reason).HasMaxLength(500).IsRequired();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Authors
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, known for his sharp criticism of political oppression.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 2, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known primarily for her six major novels about the British landed gentry.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for his works of science fiction.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 4, FirstName = "Toni", LastName = "Morrison", Biography = "American novelist and Nobel Prize in Literature laureate.", BirthDate = new DateOnly(1931, 2, 18), Country = "United States", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 5, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and author of Sapiens and Homo Deus.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 6, FirstName = "Margaret", LastName = "Atwood", Biography = "Canadian poet, novelist, and environmental activist.", BirthDate = new DateOnly(1939, 11, 18), Country = "Canada", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new Category { Id = 2, Name = "Science Fiction", Description = "Fiction based on imagined future scientific or technological advances" },
            new Category { Id = 3, Name = "History", Description = "Books about past events and civilizations" },
            new Category { Id = 4, Name = "Science", Description = "Books about the natural world and scientific discoveries" },
            new Category { Id = 5, Name = "Biography", Description = "Accounts of a person's life written by someone else" },
            new Category { Id = 6, Name = "Classic Literature", Description = "Timeless literary masterpieces" }
        );

        // Books (12+)
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "1984", ISBN = "978-0451524935", Publisher = "Signet Classics", PublicationYear = 1949, Description = "A dystopian novel about totalitarianism.", PageCount = 328, Language = "English", TotalCopies = 5, AvailableCopies = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 2, Title = "Animal Farm", ISBN = "978-0451526342", Publisher = "Signet Classics", PublicationYear = 1945, Description = "An allegorical novella about Soviet totalitarianism.", PageCount = 112, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 3, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, Language = "English", TotalCopies = 4, AvailableCopies = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 4, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about the Dashwood sisters.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 5, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 3, AvailableCopies = 1, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 6, Title = "I, Robot", ISBN = "978-0553294385", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of robot-related short stories.", PageCount = 224, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 7, Title = "Beloved", ISBN = "978-1400033416", Publisher = "Vintage", PublicationYear = 1987, Description = "A novel about the aftermath of slavery.", PageCount = 324, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 8, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A book exploring the history of the human species.", PageCount = 464, Language = "English", TotalCopies = 4, AvailableCopies = 2, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 9, Title = "Homo Deus", ISBN = "978-0062464316", Publisher = "Harper", PublicationYear = 2017, Description = "A book about the future of humanity.", PageCount = 464, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 10, Title = "The Handmaid's Tale", ISBN = "978-0385490818", Publisher = "Anchor Books", PublicationYear = 1985, Description = "A dystopian novel set in a totalitarian society.", PageCount = 311, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 11, Title = "The Testaments", ISBN = "978-0385543781", Publisher = "Anchor Books", PublicationYear = 2019, Description = "Sequel to The Handmaid's Tale.", PageCount = 432, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 12, Title = "Song of Solomon", ISBN = "978-1400033423", Publisher = "Vintage", PublicationYear = 1977, Description = "A novel about an African-American man's search for identity.", PageCount = 337, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // BookAuthor relationships
        modelBuilder.Entity<BookAuthor>().HasData(
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
            new BookAuthor { BookId = 12, AuthorId = 4 }
        );

        // BookCategory relationships
        modelBuilder.Entity<BookCategory>().HasData(
            new BookCategory { BookId = 1, CategoryId = 1 },
            new BookCategory { BookId = 1, CategoryId = 2 },
            new BookCategory { BookId = 1, CategoryId = 6 },
            new BookCategory { BookId = 2, CategoryId = 1 },
            new BookCategory { BookId = 2, CategoryId = 6 },
            new BookCategory { BookId = 3, CategoryId = 1 },
            new BookCategory { BookId = 3, CategoryId = 6 },
            new BookCategory { BookId = 4, CategoryId = 1 },
            new BookCategory { BookId = 4, CategoryId = 6 },
            new BookCategory { BookId = 5, CategoryId = 2 },
            new BookCategory { BookId = 6, CategoryId = 2 },
            new BookCategory { BookId = 7, CategoryId = 1 },
            new BookCategory { BookId = 8, CategoryId = 3 },
            new BookCategory { BookId = 8, CategoryId = 4 },
            new BookCategory { BookId = 9, CategoryId = 3 },
            new BookCategory { BookId = 9, CategoryId = 4 },
            new BookCategory { BookId = 10, CategoryId = 1 },
            new BookCategory { BookId = 10, CategoryId = 2 },
            new BookCategory { BookId = 11, CategoryId = 1 },
            new BookCategory { BookId = 11, CategoryId = 2 },
            new BookCategory { BookId = 12, CategoryId = 1 }
        );

        // Patrons (6+)
        modelBuilder.Entity<Patron>().HasData(
            new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Maple St", MembershipDate = new DateOnly(2023, 3, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Patron { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave", MembershipDate = new DateOnly(2023, 6, 1), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Patron { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Rd", MembershipDate = new DateOnly(2024, 1, 10), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Patron { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Elm Blvd", MembershipDate = new DateOnly(2023, 9, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Patron { Id = 5, FirstName = "Eva", LastName = "Davis", Email = "eva.davis@email.com", Phone = "555-0105", Address = "654 Birch Ln", MembershipDate = new DateOnly(2022, 12, 1), MembershipType = MembershipType.Premium, IsActive = false, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Patron { Id = 6, FirstName = "Frank", LastName = "Garcia", Email = "frank.garcia@email.com", Phone = "555-0106", Address = "987 Cedar Ct", MembershipDate = new DateOnly(2024, 2, 14), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Patron { Id = 7, FirstName = "Grace", LastName = "Miller", Email = "grace.miller@email.com", Phone = "555-0107", Address = "147 Walnut Dr", MembershipDate = new DateOnly(2023, 5, 5), MembershipType = MembershipType.Standard, IsActive = false, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Loans (8+): Active, Returned, Overdue — matching AvailableCopies
        // Active loans: Book1(2 active), Book2(1 active), Book3(1 active), Book5(2 active), Book6(1 active), Book7(1 active), Book8(2 active), Book10(1 active), Book12(1 active)
        var now = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Loan>().HasData(
            // Active loan - Alice borrowed 1984
            new Loan { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-10) },
            // Active loan - Alice borrowed Foundation
            new Loan { Id = 2, BookId = 5, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-5) },
            // Active loan - Bob borrowed Animal Farm
            new Loan { Id = 3, BookId = 2, PatronId = 2, LoanDate = now.AddDays(-7), DueDate = now.AddDays(7), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-7) },
            // Overdue loan - Bob borrowed 1984 (copy 2) - past due
            new Loan { Id = 4, BookId = 1, PatronId = 2, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-20) },
            // Active loan - Carol borrowed Foundation (copy 2)
            new Loan { Id = 5, BookId = 5, PatronId = 3, LoanDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-3) },
            // Returned loan - David borrowed Pride and Prejudice
            new Loan { Id = 6, BookId = 3, PatronId = 4, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-17), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-30) },
            // Active loan - Carol borrowed I, Robot
            new Loan { Id = 7, BookId = 6, PatronId = 3, LoanDate = now.AddDays(-2), DueDate = now.AddDays(5), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-2) },
            // Active loan - David borrowed Beloved
            new Loan { Id = 8, BookId = 7, PatronId = 4, LoanDate = now.AddDays(-8), DueDate = now.AddDays(6), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-8) },
            // Active loan - Alice borrowed Sapiens
            new Loan { Id = 9, BookId = 8, PatronId = 1, LoanDate = now.AddDays(-4), DueDate = now.AddDays(17), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-4) },
            // Returned loan (late) - Frank borrowed Sapiens
            new Loan { Id = 10, BookId = 8, PatronId = 6, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-18), ReturnDate = now.AddDays(-15), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-25) },
            // Active loan - David borrowed Handmaid's Tale
            new Loan { Id = 11, BookId = 10, PatronId = 4, LoanDate = now.AddDays(-6), DueDate = now.AddDays(8), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-6) },
            // Active loan - Frank borrowed Song of Solomon
            new Loan { Id = 12, BookId = 12, PatronId = 6, LoanDate = now.AddDays(-1), DueDate = now.AddDays(6), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-1) },
            // Active loan - Alice borrowed Pride and Prejudice
            new Loan { Id = 13, BookId = 3, PatronId = 1, LoanDate = now.AddDays(-2), DueDate = now.AddDays(19), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-2) }
        );

        // Reservations
        modelBuilder.Entity<Reservation>().HasData(
            // Pending reservation - David wants Foundation (no copies available after 2 active loans)
            new Reservation { Id = 1, BookId = 5, PatronId = 4, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-1) },
            // Ready reservation - Bob reserved I, Robot (ready for pickup)
            new Reservation { Id = 2, BookId = 6, PatronId = 2, ReservationDate = now.AddDays(-5), ExpirationDate = now.AddDays(2), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now.AddDays(-5) },
            // Pending reservation - Frank wants 1984
            new Reservation { Id = 3, BookId = 1, PatronId = 6, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-2) }
        );

        // Fines
        modelBuilder.Entity<Fine>().HasData(
            // Unpaid fine for Bob (overdue 1984)
            new Fine { Id = 1, PatronId = 2, LoanId = 4, Amount = 1.50m, Reason = "Overdue book: 1984 (6 days late)", IssuedDate = now, Status = FineStatus.Unpaid, CreatedAt = now },
            // Paid fine for Frank (late return of Sapiens - 3 days late)
            new Fine { Id = 2, PatronId = 6, LoanId = 10, Amount = 0.75m, Reason = "Overdue book: Sapiens (3 days late)", IssuedDate = now.AddDays(-15), PaidDate = now.AddDays(-14), Status = FineStatus.Paid, CreatedAt = now.AddDays(-15) },
            // Unpaid fine for David (various)
            new Fine { Id = 3, PatronId = 4, LoanId = 6, Amount = 0.00m, Reason = "Returned on time - waived processing fee", IssuedDate = now.AddDays(-17), Status = FineStatus.Waived, CreatedAt = now.AddDays(-17) }
        );
    }
}
