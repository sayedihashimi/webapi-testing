using LibraryApi.Models;

namespace LibraryApi.DTOs;

public sealed record FineResponse(
    int Id,
    int PatronId,
    string PatronName,
    int LoanId,
    decimal Amount,
    string Reason,
    DateTime IssuedDate,
    DateTime? PaidDate,
    FineStatus Status);

public sealed record FineDetailResponse(
    int Id,
    int PatronId,
    string PatronName,
    int LoanId,
    string BookTitle,
    decimal Amount,
    string Reason,
    DateTime IssuedDate,
    DateTime? PaidDate,
    FineStatus Status,
    DateTime CreatedAt);
