using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models;

public sealed class Book
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ISBN { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Publisher { get; set; }

    public int? PublicationYear { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int? PageCount { get; set; }

    [MaxLength(50)]
    public string Language { get; set; } = "English";

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookAuthor> BookAuthors { get; set; } = [];
    public ICollection<BookCategory> BookCategories { get; set; } = [];
    public ICollection<Loan> Loans { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
}
