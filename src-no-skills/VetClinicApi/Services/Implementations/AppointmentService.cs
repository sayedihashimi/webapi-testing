using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs.Appointment;
using VetClinicApi.DTOs.Common;
using VetClinicApi.Models;
using VetClinicApi.Models.Enums;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Valid status transitions
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> ValidTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
        [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.NoShow] = []
    };

    public async Task<PagedResult<AppointmentDto>> GetAllAsync(DateOnly? date, AppointmentStatus? status, int? vetId, int? petId, PaginationParams pagination)
    {
        var query = _db.Appointments.Include(a => a.Pet).Include(a => a.Veterinarian).AsQueryable();

        if (date.HasValue)
        {
            var start = date.Value.ToDateTime(TimeOnly.MinValue);
            var end = date.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end);
        }

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (vetId.HasValue)
            query = query.Where(a => a.VeterinarianId == vetId.Value);

        if (petId.HasValue)
            query = query.Where(a => a.PetId == petId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return new PagedResult<AppointmentDto> { Items = items, TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize };
    }

    public async Task<AppointmentDetailDto> GetByIdAsync(int id)
    {
        var apt = await _db.Appointments
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found");

        return new AppointmentDetailDto
        {
            Id = apt.Id, PetId = apt.PetId, PetName = apt.Pet.Name,
            VeterinarianId = apt.VeterinarianId,
            VeterinarianName = apt.Veterinarian.FirstName + " " + apt.Veterinarian.LastName,
            AppointmentDate = apt.AppointmentDate, DurationMinutes = apt.DurationMinutes,
            Status = apt.Status, Reason = apt.Reason, Notes = apt.Notes,
            CancellationReason = apt.CancellationReason,
            CreatedAt = apt.CreatedAt, UpdatedAt = apt.UpdatedAt,
            Pet = new AppointmentPetDto
            {
                Id = apt.Pet.Id, Name = apt.Pet.Name, Species = apt.Pet.Species,
                Breed = apt.Pet.Breed, OwnerName = apt.Pet.Owner.FirstName + " " + apt.Pet.Owner.LastName
            },
            Veterinarian = new AppointmentVetDto
            {
                Id = apt.Veterinarian.Id, FirstName = apt.Veterinarian.FirstName,
                LastName = apt.Veterinarian.LastName, Specialization = apt.Veterinarian.Specialization
            },
            MedicalRecord = apt.MedicalRecord == null ? null : new MedicalRecordSummaryDto
            {
                Id = apt.MedicalRecord.Id, Diagnosis = apt.MedicalRecord.Diagnosis, Treatment = apt.MedicalRecord.Treatment
            }
        };
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new KeyNotFoundException($"Active pet with ID {dto.PetId} not found");

        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new KeyNotFoundException($"Veterinarian with ID {dto.VeterinarianId} not found");

        if (dto.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future");

        await CheckScheduleConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, null);

        var apt = new Appointment
        {
            PetId = dto.PetId, VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate, DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason, Notes = dto.Notes
        };

        _db.Appointments.Add(apt);
        await _db.SaveChangesAsync();

        await _db.Entry(apt).Reference(a => a.Pet).LoadAsync();
        await _db.Entry(apt).Reference(a => a.Veterinarian).LoadAsync();
        _logger.LogInformation("Created appointment {AptId} for pet {PetId} with vet {VetId}", apt.Id, apt.PetId, apt.VeterinarianId);
        return MapToDto(apt);
    }

    public async Task<AppointmentDto> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        var apt = await _db.Appointments.Include(a => a.Pet).Include(a => a.Veterinarian).FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found");

        if (apt.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            throw new InvalidOperationException($"Cannot update appointment in {apt.Status} status");

        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new KeyNotFoundException($"Active pet with ID {dto.PetId} not found");

        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new KeyNotFoundException($"Veterinarian with ID {dto.VeterinarianId} not found");

        await CheckScheduleConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id);

        apt.PetId = dto.PetId;
        apt.VeterinarianId = dto.VeterinarianId;
        apt.AppointmentDate = dto.AppointmentDate;
        apt.DurationMinutes = dto.DurationMinutes;
        apt.Reason = dto.Reason;
        apt.Notes = dto.Notes;
        apt.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(apt).Reference(a => a.Pet).LoadAsync();
        await _db.Entry(apt).Reference(a => a.Veterinarian).LoadAsync();
        _logger.LogInformation("Updated appointment {AptId}", id);
        return MapToDto(apt);
    }

    public async Task<AppointmentDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto)
    {
        var apt = await _db.Appointments.Include(a => a.Pet).Include(a => a.Veterinarian).FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found");

        if (!ValidTransitions.TryGetValue(apt.Status, out var allowed) || !allowed.Contains(dto.NewStatus))
            throw new InvalidOperationException($"Cannot transition from {apt.Status} to {dto.NewStatus}");

        if (dto.NewStatus == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
                throw new ArgumentException("Cancellation reason is required when cancelling an appointment");

            if (apt.AppointmentDate < DateTime.UtcNow)
                throw new InvalidOperationException("Cannot cancel a past appointment");

            apt.CancellationReason = dto.CancellationReason;
        }

        apt.Status = dto.NewStatus;
        apt.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated appointment {AptId} status to {Status}", id, dto.NewStatus);
        return MapToDto(apt);
    }

    public async Task<List<AppointmentDto>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    private async Task CheckScheduleConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId)
    {
        var endTime = appointmentDate.AddMinutes(durationMinutes);

        var conflicting = await _db.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value)
                && a.AppointmentDate < endTime
                && appointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes))
            .AnyAsync();

        if (conflicting)
            throw new InvalidOperationException("The veterinarian has a scheduling conflict at the requested time");
    }

    private static AppointmentDto MapToDto(Appointment a) => new()
    {
        Id = a.Id, PetId = a.PetId, PetName = a.Pet.Name,
        VeterinarianId = a.VeterinarianId, VeterinarianName = a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
        AppointmentDate = a.AppointmentDate, DurationMinutes = a.DurationMinutes,
        Status = a.Status, Reason = a.Reason, Notes = a.Notes,
        CancellationReason = a.CancellationReason, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt
    };
}
