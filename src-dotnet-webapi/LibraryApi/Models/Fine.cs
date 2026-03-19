namespace LibraryApi.Models;

public sealed class Fine
{
    public int Id { get; set; }
    public int PatronId { get; set; }
    public Patron Patron { get; set; } = null!;
    public int LoanId { get; set; }
    public Loan Loan { get; set; } = null!;
    public decimal Amount { get; set; }
    public required string Reason { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public FineStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
