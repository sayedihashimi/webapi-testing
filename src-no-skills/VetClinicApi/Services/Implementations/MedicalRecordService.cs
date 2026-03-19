using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs.MedicalRecord;
using VetClinicApi.Models;
using VetClinicApi.Models.Enums;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Services.Implementations;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<MedicalRecordService> _logger;

    public MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MedicalRecordDto> GetByIdAsync(int id)
    {
        var record = await _db.MedicalRecords
            .Include(m => m.Pet).Include(m => m.Veterinarian).Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Medical record with ID {id} not found");

        return MapToDto(record);
    }

    public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto)
    {
        var appointment = await _db.Appointments
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId)
            ?? throw new KeyNotFoundException($"Appointment with ID {dto.AppointmentId} not found");

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
            throw new InvalidOperationException($"Medical records can only be created for InProgress or Completed appointments. Current status: {appointment.Status}");

        if (appointment.MedicalRecord != null)
            throw new InvalidOperationException($"Appointment {dto.AppointmentId} already has a medical record");

        var record = new MedicalRecord
        {
            AppointmentId = dto.AppointmentId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate
        };

        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();

        await _db.Entry(record).Reference(m => m.Pet).LoadAsync();
        await _db.Entry(record).Reference(m => m.Veterinarian).LoadAsync();
        _logger.LogInformation("Created medical record {RecordId} for appointment {AptId}", record.Id, dto.AppointmentId);
        return MapToDto(record);
    }

    public async Task<MedicalRecordDto> UpdateAsync(int id, UpdateMedicalRecordDto dto)
    {
        var record = await _db.MedicalRecords
            .Include(m => m.Pet).Include(m => m.Veterinarian).Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Medical record with ID {id} not found");

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated medical record {RecordId}", id);
        return MapToDto(record);
    }

    private static MedicalRecordDto MapToDto(MedicalRecord m) => new()
    {
        Id = m.Id, AppointmentId = m.AppointmentId, PetId = m.PetId,
        PetName = m.Pet.Name, VeterinarianId = m.VeterinarianId,
        VeterinarianName = m.Veterinarian.FirstName + " " + m.Veterinarian.LastName,
        Diagnosis = m.Diagnosis, Treatment = m.Treatment, Notes = m.Notes,
        FollowUpDate = m.FollowUpDate, CreatedAt = m.CreatedAt,
        Prescriptions = m.Prescriptions.Select(p => new PrescriptionSummaryDto
        {
            Id = p.Id, MedicationName = p.MedicationName, Dosage = p.Dosage, IsActive = p.IsActive
        }).ToList()
    };
}
