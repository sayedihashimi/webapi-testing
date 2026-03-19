using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PetService(VetClinicDbContext db, ILogger<PetService> logger) : IPetService
{
    public async Task<PagedResult<PetDto>> GetAllAsync(string? search, int page, int pageSize, bool includeInactive, CancellationToken ct = default)
    {
        var query = db.Pets.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Species.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync(ct);

        return new PagedResult<PetDto>(items, totalCount, page, pageSize);
    }

    public async Task<PetDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var pet = await db.Pets
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (pet is null)
        {
            return null;
        }

        var ownerDto = new OwnerDto(
            pet.Owner.Id, pet.Owner.FirstName, pet.Owner.LastName, pet.Owner.Email, pet.Owner.Phone,
            pet.Owner.Address, pet.Owner.City, pet.Owner.State, pet.Owner.ZipCode,
            pet.Owner.CreatedAt, pet.Owner.UpdatedAt);

        return new PetDetailDto(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth,
            pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive,
            pet.OwnerId, ownerDto, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<PetDto> CreateAsync(CreatePetDto dto, CancellationToken ct = default)
    {
        var pet = new Pet
        {
            Name = dto.Name,
            Species = dto.Species,
            Breed = dto.Breed,
            DateOfBirth = dto.DateOfBirth,
            Weight = dto.Weight,
            Color = dto.Color,
            MicrochipNumber = dto.MicrochipNumber,
            OwnerId = dto.OwnerId
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created pet {PetId}: {PetName} for owner {OwnerId}", pet.Id, pet.Name, pet.OwnerId);
        return MapToDto(pet);
    }

    public async Task<PetDto?> UpdateAsync(int id, UpdatePetDto dto, CancellationToken ct = default)
    {
        var pet = await db.Pets.FindAsync([id], ct);
        if (pet is null)
        {
            return null;
        }

        pet.Name = dto.Name;
        pet.Species = dto.Species;
        pet.Breed = dto.Breed;
        pet.DateOfBirth = dto.DateOfBirth;
        pet.Weight = dto.Weight;
        pet.Color = dto.Color;
        pet.MicrochipNumber = dto.MicrochipNumber;
        pet.OwnerId = dto.OwnerId;
        pet.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated pet {PetId}", pet.Id);
        return MapToDto(pet);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var pet = await db.Pets.FindAsync([id], ct);
        if (pet is null)
        {
            return false;
        }

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Soft-deleted pet {PetId}", id);
        return true;
    }

    public async Task<IReadOnlyList<MedicalRecordDto>> GetMedicalRecordsAsync(int petId, CancellationToken ct = default) =>
        await db.MedicalRecords
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordDto(
                m.Id, m.AppointmentId, m.PetId, m.VeterinarianId,
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<VaccinationDto>> GetVaccinationsAsync(int petId, CancellationToken ct = default) =>
        await db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => new VaccinationDto(
                v.Id, v.PetId, v.VaccineName, v.DateAdministered, v.ExpirationDate,
                v.BatchNumber, v.AdministeredByVetId,
                v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
                v.Notes, v.CreatedAt, v.IsExpired, v.IsDueSoon))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var soon = today.AddDays(30);

        var vaccinations = await db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= soon)
            .OrderBy(v => v.ExpirationDate)
            .ToListAsync(ct);

        return vaccinations.Select(v => new VaccinationDto(
            v.Id, v.PetId, v.VaccineName, v.DateAdministered, v.ExpirationDate,
            v.BatchNumber, v.AdministeredByVetId,
            $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
            v.Notes, v.CreatedAt, v.IsExpired, v.IsDueSoon)).ToList();
    }

    public async Task<IReadOnlyList<PrescriptionDto>> GetActivePrescriptionsAsync(int petId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await db.Prescriptions
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId)
            .ToListAsync(ct);

        return prescriptions
            .Where(p => p.IsActive)
            .Select(p => new PrescriptionDto(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.IsActive,
                p.Instructions, p.CreatedAt))
            .ToList();
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        await db.Pets.AnyAsync(p => p.Id == id, ct);

    private static PetDto MapToDto(Pet p) =>
        new(p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
            p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
            p.OwnerId, p.CreatedAt, p.UpdatedAt);
}
