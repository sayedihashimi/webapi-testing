using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public class LoanCreateDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int PatronId { get; set; }
}

public class LoanDto
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
    public string Status { get; set; } = string.Empty;
    public int RenewalCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
