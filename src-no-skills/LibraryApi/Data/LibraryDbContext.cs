using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<BookCategory> BookCategories => Set<BookCategory>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Patron> Patrons => Set<Patron>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Fine> Fines => Set<Fine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // BookAuthor many-to-many
        modelBuilder.Entity<BookAuthor>()
            .HasKey(ba => new { ba.BookId, ba.AuthorId });

        modelBuilder.Entity<BookAuthor>()
            .HasOne(ba => ba.Book)
            .WithMany(b => b.BookAuthors)
            .HasForeignKey(ba => ba.BookId);

        modelBuilder.Entity<BookAuthor>()
            .HasOne(ba => ba.Author)
            .WithMany(a => a.BookAuthors)
            .HasForeignKey(ba => ba.AuthorId);

        // BookCategory many-to-many
        modelBuilder.Entity<BookCategory>()
            .HasKey(bc => new { bc.BookId, bc.CategoryId });

        modelBuilder.Entity<BookCategory>()
            .HasOne(bc => bc.Book)
            .WithMany(b => b.BookCategories)
            .HasForeignKey(bc => bc.BookId);

        modelBuilder.Entity<BookCategory>()
            .HasOne(bc => bc.Category)
            .WithMany(c => c.BookCategories)
            .HasForeignKey(bc => bc.CategoryId);

        // Book: unique ISBN
        modelBuilder.Entity<Book>()
            .HasIndex(b => b.ISBN)
            .IsUnique();

        // Category: unique Name
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Patron: unique Email
        modelBuilder.Entity<Patron>()
            .HasIndex(p => p.Email)
            .IsUnique();

        // Fine: decimal precision
        modelBuilder.Entity<Fine>()
            .Property(f => f.Amount)
            .HasColumnType("decimal(10,2)");

        // Loan relationships
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Book)
            .WithMany(b => b.Loans)
            .HasForeignKey(l => l.BookId);

        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Patron)
            .WithMany(p => p.Loans)
            .HasForeignKey(l => l.PatronId);

        // Reservation relationships
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Book)
            .WithMany(b => b.Reservations)
            .HasForeignKey(r => r.BookId);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Patron)
            .WithMany(p => p.Reservations)
            .HasForeignKey(r => r.PatronId);

        // Fine relationships
        modelBuilder.Entity<Fine>()
            .HasOne(f => f.Patron)
            .WithMany(p => p.Fines)
            .HasForeignKey(f => f.PatronId);

        modelBuilder.Entity<Fine>()
            .HasOne(f => f.Loan)
            .WithMany(l => l.Fines)
            .HasForeignKey(f => f.LoanId);

        // Store enums as strings for readability
        modelBuilder.Entity<Patron>()
            .Property(p => p.MembershipType)
            .HasConversion<string>();

        modelBuilder.Entity<Loan>()
            .Property(l => l.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Reservation>()
            .Property(r => r.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Fine>()
            .Property(f => f.Status)
            .HasConversion<string>();
    }
}
