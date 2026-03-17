using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService
{
    // Valid status transitions
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> ValidTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
        [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.NoShow] = [],
    };

    public async Task<PagedResult<AppointmentDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, AppointmentStatus? status, int? vetId, int? petId, PaginationParams pagination)
    {
        var query = db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .AsQueryable();

        if (fromDate.HasValue) query = query.Where(a => a.AppointmentDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(a => a.AppointmentDate <= toDate.Value);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (vetId.HasValue) query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue) query = query.Where(a => a.PetId == petId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip(pagination.Skip).Take(pagination.PageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return new PagedResult<AppointmentDto>(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task<AppointmentDetailDto?> GetByIdAsync(int id)
    {
        var a = await db.Appointments
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Prescriptions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (a is null) return null;

        MedicalRecordDto? mrDto = null;
        if (a.MedicalRecord is { } mr)
        {
            mrDto = new MedicalRecordDto(mr.Id, mr.AppointmentId, mr.PetId, a.Pet.Name, mr.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                mr.Diagnosis, mr.Treatment, mr.Notes, mr.FollowUpDate, mr.CreatedAt);
        }

        return new AppointmentDetailDto(
            a.Id, a.PetId, a.Pet.Name, a.Pet.Species, a.Pet.OwnerId,
            a.Pet.Owner.FirstName + " " + a.Pet.Owner.LastName,
            a.VeterinarianId, a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
            a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes, a.CancellationReason,
            a.CreatedAt, a.UpdatedAt, mrDto);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
    {
        if (dto.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future.");

        if (!await db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new InvalidOperationException("Active pet not found.");

        if (!await db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new InvalidOperationException("Veterinarian not found.");

        await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, excludeAppointmentId: null);

        var appointment = new Appointment
        {
            PetId = dto.PetId,
            VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate,
            DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason,
            Notes = dto.Notes
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();

        // Reload with nav properties
        await db.Entry(appointment).Reference(a => a.Pet).LoadAsync();
        await db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync();

        logger.LogInformation("Created appointment {AppointmentId} for pet {PetId} with vet {VetId}", appointment.Id, dto.PetId, dto.VeterinarianId);
        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        var appointment = await db.Appointments.Include(a => a.Pet).Include(a => a.Veterinarian).FirstOrDefaultAsync(a => a.Id == id);
        if (appointment is null) return null;

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            throw new InvalidOperationException("Cannot update a completed, cancelled, or no-show appointment.");

        if (!await db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new InvalidOperationException("Active pet not found.");

        if (!await db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new InvalidOperationException("Veterinarian not found.");

        // Only check conflicts if date/time or vet changed
        if (appointment.AppointmentDate != dto.AppointmentDate || appointment.VeterinarianId != dto.VeterinarianId || appointment.DurationMinutes != dto.DurationMinutes)
            await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, excludeAppointmentId: id);

        appointment.PetId = dto.PetId;
        appointment.VeterinarianId = dto.VeterinarianId;
        appointment.AppointmentDate = dto.AppointmentDate;
        appointment.DurationMinutes = dto.DurationMinutes;
        appointment.Reason = dto.Reason;
        appointment.Notes = dto.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated appointment {AppointmentId}", id);
        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto)
    {
        var appointment = await db.Appointments.Include(a => a.Pet).Include(a => a.Veterinarian).FirstOrDefaultAsync(a => a.Id == id);
        if (appointment is null) return null;

        // Validate transition
        if (!ValidTransitions.TryGetValue(appointment.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new InvalidOperationException($"Cannot transition from {appointment.Status} to {dto.Status}.");

        // Cancellation rules
        if (dto.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
                throw new ArgumentException("CancellationReason is required when cancelling.");

            if (appointment.AppointmentDate < DateTime.UtcNow)
                throw new InvalidOperationException("Cannot cancel a past appointment.");

            appointment.CancellationReason = dto.CancellationReason;
        }

        appointment.Status = dto.Status;
        appointment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, dto.Status);
        return MapToDto(appointment);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetTodayAsync()
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        return await db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    private async Task CheckSchedulingConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId)
    {
        var newStart = appointmentDate;
        var newEnd = appointmentDate.AddMinutes(durationMinutes);

        var query = db.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        var hasConflict = await query.AnyAsync(a =>
            newStart < a.AppointmentDate.AddMinutes(a.DurationMinutes) && newEnd > a.AppointmentDate);

        if (hasConflict)
            throw new InvalidOperationException("Scheduling conflict: veterinarian already has an appointment during this time slot.");
    }

    private static AppointmentDto MapToDto(Appointment a) => new(
        a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
        a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
        a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes, a.CancellationReason,
        a.CreatedAt, a.UpdatedAt);
}
