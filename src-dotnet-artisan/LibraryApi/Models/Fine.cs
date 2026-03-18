using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models;

public sealed class Fine
{
    public int Id { get; set; }

    public int PatronId { get; set; }
    public Patron Patron { get; set; } = null!;

    public int LoanId { get; set; }
    public Loan Loan { get; set; } = null!;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    public DateTime? PaidDate { get; set; }

    public FineStatus Status { get; set; } = FineStatus.Unpaid;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
