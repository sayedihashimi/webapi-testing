using System.ComponentModel.DataAnnotations;
using LibraryApi.Models.Enums;

namespace LibraryApi.DTOs.Reservation;

public record ReservationListDto(
    int Id, string BookTitle, string PatronName,
    DateTime ReservationDate, DateTime? ExpirationDate,
    ReservationStatus Status, int QueuePosition);

public record ReservationDetailDto(
    int Id, int BookId, string BookTitle,
    int PatronId, string PatronName, string PatronEmail,
    DateTime ReservationDate, DateTime? ExpirationDate,
    ReservationStatus Status, int QueuePosition, DateTime CreatedAt);

public class CreateReservationDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int PatronId { get; set; }
}
