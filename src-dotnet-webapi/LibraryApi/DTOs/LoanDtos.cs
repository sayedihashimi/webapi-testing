using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

public class CreateLoanRequest
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int PatronId { get; set; }
}

public class LoanResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookISBN { get; set; } = string.Empty;
    public int PatronId { get; set; }
    public string PatronName { get; set; } = string.Empty;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public LoanStatus Status { get; set; }
    public int RenewalCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
