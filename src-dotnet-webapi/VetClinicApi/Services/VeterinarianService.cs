using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VeterinarianService(VetClinicDbContext db, ILogger<VeterinarianService> logger) : IVeterinarianService
{
    public async Task<PaginatedResponse<VeterinarianResponse>> GetAllAsync(
        string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Veterinarians.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => MapToResponse(v))
            .ToListAsync(ct);

        return PaginatedResponse<VeterinarianResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<VeterinarianResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var vet = await db.Veterinarians.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
        return vet is null ? null : MapToResponse(vet);
    }

    public async Task<VeterinarianResponse> CreateAsync(CreateVeterinarianRequest request, CancellationToken ct)
    {
        if (await db.Veterinarians.AnyAsync(v => v.Email == request.Email, ct))
            throw new InvalidOperationException($"A veterinarian with email '{request.Email}' already exists.");

        if (await db.Veterinarians.AnyAsync(v => v.LicenseNumber == request.LicenseNumber, ct))
            throw new InvalidOperationException($"A veterinarian with license number '{request.LicenseNumber}' already exists.");

        var vet = new Veterinarian
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            HireDate = request.HireDate
        };

        db.Veterinarians.Add(vet);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created veterinarian {VetId}: {Name}", vet.Id, $"{vet.FirstName} {vet.LastName}");
        return MapToResponse(vet);
    }

    public async Task<VeterinarianResponse?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken ct)
    {
        var vet = await db.Veterinarians.FindAsync([id], ct);
        if (vet is null) return null;

        if (await db.Veterinarians.AnyAsync(v => v.Email == request.Email && v.Id != id, ct))
            throw new InvalidOperationException($"A veterinarian with email '{request.Email}' already exists.");

        if (await db.Veterinarians.AnyAsync(v => v.LicenseNumber == request.LicenseNumber && v.Id != id, ct))
            throw new InvalidOperationException($"A veterinarian with license number '{request.LicenseNumber}' already exists.");

        vet.FirstName = request.FirstName;
        vet.LastName = request.LastName;
        vet.Email = request.Email;
        vet.Phone = request.Phone;
        vet.Specialization = request.Specialization;
        vet.LicenseNumber = request.LicenseNumber;
        vet.IsAvailable = request.IsAvailable;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated veterinarian {VetId}", id);
        return MapToResponse(vet);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct)
    {
        if (!await db.Veterinarians.AnyAsync(v => v.Id == vetId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found.");

        var startOfDay = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId && a.AppointmentDate >= startOfDay && a.AppointmentDate <= endOfDay)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<PaginatedResponse<AppointmentResponse>> GetAppointmentsAsync(
        int vetId, string? status, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Veterinarians.AnyAsync(v => v.Id == vetId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found.");

        var query = db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<AppointmentResponse>.Create(items, page, pageSize, totalCount);
    }

    private static VeterinarianResponse MapToResponse(Veterinarian v) =>
        new(v.Id, v.FirstName, v.LastName, v.Email, v.Phone, v.Specialization, v.LicenseNumber, v.IsAvailable, v.HireDate);
}
