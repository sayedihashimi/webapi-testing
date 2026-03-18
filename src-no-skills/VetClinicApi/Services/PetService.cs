using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PetService : IPetService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<PetService> _logger;

    public PetService(VetClinicDbContext db, ILogger<PetService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<PetResponseDto>> GetAllAsync(string? search, string? species, bool includeInactive, PaginationParams pagination)
    {
        var query = _db.Pets.AsQueryable();
        if (!includeInactive)
            query = query.Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }
        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species.ToLower() == species.ToLower());
        query = query.OrderBy(p => p.Name);
        return await query.Select(p => p.ToResponseDto()).ToPagedResultAsync(pagination);
    }

    public async Task<PetDetailDto?> GetByIdAsync(int id)
    {
        var pet = await _db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id);
        return pet?.ToDetailDto();
    }

    public async Task<PetResponseDto> CreateAsync(PetCreateDto dto)
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
            OwnerId = dto.OwnerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Pet created: {PetId} {Name}", pet.Id, pet.Name);
        return pet.ToResponseDto();
    }

    public async Task<PetResponseDto?> UpdateAsync(int id, PetUpdateDto dto)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet is null) return null;
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
        return pet.ToResponseDto();
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet is null) return false;
        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Pet soft-deleted: {PetId}", id);
        return true;
    }

    public async Task<IEnumerable<MedicalRecordResponseDto>> GetMedicalRecordsAsync(int petId)
    {
        return await _db.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => m.ToResponseDto())
            .ToListAsync();
    }

    public async Task<IEnumerable<VaccinationResponseDto>> GetVaccinationsAsync(int petId)
    {
        return await _db.Vaccinations
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => v.ToResponseDto())
            .ToListAsync();
    }

    public async Task<IEnumerable<VaccinationResponseDto>> GetUpcomingVaccinationsAsync(int petId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonDate = today.AddDays(30);
        return await _db.Vaccinations
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= dueSoonDate)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => v.ToResponseDto())
            .ToListAsync();
    }

    public async Task<IEnumerable<PrescriptionResponseDto>> GetActivePrescriptionsAsync(int petId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _db.Prescriptions
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => p.ToResponseDto())
            .ToListAsync();
    }
}
