namespace VetClinicApi.Models;

public sealed class Veterinarian
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public string? Specialization { get; set; }
    public required string LicenseNumber { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateOnly HireDate { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = [];
}
