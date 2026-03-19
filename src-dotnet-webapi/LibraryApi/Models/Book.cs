namespace LibraryApi.Models;

public sealed class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string ISBN { get; set; }
    public string? Publisher { get; set; }
    public int? PublicationYear { get; set; }
    public string? Description { get; set; }
    public int? PageCount { get; set; }
    public string Language { get; set; } = "English";
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookAuthor> BookAuthors { get; set; } = [];
    public ICollection<BookCategory> BookCategories { get; set; } = [];
    public ICollection<Loan> Loans { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
}
