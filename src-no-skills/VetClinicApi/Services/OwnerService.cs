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

    public async Task<PagedResponse<OwnerResponseDto>> GetAllAsync(string? search, PaginationParams pagination)
    {
        var query = _db.Owners.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o => o.FirstName.ToLower().Contains(s) || o.LastName.ToLower().Contains(s) || o.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(o => MapToResponse(o, false))
            .ToListAsync();

        return new PagedResponse<OwnerResponseDto> { Items = items, TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize };
    }

    public async Task<OwnerResponseDto?> GetByIdAsync(int id)
    {
        var owner = await _db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id);
        return owner == null ? null : MapToResponse(owner, true);
    }

    public async Task<OwnerResponseDto> CreateAsync(CreateOwnerDto dto)
    {
        if (await _db.Owners.AnyAsync(o => o.Email == dto.Email))
            throw new ArgumentException($"An owner with email '{dto.Email}' already exists.");

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
        return MapToResponse(owner, false);
    }

    public async Task<OwnerResponseDto> UpdateAsync(int id, UpdateOwnerDto dto)
    {
        var owner = await _db.Owners.FindAsync(id) ?? throw new KeyNotFoundException($"Owner with ID {id} not found.");

        if (await _db.Owners.AnyAsync(o => o.Email == dto.Email && o.Id != id))
            throw new ArgumentException($"An owner with email '{dto.Email}' already exists.");

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
        return MapToResponse(owner, false);
    }

    public async Task DeleteAsync(int id)
    {
        var owner = await _db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Owner with ID {id} not found.");

        if (owner.Pets.Any(p => p.IsActive))
            throw new InvalidOperationException("Cannot delete owner with active pets. Deactivate or transfer pets first.");

        _db.Owners.Remove(owner);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Owner deleted: {OwnerId}", id);
    }

    public async Task<List<PetResponseDto>> GetPetsAsync(int ownerId)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == ownerId))
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        return await _db.Pets
            .Where(p => p.OwnerId == ownerId)
            .Include(p => p.Owner)
            .Select(p => new PetResponseDto
            {
                Id = p.Id, Name = p.Name, Species = p.Species, Breed = p.Breed,
                DateOfBirth = p.DateOfBirth, Weight = p.Weight, Color = p.Color,
                MicrochipNumber = p.MicrochipNumber, IsActive = p.IsActive, OwnerId = p.OwnerId,
                CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
            }).ToListAsync();
    }

    public async Task<PagedResponse<AppointmentResponseDto>> GetAppointmentsAsync(int ownerId, PaginationParams pagination)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == ownerId))
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        var petIds = await _db.Pets.Where(p => p.OwnerId == ownerId).Select(p => p.Id).ToListAsync();

        var query = _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .Where(a => petIds.Contains(a.PetId))
            .OrderByDescending(a => a.AppointmentDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResponse<AppointmentResponseDto>
        {
            Items = items.Select(AppointmentService.MapToResponse).ToList(),
            TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    private static OwnerResponseDto MapToResponse(Owner o, bool includePets) => new()
    {
        Id = o.Id, FirstName = o.FirstName, LastName = o.LastName, Email = o.Email,
        Phone = o.Phone, Address = o.Address, City = o.City, State = o.State, ZipCode = o.ZipCode,
        CreatedAt = o.CreatedAt, UpdatedAt = o.UpdatedAt,
        Pets = includePets ? o.Pets.Select(p => new PetSummaryDto { Id = p.Id, Name = p.Name, Species = p.Species, Breed = p.Breed, IsActive = p.IsActive }).ToList() : new()
    };
}
