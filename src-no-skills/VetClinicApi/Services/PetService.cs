using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PaginatedResponse<PetDto>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize);
    Task<PetDto> GetByIdAsync(int id);
    Task<PetDto> CreateAsync(CreatePetDto dto);
    Task<PetDto> UpdateAsync(int id, UpdatePetDto dto);
    Task DeleteAsync(int id);
    Task<List<MedicalRecordDto>> GetMedicalRecordsAsync(int petId);
    Task<List<VaccinationDto>> GetVaccinationsAsync(int petId);
    Task<List<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId);
    Task<List<PrescriptionDto>> GetActivePrescriptionsAsync(int petId);
}

public class PetService : IPetService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<PetService> _logger;

    public PetService(VetClinicDbContext db, ILogger<PetService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResponse<PetDto>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize)
    {
        var query = _db.Pets.Include(p => p.Owner).AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species.ToLower() == species.ToLower());

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<PetDto>
        {
            Data = items.Select(MapToDto),
            Page = page, PageSize = pageSize, TotalCount = totalCount
        };
    }

    public async Task<PetDto> GetByIdAsync(int id)
    {
        var pet = await _db.Pets.Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Pet", id);
        return MapToDto(pet);
    }

    public async Task<PetDto> CreateAsync(CreatePetDto dto)
    {
        ValidateSpecies(dto.Species);

        if (!await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new NotFoundException("Owner", dto.OwnerId);

        if (!string.IsNullOrWhiteSpace(dto.MicrochipNumber) &&
            await _db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber))
            throw new BusinessRuleException("A pet with this microchip number already exists.", StatusCodes.Status409Conflict);

        var pet = new Pet
        {
            Name = dto.Name, Species = dto.Species, Breed = dto.Breed,
            DateOfBirth = dto.DateOfBirth, Weight = dto.Weight,
            Color = dto.Color, MicrochipNumber = dto.MicrochipNumber,
            OwnerId = dto.OwnerId
        };

        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();

        // Reload with owner
        await _db.Entry(pet).Reference(p => p.Owner).LoadAsync();
        _logger.LogInformation("Created pet {PetId}: {Name}", pet.Id, pet.Name);
        return MapToDto(pet);
    }

    public async Task<PetDto> UpdateAsync(int id, UpdatePetDto dto)
    {
        ValidateSpecies(dto.Species);

        var pet = await _db.Pets.Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Pet", id);

        if (!await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new NotFoundException("Owner", dto.OwnerId);

        if (!string.IsNullOrWhiteSpace(dto.MicrochipNumber) &&
            await _db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber && p.Id != id))
            throw new BusinessRuleException("A pet with this microchip number already exists.", StatusCodes.Status409Conflict);

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

        // Reload owner if changed
        await _db.Entry(pet).Reference(p => p.Owner).LoadAsync();
        _logger.LogInformation("Updated pet {PetId}", pet.Id);
        return MapToDto(pet);
    }

    public async Task DeleteAsync(int id)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Pet", id);

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Soft-deleted pet {PetId}", pet.Id);
    }

    public async Task<List<MedicalRecordDto>> GetMedicalRecordsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new NotFoundException("Pet", petId);

        var records = await _db.MedicalRecords
            .Include(m => m.Prescriptions)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return records.Select(r => new MedicalRecordDto
        {
            Id = r.Id, AppointmentId = r.AppointmentId, PetId = r.PetId,
            VeterinarianId = r.VeterinarianId, Diagnosis = r.Diagnosis,
            Treatment = r.Treatment, Notes = r.Notes, FollowUpDate = r.FollowUpDate,
            CreatedAt = r.CreatedAt,
            Prescriptions = r.Prescriptions.Select(MapPrescription).ToList()
        }).ToList();
    }

    public async Task<List<VaccinationDto>> GetVaccinationsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new NotFoundException("Pet", petId);

        var vaccinations = await _db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .ToListAsync();

        return vaccinations.Select(MapVaccination).ToList();
    }

    public async Task<List<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new NotFoundException("Pet", petId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonDate = today.AddDays(30);

        var vaccinations = await _db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= dueSoonDate)
            .OrderBy(v => v.ExpirationDate)
            .ToListAsync();

        return vaccinations.Select(MapVaccination).ToList();
    }

    public async Task<List<PrescriptionDto>> GetActivePrescriptionsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new NotFoundException("Pet", petId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await _db.Prescriptions
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId)
            .ToListAsync();

        // Filter in memory since EndDate and IsActive are computed
        return prescriptions
            .Where(p => p.IsActive)
            .Select(MapPrescription)
            .ToList();
    }

    private static void ValidateSpecies(string species)
    {
        var valid = new[] { "Dog", "Cat", "Bird", "Rabbit" };
        if (!valid.Contains(species, StringComparer.OrdinalIgnoreCase))
            throw new BusinessRuleException($"Invalid species. Must be one of: {string.Join(", ", valid)}");
    }

    private static PetDto MapToDto(Pet pet) => new()
    {
        Id = pet.Id, Name = pet.Name, Species = pet.Species, Breed = pet.Breed,
        DateOfBirth = pet.DateOfBirth, Weight = pet.Weight, Color = pet.Color,
        MicrochipNumber = pet.MicrochipNumber, IsActive = pet.IsActive,
        OwnerId = pet.OwnerId, CreatedAt = pet.CreatedAt, UpdatedAt = pet.UpdatedAt,
        Owner = pet.Owner != null ? new OwnerSummaryDto
        {
            Id = pet.Owner.Id, FirstName = pet.Owner.FirstName,
            LastName = pet.Owner.LastName, Email = pet.Owner.Email, Phone = pet.Owner.Phone
        } : null
    };

    private static PrescriptionDto MapPrescription(Prescription p) => new()
    {
        Id = p.Id, MedicalRecordId = p.MedicalRecordId, MedicationName = p.MedicationName,
        Dosage = p.Dosage, DurationDays = p.DurationDays, StartDate = p.StartDate,
        EndDate = p.EndDate, Instructions = p.Instructions, IsActive = p.IsActive,
        CreatedAt = p.CreatedAt
    };

    private static VaccinationDto MapVaccination(Vaccination v) => new()
    {
        Id = v.Id, PetId = v.PetId, VaccineName = v.VaccineName,
        DateAdministered = v.DateAdministered, ExpirationDate = v.ExpirationDate,
        BatchNumber = v.BatchNumber, AdministeredByVetId = v.AdministeredByVetId,
        AdministeredByVet = v.AdministeredByVet != null ? new VeterinarianSummaryDto
        {
            Id = v.AdministeredByVet.Id, FirstName = v.AdministeredByVet.FirstName,
            LastName = v.AdministeredByVet.LastName, Specialization = v.AdministeredByVet.Specialization
        } : null,
        Notes = v.Notes, IsExpired = v.IsExpired, IsDueSoon = v.IsDueSoon,
        CreatedAt = v.CreatedAt
    };
}
