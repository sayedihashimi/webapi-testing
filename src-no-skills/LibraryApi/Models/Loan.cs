using System.ComponentModel.DataAnnotations;
using LibraryApi.Models.Enums;

namespace LibraryApi.Models;

public class Loan
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public int PatronId { get; set; }
    public Patron Patron { get; set; } = null!;

    public DateTime LoanDate { get; set; } = DateTime.UtcNow;

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Active;

    [Range(0, 2)]
    public int RenewalCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Fine> Fines { get; set; } = [];
}
