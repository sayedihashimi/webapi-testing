using LibraryApi.Models;

namespace LibraryApi.DTOs;

// Responses
public sealed record FineResponse(
    int Id,
    int PatronId,
    string PatronName,
    int LoanId,
    decimal Amount,
    string Reason,
    DateTime IssuedDate,
    DateTime? PaidDate,
    FineStatus Status,
    DateTime CreatedAt);
