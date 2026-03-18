namespace VetClinicApi.Models;

public class Veterinarian
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public DateOnly HireDate { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
