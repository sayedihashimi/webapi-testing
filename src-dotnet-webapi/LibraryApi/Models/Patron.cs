namespace LibraryApi.Models;

public class Patron
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateOnly MembershipDate { get; set; }
    public MembershipType MembershipType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<Loan> Loans { get; set; } = [];
    public List<Reservation> Reservations { get; set; } = [];
    public List<Fine> Fines { get; set; } = [];
}
