using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService
{
    public async Task<PagedResult<OwnerDto>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Owners.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(o =>
                o.FirstName.ToLower().Contains(term) ||
                o.LastName.ToLower().Contains(term) ||
                o.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => MapToDto(o))
            .ToListAsync(ct);

        return new PagedResult<OwnerDto>(items, totalCount, page, pageSize);
    }

    public async Task<OwnerDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var owner = await db.Owners
            .Include(o => o.Pets.Where(p => p.IsActive))
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (owner is null)
        {
            return null;
        }

        return new OwnerDetailDto(
            owner.Id, owner.FirstName, owner.LastName, owner.Email, owner.Phone,
            owner.Address, owner.City, owner.State, owner.ZipCode,
            owner.CreatedAt, owner.UpdatedAt,
            owner.Pets.Select(p => new PetSummaryDto(p.Id, p.Name, p.Species, p.Breed, p.IsActive)).ToList());
    }

    public async Task<OwnerDto> CreateAsync(CreateOwnerDto dto, CancellationToken ct = default)
    {
        var owner = new Owner
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode
        };

        db.Owners.Add(owner);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created owner {OwnerId}: {FirstName} {LastName}", owner.Id, owner.FirstName, owner.LastName);
        return MapToDto(owner);
    }

    public async Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerDto dto, CancellationToken ct = default)
    {
        var owner = await db.Owners.FindAsync([id], ct);
        if (owner is null)
        {
            return null;
        }

        owner.FirstName = dto.FirstName;
        owner.LastName = dto.LastName;
        owner.Email = dto.Email;
        owner.Phone = dto.Phone;
        owner.Address = dto.Address;
        owner.City = dto.City;
        owner.State = dto.State;
        owner.ZipCode = dto.ZipCode;
        owner.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated owner {OwnerId}", owner.Id);
        return MapToDto(owner);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var owner = await db.Owners.FindAsync([id], ct);
        if (owner is null)
        {
            return false;
        }

        db.Owners.Remove(owner);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted owner {OwnerId}", id);
        return true;
    }

    public async Task<bool> HasActivePetsAsync(int id, CancellationToken ct = default) =>
        await db.Pets.AnyAsync(p => p.OwnerId == id && p.IsActive, ct);

    public async Task<IReadOnlyList<PetSummaryDto>> GetPetsAsync(int ownerId, CancellationToken ct = default) =>
        await db.Pets
            .Where(p => p.OwnerId == ownerId && p.IsActive)
            .Select(p => new PetSummaryDto(p.Id, p.Name, p.Species, p.Breed, p.IsActive))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AppointmentDto>> GetAppointmentsAsync(int ownerId, CancellationToken ct = default) =>
        await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

    private static OwnerDto MapToDto(Owner o) =>
        new(o.Id, o.FirstName, o.LastName, o.Email, o.Phone,
            o.Address, o.City, o.State, o.ZipCode,
            o.CreatedAt, o.UpdatedAt);
}
