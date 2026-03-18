using LibraryApi.Models;

namespace LibraryApi.DTOs;

// --- Loan DTOs ---
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

public sealed record CreateLoanRequest(int BookId, int PatronId);

// --- Reservation DTOs ---
public sealed record ReservationResponse(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime ReservationDate,
    DateTime? ExpirationDate,
    ReservationStatus Status,
    int QueuePosition,
    DateTime CreatedAt);

public sealed record CreateReservationRequest(int BookId, int PatronId);

// --- Fine DTOs ---
public sealed record FineResponse(
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
