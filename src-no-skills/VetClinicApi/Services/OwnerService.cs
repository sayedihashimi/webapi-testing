using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class OwnerService : IOwnerService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<OwnerService> _logger;

    public OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<OwnerResponseDto>> GetAllAsync(string? search, PaginationParams pagination)
    {
        var query = _db.Owners.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o => o.FirstName.ToLower().Contains(s) || o.LastName.ToLower().Contains(s) || o.Email.ToLower().Contains(s));
        }
        query = query.OrderBy(o => o.LastName).ThenBy(o => o.FirstName);
        return await query.Select(o => o.ToResponseDto()).ToPagedResultAsync(pagination);
    }

    public async Task<OwnerDetailDto?> GetByIdAsync(int id)
    {
        var owner = await _db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id);
        return owner?.ToDetailDto();
    }

    public async Task<OwnerResponseDto> CreateAsync(OwnerCreateDto dto)
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
            ZipCode = dto.ZipCode,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Owners.Add(owner);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Owner created: {OwnerId} {Name}", owner.Id, $"{owner.FirstName} {owner.LastName}");
        return owner.ToResponseDto();
    }

    public async Task<OwnerResponseDto?> UpdateAsync(int id, OwnerUpdateDto dto)
    {
        var owner = await _db.Owners.FindAsync(id);
        if (owner is null) return null;
        owner.FirstName = dto.FirstName;
        owner.LastName = dto.LastName;
        owner.Email = dto.Email;
        owner.Phone = dto.Phone;
        owner.Address = dto.Address;
        owner.City = dto.City;
        owner.State = dto.State;
        owner.ZipCode = dto.ZipCode;
        owner.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return owner.ToResponseDto();
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var owner = await _db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id);
        if (owner is null) return (false, "Owner not found");
        if (owner.Pets.Any(p => p.IsActive))
            return (false, "Cannot delete owner with active pets. Deactivate all pets first.");
        _db.Owners.Remove(owner);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Owner deleted: {OwnerId}", id);
        return (true, null);
    }

    public async Task<IEnumerable<PetResponseDto>> GetPetsAsync(int ownerId)
    {
        return await _db.Pets.Where(p => p.OwnerId == ownerId).Select(p => p.ToResponseDto()).ToListAsync();
    }

    public async Task<PagedResult<AppointmentResponseDto>> GetAppointmentsAsync(int ownerId, PaginationParams pagination)
    {
        var query = _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId)
            .OrderByDescending(a => a.AppointmentDate);
        return await query.Select(a => a.ToResponseDto()).ToPagedResultAsync(pagination);
    }
}
