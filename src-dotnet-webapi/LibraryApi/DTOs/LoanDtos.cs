using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

// Requests
public sealed record CreateLoanRequest
{
    [Required]
    public required int BookId { get; init; }

    [Required]
    public required int PatronId { get; init; }
}

// Responses
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
    int RenewalCount,
    DateTime CreatedAt);

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
    DateTime CreatedAt,
    IReadOnlyList<FineResponse> Fines);
