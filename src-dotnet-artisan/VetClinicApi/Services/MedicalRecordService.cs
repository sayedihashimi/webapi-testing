using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordDetailDto?> GetByIdAsync(int id)
    {
        var mr = await db.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mr is null) return null;

        var prescriptions = mr.Prescriptions.Select(p =>
            new PrescriptionDto(p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays, p.StartDate, p.EndDate, p.IsActive, p.Instructions, p.CreatedAt)).ToList();

        return new MedicalRecordDetailDto(
            mr.Id, mr.AppointmentId, mr.PetId, mr.Pet.Name, mr.VeterinarianId,
            mr.Veterinarian.FirstName + " " + mr.Veterinarian.LastName,
            mr.Diagnosis, mr.Treatment, mr.Notes, mr.FollowUpDate, mr.CreatedAt, prescriptions);
    }

    public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto)
    {
        var appointment = await db.Appointments.FindAsync(dto.AppointmentId)
            ?? throw new InvalidOperationException("Appointment not found.");

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
            throw new InvalidOperationException("Medical records can only be created for completed or in-progress appointments.");

        if (await db.MedicalRecords.AnyAsync(m => m.AppointmentId == dto.AppointmentId))
            throw new InvalidOperationException("A medical record already exists for this appointment.");

        var record = new MedicalRecord
        {
            AppointmentId = dto.AppointmentId,
            PetId = dto.PetId,
            VeterinarianId = dto.VeterinarianId,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate
        };

        db.MedicalRecords.Add(record);
        await db.SaveChangesAsync();

        await db.Entry(record).Reference(m => m.Pet).LoadAsync();
        await db.Entry(record).Reference(m => m.Veterinarian).LoadAsync();

        logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}", record.Id, dto.AppointmentId);

        return new MedicalRecordDto(
            record.Id, record.AppointmentId, record.PetId, record.Pet.Name, record.VeterinarianId,
            record.Veterinarian.FirstName + " " + record.Veterinarian.LastName,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt);
    }

    public async Task<MedicalRecordDto?> UpdateAsync(int id, UpdateMedicalRecordDto dto)
    {
        var record = await db.MedicalRecords
            .Include(m => m.Pet).Include(m => m.Veterinarian)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (record is null) return null;

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated medical record {RecordId}", id);

        return new MedicalRecordDto(
            record.Id, record.AppointmentId, record.PetId, record.Pet.Name, record.VeterinarianId,
            record.Veterinarian.FirstName + " " + record.Veterinarian.LastName,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt);
    }
}
