namespace LibraryApi.Models;

public class Reservation
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public int PatronId { get; set; }
    public Patron Patron { get; set; } = null!;

    public DateTime ReservationDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpirationDate { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public int QueuePosition { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
