using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger) : IPrescriptionService
{
    public async Task<PrescriptionDto?> GetByIdAsync(int id)
    {
        var p = await db.Prescriptions.FindAsync(id);
        if (p is null) return null;

        return new PrescriptionDto(p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays, p.StartDate, p.EndDate, p.IsActive, p.Instructions, p.CreatedAt);
    }

    public async Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto)
    {
        if (!await db.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId))
            throw new InvalidOperationException("Medical record not found.");

        var prescription = new Prescription
        {
            MedicalRecordId = dto.MedicalRecordId,
            MedicationName = dto.MedicationName,
            Dosage = dto.Dosage,
            DurationDays = dto.DurationDays,
            StartDate = dto.StartDate,
            Instructions = dto.Instructions
        };

        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync();
        logger.LogInformation("Created prescription {PrescriptionId} for record {RecordId}", prescription.Id, dto.MedicalRecordId);

        return new PrescriptionDto(prescription.Id, prescription.MedicalRecordId, prescription.MedicationName, prescription.Dosage, prescription.DurationDays, prescription.StartDate, prescription.EndDate, prescription.IsActive, prescription.Instructions, prescription.CreatedAt);
    }
}
