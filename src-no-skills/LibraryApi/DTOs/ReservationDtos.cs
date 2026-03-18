using LibraryApi.Models;

namespace LibraryApi.DTOs;

public class ReservationDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int PatronId { get; set; }
    public string PatronName { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public ReservationStatus Status { get; set; }
    public int QueuePosition { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReservationDto
{
    public int BookId { get; set; }
    public int PatronId { get; set; }
}
