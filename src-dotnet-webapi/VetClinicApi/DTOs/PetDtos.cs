using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Pet DTOs ---

public class CreatePetRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Species { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Breed { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Range(0.01, 9999.99)]
    public decimal? Weight { get; set; }

    public string? Color { get; set; }
    public string? MicrochipNumber { get; set; }

    [Required]
    public int OwnerId { get; set; }
}

public class UpdatePetRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Species { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Breed { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Range(0.01, 9999.99)]
    public decimal? Weight { get; set; }

    public string? Color { get; set; }
    public string? MicrochipNumber { get; set; }

    [Required]
    public int OwnerId { get; set; }
}

public class PetResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public decimal? Weight { get; set; }
    public string? Color { get; set; }
    public string? MicrochipNumber { get; set; }
    public bool IsActive { get; set; }
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public OwnerSummaryResponse? Owner { get; set; }
}

public class PetSummaryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public bool IsActive { get; set; }
}
