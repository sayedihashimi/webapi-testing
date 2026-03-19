using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

public sealed record ReservationResponse(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime ReservationDate,
    DateTime? ExpirationDate,
    ReservationStatus Status,
    int QueuePosition);

public sealed record ReservationDetailResponse(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    string PatronEmail,
    DateTime ReservationDate,
    DateTime? ExpirationDate,
    ReservationStatus Status,
    int QueuePosition,
    DateTime CreatedAt);

public sealed class CreateReservationRequest
{
    [Required]
    public int BookId { get; init; }

    [Required]
    public int PatronId { get; init; }
}
