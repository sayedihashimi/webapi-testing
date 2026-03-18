using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.Models;

public class Owner
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public string? Address { get; set; }
    public string? City { get; set; }

    [MaxLength(2)]
    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
