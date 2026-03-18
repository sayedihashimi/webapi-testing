using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryApi.Models;

public class Fine
{
    public int Id { get; set; }

    public int PatronId { get; set; }
    public Patron Patron { get; set; } = null!;

    public int LoanId { get; set; }
    public Loan Loan { get; set; } = null!;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    public DateTime? PaidDate { get; set; }

    public FineStatus Status { get; set; } = FineStatus.Unpaid;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
