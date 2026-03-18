using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService
{
    public async Task<PagedResponse<OwnerSummaryResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Owners.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o =>
                o.FirstName.ToLower().Contains(s) ||
                o.LastName.ToLower().Contains(s) ||
                o.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OwnerSummaryResponse
            {
                Id = o.Id,
                FirstName = o.FirstName,
                LastName = o.LastName,
                Email = o.Email,
                Phone = o.Phone
            })
            .ToListAsync(ct);

        return new PagedResponse<OwnerSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<OwnerResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var owner = await db.Owners.AsNoTracking()
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (owner is null) return null;

        return MapToResponse(owner);
    }

    public async Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken ct)
    {
        var existing = await db.Owners.AsNoTracking().AnyAsync(o => o.Email == request.Email, ct);
        if (existing)
            throw new InvalidOperationException($"An owner with email '{request.Email}' already exists.");

        var owner = new Owner
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Owners.Add(owner);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created owner {OwnerId}", owner.Id);

        return MapToResponse(owner);
    }

    public async Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct)
    {
        var owner = await db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (owner is null) return null;

        var emailConflict = await db.Owners.AsNoTracking().AnyAsync(o => o.Email == request.Email && o.Id != id, ct);
        if (emailConflict)
            throw new InvalidOperationException($"An owner with email '{request.Email}' already exists.");

        owner.FirstName = request.FirstName;
        owner.LastName = request.LastName;
        owner.Email = request.Email;
        owner.Phone = request.Phone;
        owner.Address = request.Address;
        owner.City = request.City;
        owner.State = request.State;
        owner.ZipCode = request.ZipCode;
        owner.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated owner {OwnerId}", owner.Id);

        return MapToResponse(owner);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var owner = await db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (owner is null)
            throw new KeyNotFoundException($"Owner with ID {id} not found.");

        if (owner.Pets.Any(p => p.IsActive))
            throw new InvalidOperationException("Cannot delete owner with active pets. Deactivate all pets first.");

        db.Owners.Remove(owner);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted owner {OwnerId}", owner.Id);

        return true;
    }

    public async Task<PagedResponse<PetSummaryResponse>> GetOwnerPetsAsync(int ownerId, CancellationToken ct)
    {
        var ownerExists = await db.Owners.AsNoTracking().AnyAsync(o => o.Id == ownerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        var pets = await db.Pets.AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .OrderBy(p => p.Name)
            .Select(p => new PetSummaryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                IsActive = p.IsActive
            })
            .ToListAsync(ct);

        return new PagedResponse<PetSummaryResponse>
        {
            Items = pets,
            Page = 1,
            PageSize = pets.Count,
            TotalCount = pets.Count,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };
    }

    public async Task<PagedResponse<AppointmentResponse>> GetOwnerAppointmentsAsync(int ownerId, int page, int pageSize, CancellationToken ct)
    {
        var ownerExists = await db.Owners.AsNoTracking().AnyAsync(o => o.Id == ownerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        var query = db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<AppointmentResponse>
        {
            Items = items.Select(MapAppointmentToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    private static OwnerResponse MapToResponse(Owner owner) => new()
    {
        Id = owner.Id,
        FirstName = owner.FirstName,
        LastName = owner.LastName,
        Email = owner.Email,
        Phone = owner.Phone,
        Address = owner.Address,
        City = owner.City,
        State = owner.State,
        ZipCode = owner.ZipCode,
        CreatedAt = owner.CreatedAt,
        UpdatedAt = owner.UpdatedAt,
        Pets = owner.Pets.Select(p => new PetSummaryResponse
        {
            Id = p.Id,
            Name = p.Name,
            Species = p.Species,
            Breed = p.Breed,
            IsActive = p.IsActive
        }).ToList()
    };

    private static AppointmentResponse MapAppointmentToResponse(Appointment a) => new()
    {
        Id = a.Id,
        PetId = a.PetId,
        VeterinarianId = a.VeterinarianId,
        AppointmentDate = a.AppointmentDate,
        DurationMinutes = a.DurationMinutes,
        Status = a.Status,
        Reason = a.Reason,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        Pet = a.Pet != null ? new PetSummaryResponse
        {
            Id = a.Pet.Id,
            Name = a.Pet.Name,
            Species = a.Pet.Species,
            Breed = a.Pet.Breed,
            IsActive = a.Pet.IsActive
        } : null,
        Veterinarian = a.Veterinarian != null ? new VeterinarianSummaryResponse
        {
            Id = a.Veterinarian.Id,
            FirstName = a.Veterinarian.FirstName,
            LastName = a.Veterinarian.LastName,
            Specialization = a.Veterinarian.Specialization
        } : null
    };
}
