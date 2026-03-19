using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

// Requests
public sealed record CreateReservationRequest
{
    [Required]
    public required int BookId { get; init; }

    [Required]
    public required int PatronId { get; init; }
}

// Responses
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
