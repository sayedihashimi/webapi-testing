using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PetService(VetClinicDbContext db, ILogger<PetService> logger) : IPetService
{
    private static readonly string[] ValidSpecies = ["Dog", "Cat", "Bird", "Rabbit"];

    public async Task<PagedResponse<PetResponse>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Pets.AsNoTracking().Include(p => p.Owner).AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species == species);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<PetResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<PetResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets.AsNoTracking()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return pet is null ? null : MapToResponse(pet);
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct)
    {
        if (!ValidSpecies.Contains(request.Species))
            throw new ArgumentException($"Invalid species '{request.Species}'. Valid values: {string.Join(", ", ValidSpecies)}");

        var ownerExists = await db.Owners.AsNoTracking().AnyAsync(o => o.Id == request.OwnerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");

        if (!string.IsNullOrWhiteSpace(request.MicrochipNumber))
        {
            var chipExists = await db.Pets.AsNoTracking().AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber, ct);
            if (chipExists)
                throw new InvalidOperationException($"A pet with microchip number '{request.MicrochipNumber}' already exists.");
        }

        var pet = new Pet
        {
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            DateOfBirth = request.DateOfBirth,
            Weight = request.Weight,
            Color = request.Color,
            MicrochipNumber = request.MicrochipNumber,
            OwnerId = request.OwnerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);

        // Reload with owner
        var created = await db.Pets.AsNoTracking().Include(p => p.Owner).FirstAsync(p => p.Id == pet.Id, ct);
        logger.LogInformation("Created pet {PetId} for owner {OwnerId}", pet.Id, pet.OwnerId);

        return MapToResponse(created);
    }

    public async Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct)
    {
        var pet = await db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pet is null) return null;

        if (!ValidSpecies.Contains(request.Species))
            throw new ArgumentException($"Invalid species '{request.Species}'. Valid values: {string.Join(", ", ValidSpecies)}");

        var ownerExists = await db.Owners.AsNoTracking().AnyAsync(o => o.Id == request.OwnerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");

        if (!string.IsNullOrWhiteSpace(request.MicrochipNumber))
        {
            var chipExists = await db.Pets.AsNoTracking().AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber && p.Id != id, ct);
            if (chipExists)
                throw new InvalidOperationException($"A pet with microchip number '{request.MicrochipNumber}' already exists.");
        }

        pet.Name = request.Name;
        pet.Species = request.Species;
        pet.Breed = request.Breed;
        pet.DateOfBirth = request.DateOfBirth;
        pet.Weight = request.Weight;
        pet.Color = request.Color;
        pet.MicrochipNumber = request.MicrochipNumber;
        pet.OwnerId = request.OwnerId;
        pet.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var updated = await db.Pets.AsNoTracking().Include(p => p.Owner).FirstAsync(p => p.Id == id, ct);
        logger.LogInformation("Updated pet {PetId}", pet.Id);

        return MapToResponse(updated);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pet is null)
            throw new KeyNotFoundException($"Pet with ID {id} not found.");

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Soft-deleted pet {PetId}", id);

        return true;
    }

    public async Task<PagedResponse<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, int page, int pageSize, CancellationToken ct)
    {
        var petExists = await db.Pets.AsNoTracking().AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var query = db.MedicalRecords.AsNoTracking()
            .Include(mr => mr.Pet)
            .Include(mr => mr.Veterinarian)
            .Include(mr => mr.Prescriptions)
            .Where(mr => mr.PetId == petId);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(mr => mr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<MedicalRecordResponse>
        {
            Items = items.Select(MapMedicalRecordToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<List<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AsNoTracking().AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var vaccinations = await db.Vaccinations.AsNoTracking()
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .ToListAsync(ct);

        return vaccinations.Select(MapVaccinationToResponse).ToList();
    }

    public async Task<List<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AsNoTracking().AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysFromNow = today.AddDays(30);

        var vaccinations = await db.Vaccinations.AsNoTracking()
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= thirtyDaysFromNow)
            .OrderBy(v => v.ExpirationDate)
            .ToListAsync(ct);

        return vaccinations.Select(MapVaccinationToResponse).ToList();
    }

    public async Task<List<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AsNoTracking().AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await db.Prescriptions.AsNoTracking()
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .ToListAsync(ct);

        return prescriptions.Select(MapPrescriptionToResponse).ToList();
    }

    private static PetResponse MapToResponse(Pet pet) => new()
    {
        Id = pet.Id,
        Name = pet.Name,
        Species = pet.Species,
        Breed = pet.Breed,
        DateOfBirth = pet.DateOfBirth,
        Weight = pet.Weight,
        Color = pet.Color,
        MicrochipNumber = pet.MicrochipNumber,
        IsActive = pet.IsActive,
        OwnerId = pet.OwnerId,
        CreatedAt = pet.CreatedAt,
        UpdatedAt = pet.UpdatedAt,
        Owner = pet.Owner != null ? new OwnerSummaryResponse
        {
            Id = pet.Owner.Id,
            FirstName = pet.Owner.FirstName,
            LastName = pet.Owner.LastName,
            Email = pet.Owner.Email,
            Phone = pet.Owner.Phone
        } : null
    };

    private static MedicalRecordResponse MapMedicalRecordToResponse(MedicalRecord mr) => new()
    {
        Id = mr.Id,
        AppointmentId = mr.AppointmentId,
        PetId = mr.PetId,
        VeterinarianId = mr.VeterinarianId,
        Diagnosis = mr.Diagnosis,
        Treatment = mr.Treatment,
        Notes = mr.Notes,
        FollowUpDate = mr.FollowUpDate,
        CreatedAt = mr.CreatedAt,
        Pet = mr.Pet != null ? new PetSummaryResponse
        {
            Id = mr.Pet.Id,
            Name = mr.Pet.Name,
            Species = mr.Pet.Species,
            Breed = mr.Pet.Breed,
            IsActive = mr.Pet.IsActive
        } : null,
        Veterinarian = mr.Veterinarian != null ? new VeterinarianSummaryResponse
        {
            Id = mr.Veterinarian.Id,
            FirstName = mr.Veterinarian.FirstName,
            LastName = mr.Veterinarian.LastName,
            Specialization = mr.Veterinarian.Specialization
        } : null,
        Prescriptions = mr.Prescriptions.Select(MapPrescriptionToResponse).ToList()
    };

    private static PrescriptionResponse MapPrescriptionToResponse(Prescription p) => new()
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

    private static VaccinationResponse MapVaccinationToResponse(Vaccination v)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new()
        {
            Id = v.Id,
            PetId = v.PetId,
            VaccineName = v.VaccineName,
            DateAdministered = v.DateAdministered,
            ExpirationDate = v.ExpirationDate,
            BatchNumber = v.BatchNumber,
            AdministeredByVetId = v.AdministeredByVetId,
            Notes = v.Notes,
            IsExpired = v.ExpirationDate < today,
            IsDueSoon = v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
            CreatedAt = v.CreatedAt,
            AdministeredByVet = v.AdministeredByVet != null ? new VeterinarianSummaryResponse
            {
                Id = v.AdministeredByVet.Id,
                FirstName = v.AdministeredByVet.FirstName,
                LastName = v.AdministeredByVet.LastName,
                Specialization = v.AdministeredByVet.Specialization
            } : null
        };
    }
}
