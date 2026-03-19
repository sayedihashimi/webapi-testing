using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public class ReservationCreateDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int PatronId { get; set; }
}

public class ReservationDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int PatronId { get; set; }
    public string PatronName { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int QueuePosition { get; set; }
    public DateTime CreatedAt { get; set; }
}
