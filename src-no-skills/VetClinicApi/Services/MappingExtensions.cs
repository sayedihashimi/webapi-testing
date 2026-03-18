using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public static class MappingExtensions
{
    // Owner
    public static OwnerResponseDto ToResponseDto(this Owner o) => new()
    {
        Id = o.Id,
        FirstName = o.FirstName,
        LastName = o.LastName,
        Email = o.Email,
        Phone = o.Phone,
        Address = o.Address,
        City = o.City,
        State = o.State,
        ZipCode = o.ZipCode,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt
    };

    public static OwnerDetailDto ToDetailDto(this Owner o) => new()
    {
        Id = o.Id,
        FirstName = o.FirstName,
        LastName = o.LastName,
        Email = o.Email,
        Phone = o.Phone,
        Address = o.Address,
        City = o.City,
        State = o.State,
        ZipCode = o.ZipCode,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
        Pets = o.Pets.Select(p => p.ToResponseDto())
    };

    // Pet
    public static PetResponseDto ToResponseDto(this Pet p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Species = p.Species,
        Breed = p.Breed,
        DateOfBirth = p.DateOfBirth,
        Weight = p.Weight,
        Color = p.Color,
        MicrochipNumber = p.MicrochipNumber,
        IsActive = p.IsActive,
        OwnerId = p.OwnerId,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    public static PetDetailDto ToDetailDto(this Pet p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Species = p.Species,
        Breed = p.Breed,
        DateOfBirth = p.DateOfBirth,
        Weight = p.Weight,
        Color = p.Color,
        MicrochipNumber = p.MicrochipNumber,
        IsActive = p.IsActive,
        OwnerId = p.OwnerId,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Owner = p.Owner.ToResponseDto()
    };

    // Veterinarian
    public static VetResponseDto ToResponseDto(this Veterinarian v) => new()
    {
        Id = v.Id,
        FirstName = v.FirstName,
        LastName = v.LastName,
        Email = v.Email,
        Phone = v.Phone,
        Specialization = v.Specialization,
        LicenseNumber = v.LicenseNumber,
        IsAvailable = v.IsAvailable,
        HireDate = v.HireDate
    };

    // Appointment
    public static AppointmentResponseDto ToResponseDto(this Appointment a) => new()
    {
        Id = a.Id,
        PetId = a.PetId,
        PetName = a.Pet?.Name ?? string.Empty,
        VeterinarianId = a.VeterinarianId,
        VeterinarianName = a.Veterinarian != null ? $"Dr. {a.Veterinarian.FirstName} {a.Veterinarian.LastName}" : string.Empty,
        AppointmentDate = a.AppointmentDate,
        DurationMinutes = a.DurationMinutes,
        Status = a.Status.ToString(),
        Reason = a.Reason,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };

    public static AppointmentDetailDto ToDetailDto(this Appointment a) => new()
    {
        Id = a.Id,
        PetId = a.PetId,
        PetName = a.Pet?.Name ?? string.Empty,
        VeterinarianId = a.VeterinarianId,
        VeterinarianName = a.Veterinarian != null ? $"Dr. {a.Veterinarian.FirstName} {a.Veterinarian.LastName}" : string.Empty,
        AppointmentDate = a.AppointmentDate,
        DurationMinutes = a.DurationMinutes,
        Status = a.Status.ToString(),
        Reason = a.Reason,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        Pet = a.Pet?.ToResponseDto()!,
        Veterinarian = a.Veterinarian?.ToResponseDto()!,
        MedicalRecord = a.MedicalRecord?.ToResponseDto()
    };

    // MedicalRecord
    public static MedicalRecordResponseDto ToResponseDto(this MedicalRecord m) => new()
    {
        Id = m.Id,
        AppointmentId = m.AppointmentId,
        PetId = m.PetId,
        PetName = m.Pet?.Name ?? string.Empty,
        VeterinarianId = m.VeterinarianId,
        VeterinarianName = m.Veterinarian != null ? $"Dr. {m.Veterinarian.FirstName} {m.Veterinarian.LastName}" : string.Empty,
        Diagnosis = m.Diagnosis,
        Treatment = m.Treatment,
        Notes = m.Notes,
        FollowUpDate = m.FollowUpDate,
        CreatedAt = m.CreatedAt,
        Prescriptions = m.Prescriptions?.Select(p => p.ToResponseDto()) ?? Enumerable.Empty<PrescriptionResponseDto>()
    };

    // Prescription
    public static PrescriptionResponseDto ToResponseDto(this Prescription p) => new()
    {
        Id = p.Id,
        MedicalRecordId = p.MedicalRecordId,
        MedicationName = p.MedicationName,
        Dosage = p.Dosage,
        DurationDays = p.DurationDays,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Instructions = p.Instructions,
        IsActive = p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow),
        CreatedAt = p.CreatedAt
    };

    // Vaccination
    public static VaccinationResponseDto ToResponseDto(this Vaccination v) => new()
    {
        Id = v.Id,
        PetId = v.PetId,
        PetName = v.Pet?.Name ?? string.Empty,
        VaccineName = v.VaccineName,
        DateAdministered = v.DateAdministered,
        ExpirationDate = v.ExpirationDate,
        BatchNumber = v.BatchNumber,
        AdministeredByVetId = v.AdministeredByVetId,
        VeterinarianName = v.AdministeredByVet != null ? $"Dr. {v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}" : string.Empty,
        Notes = v.Notes,
        IsExpired = v.IsExpired,
        IsDueSoon = v.IsDueSoon,
        CreatedAt = v.CreatedAt
    };

    // Pagination helper
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, PaginationParams pagination)
    {
        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);
        var items = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            query.Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize));

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
