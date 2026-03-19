using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs.Appointment;
using VetClinicApi.DTOs.Common;
using VetClinicApi.DTOs.Veterinarian;
using VetClinicApi.Models;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Services.Implementations;

public class VeterinarianService : IVeterinarianService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<VeterinarianService> _logger;

    public VeterinarianService(VetClinicDbContext db, ILogger<VeterinarianService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, PaginationParams pagination)
    {
        var query = _db.Veterinarians.AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(v => MapToDto(v))
            .ToListAsync();

        return new PagedResult<VeterinarianDto> { Items = items, TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize };
    }

    public async Task<VeterinarianDto> GetByIdAsync(int id)
    {
        var vet = await _db.Veterinarians.FindAsync(id)
            ?? throw new KeyNotFoundException($"Veterinarian with ID {id} not found");
        return MapToDto(vet);
    }

    public async Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto)
    {
        if (await _db.Veterinarians.AnyAsync(v => v.Email == dto.Email))
            throw new ArgumentException($"A veterinarian with email '{dto.Email}' already exists");

        if (await _db.Veterinarians.AnyAsync(v => v.LicenseNumber == dto.LicenseNumber))
            throw new ArgumentException($"A veterinarian with license number '{dto.LicenseNumber}' already exists");

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
        var vet = await _db.Veterinarians.FindAsync(id)
            ?? throw new KeyNotFoundException($"Veterinarian with ID {id} not found");

        if (await _db.Veterinarians.AnyAsync(v => v.Email == dto.Email && v.Id != id))
            throw new ArgumentException($"A veterinarian with email '{dto.Email}' already exists");

        if (await _db.Veterinarians.AnyAsync(v => v.LicenseNumber == dto.LicenseNumber && v.Id != id))
            throw new ArgumentException($"A veterinarian with license number '{dto.LicenseNumber}' already exists");

        vet.FirstName = dto.FirstName;
        vet.LastName = dto.LastName;
        vet.Email = dto.Email;
        vet.Phone = dto.Phone;
        vet.Specialization = dto.Specialization;
        vet.LicenseNumber = dto.LicenseNumber;
        vet.IsAvailable = dto.IsAvailable;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated veterinarian {VetId}", id);
        return MapToDto(vet);
    }

    public async Task<List<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date)
    {
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == vetId))
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found");

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        return await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId && a.AppointmentDate >= startOfDay && a.AppointmentDate <= endOfDay)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => MapAppointmentToDto(a))
            .ToListAsync();
    }

    public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int vetId, PaginationParams pagination)
    {
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == vetId))
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found");

        var query = _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => MapAppointmentToDto(a))
            .ToListAsync();

        return new PagedResult<AppointmentDto> { Items = items, TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize };
    }

    private static VeterinarianDto MapToDto(Veterinarian v) => new()
    {
        Id = v.Id, FirstName = v.FirstName, LastName = v.LastName,
        Email = v.Email, Phone = v.Phone, Specialization = v.Specialization,
        LicenseNumber = v.LicenseNumber, IsAvailable = v.IsAvailable, HireDate = v.HireDate
    };

    private static AppointmentDto MapAppointmentToDto(Appointment a) => new()
    {
        Id = a.Id, PetId = a.PetId, PetName = a.Pet.Name,
        VeterinarianId = a.VeterinarianId, VeterinarianName = a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
        AppointmentDate = a.AppointmentDate, DurationMinutes = a.DurationMinutes,
        Status = a.Status, Reason = a.Reason, Notes = a.Notes,
        CancellationReason = a.CancellationReason, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt
    };
}
