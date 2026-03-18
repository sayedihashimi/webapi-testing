namespace VetClinicApi.DTOs;

// ===== Pagination =====
public class PaginationParams
{
    private int _page = 1;
    private int _pageSize = 10;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : (value > 100 ? 100 : value);
    }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}

// ===== Owner DTOs =====
public class OwnerCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}

public class OwnerUpdateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}

public class OwnerResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OwnerDetailDto : OwnerResponseDto
{
    public IEnumerable<PetResponseDto> Pets { get; set; } = Enumerable.Empty<PetResponseDto>();
}

// ===== Pet DTOs =====
public class PetCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public decimal? Weight { get; set; }
    public string? Color { get; set; }
    public string? MicrochipNumber { get; set; }
    public int OwnerId { get; set; }
}

public class PetUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public decimal? Weight { get; set; }
    public string? Color { get; set; }
    public string? MicrochipNumber { get; set; }
    public int OwnerId { get; set; }
}

public class PetResponseDto
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
}

public class PetDetailDto : PetResponseDto
{
    public OwnerResponseDto Owner { get; set; } = null!;
}

// ===== Veterinarian DTOs =====
public class VetCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
}

public class VetUpdateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateOnly HireDate { get; set; }
}

public class VetResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateOnly HireDate { get; set; }
}

// ===== Appointment DTOs =====
public class AppointmentCreateDto
{
    public int PetId { get; set; }
    public int VeterinarianId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class AppointmentUpdateDto
{
    public DateTime AppointmentDate { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public int VeterinarianId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class AppointmentStatusUpdateDto
{
    public string Status { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
}

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public int VeterinarianId { get; set; }
    public string VeterinarianName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AppointmentDetailDto : AppointmentResponseDto
{
    public PetResponseDto Pet { get; set; } = null!;
    public VetResponseDto Veterinarian { get; set; } = null!;
    public MedicalRecordResponseDto? MedicalRecord { get; set; }
}

// ===== Medical Record DTOs =====
public class MedicalRecordCreateDto
{
    public int AppointmentId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly? FollowUpDate { get; set; }
}

public class MedicalRecordUpdateDto
{
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly? FollowUpDate { get; set; }
}

public class MedicalRecordResponseDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public int VeterinarianId { get; set; }
    public string VeterinarianName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<PrescriptionResponseDto> Prescriptions { get; set; } = Enumerable.Empty<PrescriptionResponseDto>();
}

// ===== Prescription DTOs =====
public class PrescriptionCreateDto
{
    public int MedicalRecordId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public DateOnly StartDate { get; set; }
    public string? Instructions { get; set; }
}

public class PrescriptionResponseDto
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Instructions { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== Vaccination DTOs =====
public class VaccinationCreateDto
{
    public int PetId { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateOnly DateAdministered { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
    public int AdministeredByVetId { get; set; }
    public string? Notes { get; set; }
}

public class VaccinationResponseDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string VaccineName { get; set; } = string.Empty;
    public DateOnly DateAdministered { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
    public int AdministeredByVetId { get; set; }
    public string VeterinarianName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsExpired { get; set; }
    public bool IsDueSoon { get; set; }
    public DateTime CreatedAt { get; set; }
}
