namespace LibraryApi.DTOs;

public class FineDto
{
    public int Id { get; set; }
    public int PatronId { get; set; }
    public string PatronName { get; set; } = string.Empty;
    public int LoanId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
