using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PetService : IPetService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<PetService> _logger;

    public PetService(VetClinicDbContext context, ILogger<PetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<PetResponseDto>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize)
    {
        var query = _context.Pets.Include(p => p.Owner).AsQueryable();

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

        return new PaginatedResponse<PetResponseDto>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<PetResponseDto> GetByIdAsync(int id)
    {
        var pet = await _context.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found.");
        return MapToResponse(pet);
    }

    public async Task<PetResponseDto> CreateAsync(CreatePetDto dto)
    {
        if (!await _context.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new BusinessRuleException($"Owner with ID {dto.OwnerId} not found.");

        if (!string.IsNullOrWhiteSpace(dto.MicrochipNumber) &&
            await _context.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber))
            throw new BusinessRuleException("A pet with this microchip number already exists.", 409, "Conflict");

        var pet = new Pet
        {
            Name = dto.Name, Species = dto.Species, Breed = dto.Breed,
            DateOfBirth = dto.DateOfBirth, Weight = dto.Weight, Color = dto.Color,
            MicrochipNumber = dto.MicrochipNumber, OwnerId = dto.OwnerId,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Pet created: {PetId} {Name}", pet.Id, pet.Name);

        await _context.Entry(pet).Reference(p => p.Owner).LoadAsync();
        return MapToResponse(pet);
    }

    public async Task<PetResponseDto> UpdateAsync(int id, UpdatePetDto dto)
    {
        var pet = await _context.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found.");

        if (!await _context.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new BusinessRuleException($"Owner with ID {dto.OwnerId} not found.");

        if (!string.IsNullOrWhiteSpace(dto.MicrochipNumber) &&
            await _context.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber && p.Id != id))
            throw new BusinessRuleException("A pet with this microchip number already exists.", 409, "Conflict");

        pet.Name = dto.Name;
        pet.Species = dto.Species;
        pet.Breed = dto.Breed;
        pet.DateOfBirth = dto.DateOfBirth;
        pet.Weight = dto.Weight;
        pet.Color = dto.Color;
        pet.MicrochipNumber = dto.MicrochipNumber;
        pet.OwnerId = dto.OwnerId;
        pet.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _context.Entry(pet).Reference(p => p.Owner).LoadAsync();
        return MapToResponse(pet);
    }

    public async Task DeleteAsync(int id)
    {
        var pet = await _context.Pets.FindAsync(id)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found.");

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Pet soft-deleted: {PetId}", id);
    }

    public async Task<List<MedicalRecordResponseDto>> GetMedicalRecordsAsync(int petId)
    {
        if (!await _context.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        return await _context.MedicalRecords
            .Include(m => m.Pet).Include(m => m.Veterinarian).Include(m => m.Prescriptions)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MedicalRecordService.MapToResponse(m))
            .ToListAsync();
    }

    public async Task<List<VaccinationResponseDto>> GetVaccinationsAsync(int petId)
    {
        if (!await _context.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var vaccinations = await _context.Vaccinations
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .ToListAsync();

        return vaccinations.Select(VaccinationService.MapToResponse).ToList();
    }

    public async Task<List<VaccinationResponseDto>> GetUpcomingVaccinationsAsync(int petId)
    {
        if (!await _context.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var soonDate = today.AddDays(30);

        var vaccinations = await _context.Vaccinations
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= soonDate)
            .OrderBy(v => v.ExpirationDate)
            .ToListAsync();

        return vaccinations.Select(VaccinationService.MapToResponse).ToList();
    }

    public async Task<List<PrescriptionResponseDto>> GetActivePrescriptionsAsync(int petId)
    {
        if (!await _context.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _context.Prescriptions
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionResponseDto
            {
                Id = p.Id, MedicalRecordId = p.MedicalRecordId,
                MedicationName = p.MedicationName, Dosage = p.Dosage,
                DurationDays = p.DurationDays, StartDate = p.StartDate, EndDate = p.EndDate,
                Instructions = p.Instructions, IsActive = p.EndDate >= today, CreatedAt = p.CreatedAt
            }).ToListAsync();
    }

    private static PetResponseDto MapToResponse(Pet pet) => new()
    {
        Id = pet.Id, Name = pet.Name, Species = pet.Species, Breed = pet.Breed,
        DateOfBirth = pet.DateOfBirth, Weight = pet.Weight, Color = pet.Color,
        MicrochipNumber = pet.MicrochipNumber, IsActive = pet.IsActive,
        OwnerId = pet.OwnerId, CreatedAt = pet.CreatedAt, UpdatedAt = pet.UpdatedAt,
        Owner = pet.Owner != null ? new OwnerSummaryDto
        {
            Id = pet.Owner.Id, FirstName = pet.Owner.FirstName, LastName = pet.Owner.LastName,
            Email = pet.Owner.Email, Phone = pet.Owner.Phone
        } : null
    };
}
