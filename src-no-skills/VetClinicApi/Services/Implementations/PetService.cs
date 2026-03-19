using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs.Common;
using VetClinicApi.DTOs.MedicalRecord;
using VetClinicApi.DTOs.Pet;
using VetClinicApi.DTOs.Prescription;
using VetClinicApi.DTOs.Vaccination;
using VetClinicApi.Models;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Services.Implementations;

public class PetService : IPetService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<PetService> _logger;

    public PetService(VetClinicDbContext db, ILogger<PetService> logger)
    {
        _db = db;
        _logger = logger;
    }

    private static readonly string[] ValidSpecies = ["Dog", "Cat", "Bird", "Rabbit"];

    public async Task<PagedResult<PetDto>> GetAllAsync(string? search, bool includeInactive, PaginationParams pagination)
    {
        var query = _db.Pets.Include(p => p.Owner).AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(s) ||
                p.Species.ToLower().Contains(s) ||
                (p.Breed != null && p.Breed.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return new PagedResult<PetDto> { Items = items, TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize };
    }

    public async Task<PetDetailDto> GetByIdAsync(int id)
    {
        var pet = await _db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found");

        return new PetDetailDto
        {
            Id = pet.Id, Name = pet.Name, Species = pet.Species, Breed = pet.Breed,
            DateOfBirth = pet.DateOfBirth, Weight = pet.Weight, Color = pet.Color,
            MicrochipNumber = pet.MicrochipNumber, IsActive = pet.IsActive,
            OwnerId = pet.OwnerId, OwnerName = pet.Owner.FirstName + " " + pet.Owner.LastName,
            CreatedAt = pet.CreatedAt, UpdatedAt = pet.UpdatedAt,
            Owner = new OwnerSummaryDto
            {
                Id = pet.Owner.Id, FirstName = pet.Owner.FirstName, LastName = pet.Owner.LastName,
                Email = pet.Owner.Email, Phone = pet.Owner.Phone
            }
        };
    }

    public async Task<PetDto> CreateAsync(CreatePetDto dto)
    {
        if (!ValidSpecies.Contains(dto.Species))
            throw new ArgumentException($"Invalid species '{dto.Species}'. Valid options: {string.Join(", ", ValidSpecies)}");

        if (!await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new KeyNotFoundException($"Owner with ID {dto.OwnerId} not found");

        if (!string.IsNullOrEmpty(dto.MicrochipNumber) && await _db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber))
            throw new ArgumentException($"A pet with microchip number '{dto.MicrochipNumber}' already exists");

        var pet = new Pet
        {
            Name = dto.Name, Species = dto.Species, Breed = dto.Breed,
            DateOfBirth = dto.DateOfBirth, Weight = dto.Weight, Color = dto.Color,
            MicrochipNumber = dto.MicrochipNumber, OwnerId = dto.OwnerId
        };

        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();

        // Reload with owner for DTO
        await _db.Entry(pet).Reference(p => p.Owner).LoadAsync();
        _logger.LogInformation("Created pet {PetId}: {PetName}", pet.Id, pet.Name);
        return MapToDto(pet);
    }

    public async Task<PetDto> UpdateAsync(int id, UpdatePetDto dto)
    {
        var pet = await _db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found");

        if (!ValidSpecies.Contains(dto.Species))
            throw new ArgumentException($"Invalid species '{dto.Species}'. Valid options: {string.Join(", ", ValidSpecies)}");

        if (!await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new KeyNotFoundException($"Owner with ID {dto.OwnerId} not found");

        if (!string.IsNullOrEmpty(dto.MicrochipNumber) && await _db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber && p.Id != id))
            throw new ArgumentException($"A pet with microchip number '{dto.MicrochipNumber}' already exists");

        pet.Name = dto.Name;
        pet.Species = dto.Species;
        pet.Breed = dto.Breed;
        pet.DateOfBirth = dto.DateOfBirth;
        pet.Weight = dto.Weight;
        pet.Color = dto.Color;
        pet.MicrochipNumber = dto.MicrochipNumber;
        pet.OwnerId = dto.OwnerId;
        pet.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(pet).Reference(p => p.Owner).LoadAsync();
        _logger.LogInformation("Updated pet {PetId}", id);
        return MapToDto(pet);
    }

    public async Task DeleteAsync(int id)
    {
        var pet = await _db.Pets.FindAsync(id)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found");

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Soft-deleted pet {PetId}", id);
    }

    public async Task<List<MedicalRecordDto>> GetMedicalRecordsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found");

        return await _db.MedicalRecords
            .Include(m => m.Prescriptions)
            .Include(m => m.Veterinarian)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MapMedicalRecordToDto(m))
            .ToListAsync();
    }

    public async Task<List<VaccinationDto>> GetVaccinationsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found");

        return await _db.Vaccinations
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => MapVaccinationToDto(v))
            .ToListAsync();
    }

    public async Task<List<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var soon = today.AddDays(30);

        return await _db.Vaccinations
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= soon)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => MapVaccinationToDto(v))
            .ToListAsync();
    }

    public async Task<List<PrescriptionDto>> GetActivePrescriptionsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await _db.Prescriptions
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId)
            .ToListAsync();

        return prescriptions
            .Where(p => p.IsActive)
            .Select(MapPrescriptionToDto)
            .ToList();
    }

    private static PetDto MapToDto(Pet p) => new()
    {
        Id = p.Id, Name = p.Name, Species = p.Species, Breed = p.Breed,
        DateOfBirth = p.DateOfBirth, Weight = p.Weight, Color = p.Color,
        MicrochipNumber = p.MicrochipNumber, IsActive = p.IsActive,
        OwnerId = p.OwnerId, OwnerName = p.Owner.FirstName + " " + p.Owner.LastName,
        CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };

    private static MedicalRecordDto MapMedicalRecordToDto(MedicalRecord m) => new()
    {
        Id = m.Id, AppointmentId = m.AppointmentId, PetId = m.PetId,
        PetName = string.Empty, // Not needed in nested context
        VeterinarianId = m.VeterinarianId,
        VeterinarianName = m.Veterinarian.FirstName + " " + m.Veterinarian.LastName,
        Diagnosis = m.Diagnosis, Treatment = m.Treatment, Notes = m.Notes,
        FollowUpDate = m.FollowUpDate, CreatedAt = m.CreatedAt,
        Prescriptions = m.Prescriptions.Select(p => new PrescriptionSummaryDto
        {
            Id = p.Id, MedicationName = p.MedicationName, Dosage = p.Dosage, IsActive = p.IsActive
        }).ToList()
    };

    private static VaccinationDto MapVaccinationToDto(Vaccination v) => new()
    {
        Id = v.Id, PetId = v.PetId, PetName = v.Pet.Name,
        VaccineName = v.VaccineName, DateAdministered = v.DateAdministered,
        ExpirationDate = v.ExpirationDate, BatchNumber = v.BatchNumber,
        AdministeredByVetId = v.AdministeredByVetId,
        AdministeredByVetName = v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
        Notes = v.Notes, IsExpired = v.IsExpired, IsDueSoon = v.IsDueSoon, CreatedAt = v.CreatedAt
    };

    private static PrescriptionDto MapPrescriptionToDto(Prescription p) => new()
    {
        Id = p.Id, MedicalRecordId = p.MedicalRecordId,
        MedicationName = p.MedicationName, Dosage = p.Dosage,
        DurationDays = p.DurationDays, StartDate = p.StartDate,
        EndDate = p.EndDate, Instructions = p.Instructions,
        IsActive = p.IsActive, CreatedAt = p.CreatedAt
    };
}
