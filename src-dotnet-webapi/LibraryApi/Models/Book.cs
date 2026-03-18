namespace LibraryApi.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public int? PublicationYear { get; set; }
    public string? Description { get; set; }
    public int? PageCount { get; set; }
    public string Language { get; set; } = "English";
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<BookAuthor> BookAuthors { get; set; } = [];
    public List<BookCategory> BookCategories { get; set; } = [];
    public List<Loan> Loans { get; set; } = [];
    public List<Reservation> Reservations { get; set; } = [];
}
