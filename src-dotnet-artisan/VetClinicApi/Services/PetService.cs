using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PetService(VetClinicDbContext db, ILogger<PetService> logger) : IPetService
{
    private static readonly string[] ValidSpecies = ["Dog", "Cat", "Bird", "Rabbit"];

    public async Task<PagedResult<PetDto>> GetAllAsync(string? search, string? species, bool includeInactive, PaginationParams pagination)
    {
        var query = db.Pets.AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species == species);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip(pagination.Skip).Take(pagination.PageSize)
            .Select(p => new PetDto(p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight, p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync();

        return new PagedResult<PetDto>(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task<PetDetailDto?> GetByIdAsync(int id)
    {
        return await db.Pets
            .Include(p => p.Owner)
            .Where(p => p.Id == id)
            .Select(p => new PetDetailDto(p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight, p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId,
                p.Owner.FirstName + " " + p.Owner.LastName, p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<PetDto> CreateAsync(CreatePetDto dto)
    {
        ValidateSpecies(dto.Species);
        if (!await db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new InvalidOperationException("Owner not found.");

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
        await db.SaveChangesAsync();
        logger.LogInformation("Created pet {PetId} ({Name})", pet.Id, pet.Name);

        return new PetDto(pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive, pet.OwnerId, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<PetDto?> UpdateAsync(int id, UpdatePetDto dto)
    {
        ValidateSpecies(dto.Species);
        var pet = await db.Pets.FindAsync(id);
        if (pet is null) return null;

        if (!await db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new InvalidOperationException("Owner not found.");

        pet.Name = dto.Name;
        pet.Species = dto.Species;
        pet.Breed = dto.Breed;
        pet.DateOfBirth = dto.DateOfBirth;
        pet.Weight = dto.Weight;
        pet.Color = dto.Color;
        pet.MicrochipNumber = dto.MicrochipNumber;
        pet.OwnerId = dto.OwnerId;
        pet.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated pet {PetId}", pet.Id);
        return new PetDto(pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive, pet.OwnerId, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var pet = await db.Pets.FindAsync(id);
        if (pet is null) return false;

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Soft-deleted pet {PetId}", id);
        return true;
    }

    public async Task<IReadOnlyList<MedicalRecordDto>> GetMedicalRecordsAsync(int petId)
    {
        return await db.MedicalRecords
            .Include(m => m.Veterinarian)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordDto(m.Id, m.AppointmentId, m.PetId, m.Pet.Name, m.VeterinarianId,
                m.Veterinarian.FirstName + " " + m.Veterinarian.LastName,
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<VaccinationDto>> GetVaccinationsAsync(int petId)
    {
        return await db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => MapVaccinationDto(v))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var soon = today.AddDays(30);

        return await db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= soon)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => MapVaccinationDto(v))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PrescriptionDto>> GetActivePrescriptionsAsync(int petId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await db.Prescriptions
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId)
            .ToListAsync();

        return prescriptions
            .Where(p => p.IsActive)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionDto(p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays, p.StartDate, p.EndDate, p.IsActive, p.Instructions, p.CreatedAt))
            .ToList();
    }

    private static void ValidateSpecies(string species)
    {
        if (!ValidSpecies.Contains(species, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Invalid species '{species}'. Valid values: {string.Join(", ", ValidSpecies)}");
    }

    private static VaccinationDto MapVaccinationDto(Vaccination v) => new(
        v.Id, v.PetId, v.Pet.Name, v.VaccineName, v.DateAdministered, v.ExpirationDate,
        v.BatchNumber, v.AdministeredByVetId,
        v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
        v.Notes, v.IsExpired, v.IsDueSoon, v.CreatedAt);
}
