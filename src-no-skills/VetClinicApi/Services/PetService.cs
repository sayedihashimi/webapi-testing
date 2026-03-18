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

    public async Task<PagedResponse<PetResponseDto>> GetAllAsync(string? search, string? species, bool includeInactive, PaginationParams pagination)
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
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResponse<PetResponseDto>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<PetResponseDto?> GetByIdAsync(int id)
    {
        var pet = await _db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id);
        return pet == null ? null : MapToResponse(pet);
    }

    public async Task<PetResponseDto> CreateAsync(CreatePetDto dto)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new KeyNotFoundException($"Owner with ID {dto.OwnerId} not found.");

        if (!string.IsNullOrEmpty(dto.MicrochipNumber) && await _db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber))
            throw new ArgumentException($"A pet with microchip number '{dto.MicrochipNumber}' already exists.");

        var pet = new Pet
        {
            Name = dto.Name, Species = dto.Species, Breed = dto.Breed,
            DateOfBirth = dto.DateOfBirth, Weight = dto.Weight, Color = dto.Color,
            MicrochipNumber = dto.MicrochipNumber, OwnerId = dto.OwnerId,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Pet created: {PetId} {Name}", pet.Id, pet.Name);

        return await GetByIdAsync(pet.Id) ?? MapToResponse(pet);
    }

    public async Task<PetResponseDto> UpdateAsync(int id, UpdatePetDto dto)
    {
        var pet = await _db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found.");

        if (!await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new KeyNotFoundException($"Owner with ID {dto.OwnerId} not found.");

        if (!string.IsNullOrEmpty(dto.MicrochipNumber) && await _db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber && p.Id != id))
            throw new ArgumentException($"A pet with microchip number '{dto.MicrochipNumber}' already exists.");

        pet.Name = dto.Name; pet.Species = dto.Species; pet.Breed = dto.Breed;
        pet.DateOfBirth = dto.DateOfBirth; pet.Weight = dto.Weight; pet.Color = dto.Color;
        pet.MicrochipNumber = dto.MicrochipNumber; pet.OwnerId = dto.OwnerId;
        pet.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(pet);
    }

    public async Task DeleteAsync(int id)
    {
        var pet = await _db.Pets.FindAsync(id) ?? throw new KeyNotFoundException($"Pet with ID {id} not found.");
        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Pet soft-deleted: {PetId}", id);
    }

    public async Task<List<MedicalRecordResponseDto>> GetMedicalRecordsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var records = await _db.MedicalRecords
            .Include(m => m.Pet).Include(m => m.Veterinarian).Include(m => m.Prescriptions)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return records.Select(MedicalRecordService.MapToResponse).ToList();
    }

    public async Task<List<VaccinationResponseDto>> GetVaccinationsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var vaccinations = await _db.Vaccinations
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .ToListAsync();

        return vaccinations.Select(VaccinationService.MapToResponse).ToList();
    }

    public async Task<List<VaccinationResponseDto>> GetUpcomingVaccinationsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var threshold = today.AddDays(30);

        var vaccinations = await _db.Vaccinations
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= threshold)
            .OrderBy(v => v.ExpirationDate)
            .ToListAsync();

        return vaccinations.Select(VaccinationService.MapToResponse).ToList();
    }

    public async Task<List<PrescriptionResponseDto>> GetActivePrescriptionsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await _db.Prescriptions
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .ToListAsync();

        return prescriptions.Select(PrescriptionService.MapToResponse).ToList();
    }

    public static PetResponseDto MapToResponse(Pet p) => new()
    {
        Id = p.Id, Name = p.Name, Species = p.Species, Breed = p.Breed,
        DateOfBirth = p.DateOfBirth, Weight = p.Weight, Color = p.Color,
        MicrochipNumber = p.MicrochipNumber, IsActive = p.IsActive, OwnerId = p.OwnerId,
        Owner = p.Owner != null ? new OwnerSummaryDto { Id = p.Owner.Id, FirstName = p.Owner.FirstName, LastName = p.Owner.LastName, Email = p.Owner.Email, Phone = p.Owner.Phone } : null,
        CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };
}
