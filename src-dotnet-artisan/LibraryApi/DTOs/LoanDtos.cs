using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

public sealed record LoanResponse(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnDate,
    LoanStatus Status,
    int RenewalCount);

public sealed record LoanDetailResponse(
    int Id,
    int BookId,
    string BookTitle,
    string BookISBN,
    int PatronId,
    string PatronName,
    string PatronEmail,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnDate,
    LoanStatus Status,
    int RenewalCount,
    DateTime CreatedAt);

public sealed class CreateLoanRequest
{
    [Required]
    public int BookId { get; init; }

    [Required]
    public int PatronId { get; init; }
}
