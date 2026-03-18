using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
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

        // BookAuthor (many-to-many join)
        modelBuilder.Entity<BookAuthor>(e =>
        {
            e.HasKey(ba => new { ba.BookId, ba.AuthorId });
            e.HasOne(ba => ba.Book).WithMany(b => b.BookAuthors).HasForeignKey(ba => ba.BookId);
            e.HasOne(ba => ba.Author).WithMany(a => a.BookAuthors).HasForeignKey(ba => ba.AuthorId);
        });

        // BookCategory (many-to-many join)
        modelBuilder.Entity<BookCategory>(e =>
        {
            e.HasKey(bc => new { bc.BookId, bc.CategoryId });
            e.HasOne(bc => bc.Book).WithMany(b => b.BookCategories).HasForeignKey(bc => bc.BookId);
            e.HasOne(bc => bc.Category).WithMany(c => c.BookCategories).HasForeignKey(bc => bc.CategoryId);
        });

        // Patron
        modelBuilder.Entity<Patron>(e =>
        {
            e.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            e.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            e.Property(p => p.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(p => p.Email).IsUnique();
            e.Property(p => p.MembershipType).HasConversion<string>();
            e.Property(p => p.IsActive).HasDefaultValue(true);
        });

        // Loan
        modelBuilder.Entity<Loan>(e =>
        {
            e.Property(l => l.Status).HasConversion<string>();
            e.HasOne(l => l.Book).WithMany(b => b.Loans).HasForeignKey(l => l.BookId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(l => l.Patron).WithMany(p => p.Loans).HasForeignKey(l => l.PatronId).OnDelete(DeleteBehavior.Restrict);
        });

        // Reservation
        modelBuilder.Entity<Reservation>(e =>
        {
            e.Property(r => r.Status).HasConversion<string>();
            e.HasOne(r => r.Book).WithMany(b => b.Reservations).HasForeignKey(r => r.BookId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Patron).WithMany(p => p.Reservations).HasForeignKey(r => r.PatronId).OnDelete(DeleteBehavior.Restrict);
        });

        // Fine
        modelBuilder.Entity<Fine>(e =>
        {
            e.Property(f => f.Amount).HasColumnType("decimal(10,2)");
            e.Property(f => f.Reason).HasMaxLength(500).IsRequired();
            e.Property(f => f.Status).HasConversion<string>();
            e.HasOne(f => f.Patron).WithMany(p => p.Fines).HasForeignKey(f => f.PatronId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.Loan).WithMany(l => l.Fines).HasForeignKey(f => f.LoanId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
