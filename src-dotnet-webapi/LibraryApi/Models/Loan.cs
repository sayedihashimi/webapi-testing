namespace LibraryApi.Models;

public sealed class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int PatronId { get; set; }
    public Patron Patron { get; set; } = null!;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public LoanStatus Status { get; set; }
    public int RenewalCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Fine> Fines { get; set; } = [];
}
