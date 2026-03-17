using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

// --- Loan DTOs ---
public class LoanDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int PatronId { get; set; }
    public string PatronName { get; set; } = string.Empty;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RenewalCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLoanDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int PatronId { get; set; }
}

// --- Reservation DTOs ---
public class ReservationDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int PatronId { get; set; }
    public string PatronName { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int QueuePosition { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReservationDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int PatronId { get; set; }
}

// --- Fine DTOs ---
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
