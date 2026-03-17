using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VeterinarianService(VetClinicDbContext db, ILogger<VeterinarianService> logger) : IVeterinarianService
{
    public async Task<PagedResult<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, PaginationParams pagination)
    {
        var query = db.Veterinarians.AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Skip(pagination.Skip).Take(pagination.PageSize)
            .Select(v => MapToDto(v))
            .ToListAsync();

        return new PagedResult<VeterinarianDto>(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task<VeterinarianDto?> GetByIdAsync(int id)
    {
        var vet = await db.Veterinarians.FindAsync(id);
        return vet is null ? null : MapToDto(vet);
    }

    public async Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto)
    {
        var vet = new Veterinarian
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Specialization = dto.Specialization,
            LicenseNumber = dto.LicenseNumber,
            IsAvailable = dto.IsAvailable,
            HireDate = dto.HireDate
        };

        db.Veterinarians.Add(vet);
        await db.SaveChangesAsync();
        logger.LogInformation("Created veterinarian {VetId} ({Name})", vet.Id, $"{vet.FirstName} {vet.LastName}");
        return MapToDto(vet);
    }

    public async Task<VeterinarianDto?> UpdateAsync(int id, UpdateVeterinarianDto dto)
    {
        var vet = await db.Veterinarians.FindAsync(id);
        if (vet is null) return null;

        vet.FirstName = dto.FirstName;
        vet.LastName = dto.LastName;
        vet.Email = dto.Email;
        vet.Phone = dto.Phone;
        vet.Specialization = dto.Specialization;
        vet.LicenseNumber = dto.LicenseNumber;
        vet.IsAvailable = dto.IsAvailable;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated veterinarian {VetId}", vet.Id);
        return MapToDto(vet);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = date.ToDateTime(TimeOnly.MaxValue);

        return await db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId && a.AppointmentDate >= start && a.AppointmentDate <= end
                && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.NoShow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync();
    }

    public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int vetId, string? status, PaginationParams pagination)
    {
        var query = db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var s))
            query = query.Where(a => a.Status == s);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip(pagination.Skip).Take(pagination.PageSize)
            .Select(a => new AppointmentDto(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync();

        return new PagedResult<AppointmentDto>(items, total, pagination.Page, pagination.PageSize);
    }

    private static VeterinarianDto MapToDto(Veterinarian v) => new(
        v.Id, v.FirstName, v.LastName, v.Email, v.Phone,
        v.Specialization, v.LicenseNumber, v.IsAvailable, v.HireDate);
}
