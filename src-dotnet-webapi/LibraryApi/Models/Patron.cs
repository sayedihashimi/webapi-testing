namespace LibraryApi.Models;

public sealed class Patron
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateOnly MembershipDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public MembershipType MembershipType { get; set; } = MembershipType.Standard;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Loan> Loans { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
    public ICollection<Fine> Fines { get; set; } = [];
}
