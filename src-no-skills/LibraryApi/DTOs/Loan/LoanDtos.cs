using System.ComponentModel.DataAnnotations;
using LibraryApi.Models.Enums;

namespace LibraryApi.DTOs.Loan;

public record LoanListDto(
    int Id, string BookTitle, string PatronName,
    DateTime LoanDate, DateTime DueDate, DateTime? ReturnDate,
    LoanStatus Status, int RenewalCount);

public record LoanDetailDto(
    int Id, int BookId, string BookTitle, string BookISBN,
    int PatronId, string PatronName, string PatronEmail,
    DateTime LoanDate, DateTime DueDate, DateTime? ReturnDate,
    LoanStatus Status, int RenewalCount, DateTime CreatedAt);

public class CreateLoanDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int PatronId { get; set; }
}

public record ReturnLoanDto(int Id, DateTime ReturnDate, bool IsOverdue, int OverdueDays, decimal? FineAmount);

public record RenewLoanDto(int Id, DateTime NewDueDate, int RenewalCount);
