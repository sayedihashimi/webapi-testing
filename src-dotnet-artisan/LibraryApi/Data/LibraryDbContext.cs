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
        modelBuilder.Entity<BookAuthor>(entity =>
        {
            entity.HasKey(ba => new { ba.BookId, ba.AuthorId });
            entity.HasOne(ba => ba.Book).WithMany(b => b.BookAuthors).HasForeignKey(ba => ba.BookId);
            entity.HasOne(ba => ba.Author).WithMany(a => a.BookAuthors).HasForeignKey(ba => ba.AuthorId);
        });

        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.HasKey(bc => new { bc.BookId, bc.CategoryId });
            entity.HasOne(bc => bc.Book).WithMany(b => b.BookCategories).HasForeignKey(bc => bc.BookId);
            entity.HasOne(bc => bc.Category).WithMany(c => c.BookCategories).HasForeignKey(bc => bc.CategoryId);
        });

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<Book>()
            .HasIndex(b => b.ISBN)
            .IsUnique();

        modelBuilder.Entity<Patron>()
            .HasIndex(p => p.Email)
            .IsUnique();

        modelBuilder.Entity<Fine>()
            .Property(f => f.Amount)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasOne(l => l.Book).WithMany(b => b.Loans).HasForeignKey(l => l.BookId);
            entity.HasOne(l => l.Patron).WithMany(p => p.Loans).HasForeignKey(l => l.PatronId);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasOne(r => r.Book).WithMany(b => b.Reservations).HasForeignKey(r => r.BookId);
            entity.HasOne(r => r.Patron).WithMany(p => p.Reservations).HasForeignKey(r => r.PatronId);
        });

        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasOne(f => f.Patron).WithMany(p => p.Fines).HasForeignKey(f => f.PatronId);
            entity.HasOne(f => f.Loan).WithMany(l => l.Fines).HasForeignKey(f => f.LoanId);
        });
    }
}
