using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class AppointmentService : IAppointmentService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> ValidTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
        [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.NoShow] = []
    };

    public async Task<PagedResult<AppointmentResponseDto>> GetAllAsync(DateTime? from, DateTime? to, string? status, int? vetId, int? petId, PaginationParams pagination)
    {
        var query = _db.Appointments.Include(a => a.Pet).Include(a => a.Veterinarian).AsQueryable();

        if (from.HasValue) query = query.Where(a => a.AppointmentDate >= from.Value);
        if (to.HasValue) query = query.Where(a => a.AppointmentDate <= to.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);
        if (vetId.HasValue) query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue) query = query.Where(a => a.PetId == petId.Value);

        query = query.OrderByDescending(a => a.AppointmentDate);
        return await query.Select(a => a.ToResponseDto()).ToPagedResultAsync(pagination);
    }

    public async Task<AppointmentDetailDto?> GetByIdAsync(int id)
    {
        var appt = await _db.Appointments
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Prescriptions)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Pet)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id);
        return appt?.ToDetailDto();
    }

    public async Task<(AppointmentResponseDto? Result, string? Error)> CreateAsync(AppointmentCreateDto dto)
    {
        if (dto.AppointmentDate <= DateTime.UtcNow)
            return (null, "Appointment date must be in the future.");

        if (dto.DurationMinutes < 15 || dto.DurationMinutes > 120)
            return (null, "Duration must be between 15 and 120 minutes.");

        var pet = await _db.Pets.FindAsync(dto.PetId);
        if (pet is null) return (null, "Pet not found.");

        var vet = await _db.Veterinarians.FindAsync(dto.VeterinarianId);
        if (vet is null) return (null, "Veterinarian not found.");

        var conflict = await HasSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, null);
        if (conflict) return (null, "Scheduling conflict: the veterinarian already has an appointment during this time slot.");

        var appt = new Appointment
        {
            PetId = dto.PetId,
            VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate,
            DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason,
            Notes = dto.Notes,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();

        // Reload with nav properties
        await _db.Entry(appt).Reference(a => a.Pet).LoadAsync();
        await _db.Entry(appt).Reference(a => a.Veterinarian).LoadAsync();

        _logger.LogInformation("Appointment created: {ApptId} for pet {PetId} with vet {VetId}", appt.Id, appt.PetId, appt.VeterinarianId);
        return (appt.ToResponseDto(), null);
    }

    public async Task<(AppointmentResponseDto? Result, string? Error)> UpdateAsync(int id, AppointmentUpdateDto dto)
    {
        var appt = await _db.Appointments.Include(a => a.Pet).Include(a => a.Veterinarian).FirstOrDefaultAsync(a => a.Id == id);
        if (appt is null) return (null, "Appointment not found.");

        if (appt.Status == AppointmentStatus.Completed || appt.Status == AppointmentStatus.Cancelled || appt.Status == AppointmentStatus.NoShow)
            return (null, $"Cannot update an appointment with status '{appt.Status}'.");

        if (dto.DurationMinutes < 15 || dto.DurationMinutes > 120)
            return (null, "Duration must be between 15 and 120 minutes.");

        var conflict = await HasSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id);
        if (conflict) return (null, "Scheduling conflict: the veterinarian already has an appointment during this time slot.");

        appt.AppointmentDate = dto.AppointmentDate;
        appt.DurationMinutes = dto.DurationMinutes;
        appt.VeterinarianId = dto.VeterinarianId;
        appt.Reason = dto.Reason;
        appt.Notes = dto.Notes;
        appt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _db.Entry(appt).Reference(a => a.Veterinarian).LoadAsync();
        _logger.LogInformation("Appointment updated: {ApptId}", id);
        return (appt.ToResponseDto(), null);
    }

    public async Task<(AppointmentResponseDto? Result, string? Error)> UpdateStatusAsync(int id, AppointmentStatusUpdateDto dto)
    {
        var appt = await _db.Appointments.Include(a => a.Pet).Include(a => a.Veterinarian).FirstOrDefaultAsync(a => a.Id == id);
        if (appt is null) return (null, "Appointment not found.");

        if (!Enum.TryParse<AppointmentStatus>(dto.Status, true, out var newStatus))
            return (null, $"Invalid status: '{dto.Status}'. Valid values: {string.Join(", ", Enum.GetNames<AppointmentStatus>())}");

        if (!ValidTransitions.TryGetValue(appt.Status, out var allowed) || !allowed.Contains(newStatus))
            return (null, $"Invalid status transition from '{appt.Status}' to '{newStatus}'.");

        if (newStatus == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
                return (null, "CancellationReason is required when cancelling an appointment.");
            if (appt.AppointmentDate < DateTime.UtcNow)
                return (null, "Cannot cancel a past appointment.");
            appt.CancellationReason = dto.CancellationReason;
        }

        appt.Status = newStatus;
        appt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Appointment {ApptId} status changed to {Status}", id, newStatus);
        return (appt.ToResponseDto(), null);
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetTodayAsync()
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);
        return await _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => a.ToResponseDto())
            .ToListAsync();
    }

    private async Task<bool> HasSchedulingConflict(int vetId, DateTime proposedStart, int durationMinutes, int? excludeAppointmentId)
    {
        var proposedEnd = proposedStart.AddMinutes(durationMinutes);

        var query = _db.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return await query.AnyAsync(a =>
            proposedStart < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
            proposedEnd > a.AppointmentDate);
    }
}
