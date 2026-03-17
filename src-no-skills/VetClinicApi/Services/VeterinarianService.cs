using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PaginatedResponse<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize);
    Task<VeterinarianDto> GetByIdAsync(int id);
    Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto);
    Task<VeterinarianDto> UpdateAsync(int id, UpdateVeterinarianDto dto);
    Task<List<AppointmentSummaryDto>> GetScheduleAsync(int vetId, DateOnly date);
    Task<PaginatedResponse<AppointmentSummaryDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize);
}

public class VeterinarianService : IVeterinarianService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<VeterinarianService> _logger;

    public VeterinarianService(VetClinicDbContext db, ILogger<VeterinarianService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResponse<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize)
    {
        var query = _db.Veterinarians.AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower() == specialization.ToLower());

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(v => MapToDto(v))
            .ToListAsync();

        return new PaginatedResponse<VeterinarianDto>
        {
            Data = items, Page = page, PageSize = pageSize, TotalCount = totalCount
        };
    }

    public async Task<VeterinarianDto> GetByIdAsync(int id)
    {
        var vet = await _db.Veterinarians.FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new NotFoundException("Veterinarian", id);
        return MapToDto(vet);
    }

    public async Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto)
    {
        if (await _db.Veterinarians.AnyAsync(v => v.Email == dto.Email))
            throw new BusinessRuleException("A veterinarian with this email already exists.", StatusCodes.Status409Conflict);

        if (await _db.Veterinarians.AnyAsync(v => v.LicenseNumber == dto.LicenseNumber))
            throw new BusinessRuleException("A veterinarian with this license number already exists.", StatusCodes.Status409Conflict);

        var vet = new Veterinarian
        {
            FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email,
            Phone = dto.Phone, Specialization = dto.Specialization,
            LicenseNumber = dto.LicenseNumber, HireDate = dto.HireDate
        };

        _db.Veterinarians.Add(vet);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created veterinarian {VetId}: {FirstName} {LastName}", vet.Id, vet.FirstName, vet.LastName);
        return MapToDto(vet);
    }

    public async Task<VeterinarianDto> UpdateAsync(int id, UpdateVeterinarianDto dto)
    {
        var vet = await _db.Veterinarians.FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new NotFoundException("Veterinarian", id);

        if (await _db.Veterinarians.AnyAsync(v => v.Email == dto.Email && v.Id != id))
            throw new BusinessRuleException("A veterinarian with this email already exists.", StatusCodes.Status409Conflict);

        if (await _db.Veterinarians.AnyAsync(v => v.LicenseNumber == dto.LicenseNumber && v.Id != id))
            throw new BusinessRuleException("A veterinarian with this license number already exists.", StatusCodes.Status409Conflict);

        vet.FirstName = dto.FirstName;
        vet.LastName = dto.LastName;
        vet.Email = dto.Email;
        vet.Phone = dto.Phone;
        vet.Specialization = dto.Specialization;
        vet.LicenseNumber = dto.LicenseNumber;
        vet.IsAvailable = dto.IsAvailable;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated veterinarian {VetId}", vet.Id);
        return MapToDto(vet);
    }

    public async Task<List<AppointmentSummaryDto>> GetScheduleAsync(int vetId, DateOnly date)
    {
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == vetId))
            throw new NotFoundException("Veterinarian", vetId);

        var dateStart = date.ToDateTime(TimeOnly.MinValue);
        var dateEnd = date.ToDateTime(TimeOnly.MaxValue);

        return await _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId &&
                        a.AppointmentDate >= dateStart && a.AppointmentDate <= dateEnd &&
                        a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.NoShow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentSummaryDto
            {
                Id = a.Id, AppointmentDate = a.AppointmentDate,
                DurationMinutes = a.DurationMinutes, Status = a.Status,
                Reason = a.Reason,
                Pet = new PetSummaryDto { Id = a.Pet.Id, Name = a.Pet.Name, Species = a.Pet.Species, Breed = a.Pet.Breed, IsActive = a.Pet.IsActive },
                Veterinarian = new VeterinarianSummaryDto { Id = a.Veterinarian.Id, FirstName = a.Veterinarian.FirstName, LastName = a.Veterinarian.LastName, Specialization = a.Veterinarian.Specialization }
            }).ToListAsync();
    }

    public async Task<PaginatedResponse<AppointmentSummaryDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize)
    {
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == vetId))
            throw new NotFoundException("Veterinarian", vetId);

        var query = _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AppointmentSummaryDto
            {
                Id = a.Id, AppointmentDate = a.AppointmentDate,
                DurationMinutes = a.DurationMinutes, Status = a.Status,
                Reason = a.Reason,
                Pet = new PetSummaryDto { Id = a.Pet.Id, Name = a.Pet.Name, Species = a.Pet.Species, Breed = a.Pet.Breed, IsActive = a.Pet.IsActive },
                Veterinarian = new VeterinarianSummaryDto { Id = a.Veterinarian.Id, FirstName = a.Veterinarian.FirstName, LastName = a.Veterinarian.LastName, Specialization = a.Veterinarian.Specialization }
            }).ToListAsync();

        return new PaginatedResponse<AppointmentSummaryDto>
        {
            Data = items, Page = page, PageSize = pageSize, TotalCount = totalCount
        };
    }

    private static VeterinarianDto MapToDto(Veterinarian v) => new()
    {
        Id = v.Id, FirstName = v.FirstName, LastName = v.LastName,
        Email = v.Email, Phone = v.Phone, Specialization = v.Specialization,
        LicenseNumber = v.LicenseNumber, IsAvailable = v.IsAvailable, HireDate = v.HireDate
    };
}
