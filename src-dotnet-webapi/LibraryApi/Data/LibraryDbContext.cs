using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
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
        base.OnModelCreating(modelBuilder);

        // Author
        modelBuilder.Entity<Author>(e =>
        {
            e.Property(a => a.FirstName).HasMaxLength(100).IsRequired();
            e.Property(a => a.LastName).HasMaxLength(100).IsRequired();
            e.Property(a => a.Biography).HasMaxLength(2000);
            e.Property(a => a.Country).HasMaxLength(100);
            e.Property(a => a.CreatedAt).HasDefaultValueSql("datetime('now')");
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
            e.Property(b => b.CreatedAt).HasDefaultValueSql("datetime('now')");
            e.Property(b => b.UpdatedAt).HasDefaultValueSql("datetime('now')");
        });

        // BookAuthor (M2M join)
        modelBuilder.Entity<BookAuthor>(e =>
        {
            e.HasKey(ba => new { ba.BookId, ba.AuthorId });
            e.HasOne(ba => ba.Book).WithMany(b => b.BookAuthors).HasForeignKey(ba => ba.BookId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ba => ba.Author).WithMany(a => a.BookAuthors).HasForeignKey(ba => ba.AuthorId).OnDelete(DeleteBehavior.Cascade);
        });

        // BookCategory (M2M join)
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
            e.Property(p => p.IsActive).HasDefaultValue(true);
            e.Property(p => p.CreatedAt).HasDefaultValueSql("datetime('now')");
            e.Property(p => p.UpdatedAt).HasDefaultValueSql("datetime('now')");
        });

        // Loan
        modelBuilder.Entity<Loan>(e =>
        {
            e.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(l => l.CreatedAt).HasDefaultValueSql("datetime('now')");
            e.HasOne(l => l.Book).WithMany(b => b.Loans).HasForeignKey(l => l.BookId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(l => l.Patron).WithMany(p => p.Loans).HasForeignKey(l => l.PatronId).OnDelete(DeleteBehavior.Restrict);
        });

        // Reservation
        modelBuilder.Entity<Reservation>(e =>
        {
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.CreatedAt).HasDefaultValueSql("datetime('now')");
            e.HasOne(r => r.Book).WithMany(b => b.Reservations).HasForeignKey(r => r.BookId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Patron).WithMany(p => p.Reservations).HasForeignKey(r => r.PatronId).OnDelete(DeleteBehavior.Restrict);
        });

        // Fine
        modelBuilder.Entity<Fine>(e =>
        {
            e.Property(f => f.Amount).HasColumnType("decimal(10,2)");
            e.Property(f => f.Reason).HasMaxLength(500).IsRequired();
            e.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(f => f.CreatedAt).HasDefaultValueSql("datetime('now')");
            e.HasOne(f => f.Patron).WithMany(p => p.Fines).HasForeignKey(f => f.PatronId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.Loan).WithMany(l => l.Fines).HasForeignKey(f => f.LoanId).OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Authors
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FirstName = "Harper", LastName = "Lee", Biography = "American novelist known for To Kill a Mockingbird.", BirthDate = new DateOnly(1926, 4, 28), Country = "United States", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 2, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, known for 1984 and Animal Farm.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "American author and biochemistry professor, prolific science fiction writer.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 4, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and author of Sapiens and Homo Deus.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 5, FirstName = "Walter", LastName = "Isaacson", Biography = "American journalist and biographer.", BirthDate = new DateOnly(1952, 5, 20), Country = "United States", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 6, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for Pride and Prejudice and other classic works.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new Category { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science and technology" },
            new Category { Id = 3, Name = "History", Description = "Non-fiction works about past events" },
            new Category { Id = 4, Name = "Science", Description = "Non-fiction works about the natural world" },
            new Category { Id = 5, Name = "Biography", Description = "Non-fiction accounts of a person's life" }
        );

        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Books
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "To Kill a Mockingbird", ISBN = "978-0-06-112008-4", Publisher = "J.B. Lippincott & Co.", PublicationYear = 1960, Description = "A novel about racial injustice in the American South.", PageCount = 281, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 2, Title = "1984", ISBN = "978-0-452-28423-4", Publisher = "Secker & Warburg", PublicationYear = 1949, Description = "A dystopian novel set in a totalitarian society.", PageCount = 328, Language = "English", TotalCopies = 4, AvailableCopies = 3, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 3, Title = "Animal Farm", ISBN = "978-0-452-28424-1", Publisher = "Secker & Warburg", PublicationYear = 1945, Description = "An allegorical novella about a group of farm animals.", PageCount = 112, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 4, Title = "Foundation", ISBN = "978-0-553-29335-7", Publisher = "Gnome Press", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 5, Title = "I, Robot", ISBN = "978-0-553-29438-5", Publisher = "Gnome Press", PublicationYear = 1950, Description = "A collection of nine science fiction short stories.", PageCount = 253, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 6, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0-06-231609-7", Publisher = "Harper", PublicationYear = 2011, Description = "A narrative history of humankind from the Stone Age to the present.", PageCount = 443, Language = "English", TotalCopies = 5, AvailableCopies = 4, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 7, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0-06-246431-6", Publisher = "Harper", PublicationYear = 2015, Description = "An exploration of humanity's future.", PageCount = 450, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 8, Title = "Steve Jobs", ISBN = "978-1-4516-4853-9", Publisher = "Simon & Schuster", PublicationYear = 2011, Description = "The authorized biography of Apple co-founder Steve Jobs.", PageCount = 656, Language = "English", TotalCopies = 4, AvailableCopies = 4, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 9, Title = "Pride and Prejudice", ISBN = "978-0-14-143951-8", Publisher = "T. Egerton", PublicationYear = 1813, Description = "A classic novel of manners set in Georgian England.", PageCount = 432, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 10, Title = "Sense and Sensibility", ISBN = "978-0-14-143966-2", Publisher = "T. Egerton", PublicationYear = 1811, Description = "A novel about the Dashwood sisters and their romantic pursuits.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 11, Title = "The Caves of Steel", ISBN = "978-0-553-29340-1", Publisher = "Doubleday", PublicationYear = 1954, Description = "A science fiction detective novel featuring robot R. Daneel Olivaw.", PageCount = 206, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Book { Id = 12, Title = "21 Lessons for the 21st Century", ISBN = "978-0-525-51217-2", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "Explores the biggest questions facing humanity today.", PageCount = 372, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = seedDate, UpdatedAt = seedDate }
        );

        // BookAuthors (M2M)
        modelBuilder.Entity<BookAuthor>().HasData(
            new BookAuthor { BookId = 1, AuthorId = 1 },   // TKAM - Harper Lee
            new BookAuthor { BookId = 2, AuthorId = 2 },   // 1984 - Orwell
            new BookAuthor { BookId = 3, AuthorId = 2 },   // Animal Farm - Orwell
            new BookAuthor { BookId = 4, AuthorId = 3 },   // Foundation - Asimov
            new BookAuthor { BookId = 5, AuthorId = 3 },   // I, Robot - Asimov
            new BookAuthor { BookId = 6, AuthorId = 4 },   // Sapiens - Harari
            new BookAuthor { BookId = 7, AuthorId = 4 },   // Homo Deus - Harari
            new BookAuthor { BookId = 8, AuthorId = 5 },   // Steve Jobs - Isaacson
            new BookAuthor { BookId = 9, AuthorId = 6 },   // P&P - Austen
            new BookAuthor { BookId = 10, AuthorId = 6 },  // S&S - Austen
            new BookAuthor { BookId = 11, AuthorId = 3 },  // Caves of Steel - Asimov
            new BookAuthor { BookId = 12, AuthorId = 4 }   // 21 Lessons - Harari
        );

        // BookCategories (M2M)
        modelBuilder.Entity<BookCategory>().HasData(
            new BookCategory { BookId = 1, CategoryId = 1 },   // TKAM - Fiction
            new BookCategory { BookId = 2, CategoryId = 1 },   // 1984 - Fiction
            new BookCategory { BookId = 2, CategoryId = 2 },   // 1984 - Sci-Fi
            new BookCategory { BookId = 3, CategoryId = 1 },   // Animal Farm - Fiction
            new BookCategory { BookId = 4, CategoryId = 2 },   // Foundation - Sci-Fi
            new BookCategory { BookId = 5, CategoryId = 2 },   // I, Robot - Sci-Fi
            new BookCategory { BookId = 5, CategoryId = 4 },   // I, Robot - Science
            new BookCategory { BookId = 6, CategoryId = 3 },   // Sapiens - History
            new BookCategory { BookId = 6, CategoryId = 4 },   // Sapiens - Science
            new BookCategory { BookId = 7, CategoryId = 3 },   // Homo Deus - History
            new BookCategory { BookId = 7, CategoryId = 4 },   // Homo Deus - Science
            new BookCategory { BookId = 8, CategoryId = 5 },   // Steve Jobs - Biography
            new BookCategory { BookId = 9, CategoryId = 1 },   // P&P - Fiction
            new BookCategory { BookId = 10, CategoryId = 1 },  // S&S - Fiction
            new BookCategory { BookId = 11, CategoryId = 2 },  // Caves of Steel - Sci-Fi
            new BookCategory { BookId = 12, CategoryId = 3 }   // 21 Lessons - History
        );

        // Patrons
        modelBuilder.Entity<Patron>().HasData(
            new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", MembershipDate = new DateOnly(2023, 6, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Patron { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Elm Avenue", MembershipDate = new DateOnly(2024, 1, 10), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Patron { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Road", MembershipDate = new DateOnly(2024, 9, 1), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Patron { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Maple Drive", MembershipDate = new DateOnly(2024, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Patron { Id = 5, FirstName = "Eve", LastName = "Davis", Email = "eve.davis@email.com", Phone = "555-0105", Address = "654 Cedar Lane", MembershipDate = new DateOnly(2023, 11, 5), MembershipType = MembershipType.Premium, IsActive = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Patron { Id = 6, FirstName = "Frank", LastName = "Wilson", Email = "frank.wilson@email.com", Phone = "555-0106", Address = "987 Birch Court", MembershipDate = new DateOnly(2024, 8, 15), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate }
        );

        // Loans
        modelBuilder.Entity<Loan>().HasData(
            new Loan { Id = 1, BookId = 2, PatronId = 1, LoanDate = new DateTime(2025, 3, 10, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2025, 3, 31, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = new DateTime(2025, 3, 10, 10, 0, 0, DateTimeKind.Utc) },
            new Loan { Id = 2, BookId = 4, PatronId = 2, LoanDate = new DateTime(2025, 3, 5, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2025, 3, 19, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = new DateTime(2025, 3, 5, 10, 0, 0, DateTimeKind.Utc) },
            new Loan { Id = 3, BookId = 6, PatronId = 3, LoanDate = new DateTime(2025, 3, 12, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2025, 3, 19, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = new DateTime(2025, 3, 12, 10, 0, 0, DateTimeKind.Utc) },
            new Loan { Id = 4, BookId = 9, PatronId = 1, LoanDate = new DateTime(2025, 2, 1, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2025, 2, 22, 10, 0, 0, DateTimeKind.Utc), ReturnDate = new DateTime(2025, 2, 24, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = new DateTime(2025, 2, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Loan { Id = 5, BookId = 1, PatronId = 2, LoanDate = new DateTime(2025, 2, 23, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2025, 3, 9, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = new DateTime(2025, 2, 23, 10, 0, 0, DateTimeKind.Utc) },
            new Loan { Id = 6, BookId = 5, PatronId = 4, LoanDate = new DateTime(2025, 3, 8, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2025, 3, 22, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = new DateTime(2025, 3, 8, 10, 0, 0, DateTimeKind.Utc) },
            new Loan { Id = 7, BookId = 3, PatronId = 6, LoanDate = new DateTime(2025, 3, 5, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2025, 3, 12, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = new DateTime(2025, 3, 5, 10, 0, 0, DateTimeKind.Utc) },
            new Loan { Id = 8, BookId = 7, PatronId = 1, LoanDate = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2025, 2, 5, 10, 0, 0, DateTimeKind.Utc), ReturnDate = new DateTime(2025, 2, 3, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc) }
        );

        // Reservations
        modelBuilder.Entity<Reservation>().HasData(
            new Reservation { Id = 1, BookId = 1, PatronId = 4, ReservationDate = new DateTime(2025, 3, 10, 10, 0, 0, DateTimeKind.Utc), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = new DateTime(2025, 3, 10, 10, 0, 0, DateTimeKind.Utc) },
            new Reservation { Id = 2, BookId = 3, PatronId = 3, ReservationDate = new DateTime(2025, 3, 11, 10, 0, 0, DateTimeKind.Utc), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = new DateTime(2025, 3, 11, 10, 0, 0, DateTimeKind.Utc) },
            new Reservation { Id = 3, BookId = 5, PatronId = 1, ReservationDate = new DateTime(2025, 3, 8, 10, 0, 0, DateTimeKind.Utc), ExpirationDate = new DateTime(2025, 3, 18, 10, 0, 0, DateTimeKind.Utc), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = new DateTime(2025, 3, 8, 10, 0, 0, DateTimeKind.Utc) }
        );

        // Fines
        modelBuilder.Entity<Fine>().HasData(
            new Fine { Id = 1, PatronId = 2, LoanId = 5, Amount = 1.50m, Reason = "Overdue: 6 days late", IssuedDate = new DateTime(2025, 3, 15, 10, 0, 0, DateTimeKind.Utc), Status = FineStatus.Unpaid, CreatedAt = new DateTime(2025, 3, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Fine { Id = 2, PatronId = 6, LoanId = 7, Amount = 0.75m, Reason = "Overdue: 3 days late", IssuedDate = new DateTime(2025, 3, 15, 10, 0, 0, DateTimeKind.Utc), Status = FineStatus.Unpaid, CreatedAt = new DateTime(2025, 3, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Fine { Id = 3, PatronId = 1, LoanId = 4, Amount = 0.50m, Reason = "Overdue: 2 days late", IssuedDate = new DateTime(2025, 2, 24, 10, 0, 0, DateTimeKind.Utc), PaidDate = new DateTime(2025, 2, 25, 10, 0, 0, DateTimeKind.Utc), Status = FineStatus.Paid, CreatedAt = new DateTime(2025, 2, 24, 10, 0, 0, DateTimeKind.Utc) }
        );
    }
}
