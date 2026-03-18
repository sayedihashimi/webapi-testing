using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(VetClinicDbContext db)
    {
        if (await db.Owners.AnyAsync())
        {
            return;
        }

        // Owners
        var owners = new List<Owner>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701" },
            new() { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201" },
            new() { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301" },
            new() { Id = 4, FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0104", Address = "321 Elm Drive", City = "Denver", State = "CO", ZipCode = "80201" },
            new() { Id = 5, FirstName = "Lisa", LastName = "Patel", Email = "lisa.patel@email.com", Phone = "555-0105", Address = "654 Birch Lane", City = "Seattle", State = "WA", ZipCode = "98101" }
        };
        db.Owners.AddRange(owners);

        // Pets
        var pets = new List<Pet>
        {
            new() { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", OwnerId = 1 },
            new() { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.2m, Color = "Cream", MicrochipNumber = "MC-002-2019", OwnerId = 1 },
            new() { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", OwnerId = 2 },
            new() { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 5.8m, Color = "Tabby", OwnerId = 2 },
            new() { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2021, 9, 3), Weight = 12.3m, Color = "Tricolor", MicrochipNumber = "MC-005-2021", OwnerId = 3 },
            new() { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.09m, Color = "Yellow", OwnerId = 4 },
            new() { Id = 7, Name = "Coco", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 20), Weight = 1.8m, Color = "Brown", OwnerId = 5 },
            new() { Id = 8, Name = "Bella", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2020, 11, 5), Weight = 28.0m, Color = "Chocolate", MicrochipNumber = "MC-008-2020", OwnerId = 5 }
        };
        db.Pets.AddRange(pets);

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", HireDate = new DateOnly(2015, 6, 1) },
            new() { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", HireDate = new DateOnly(2018, 3, 15) },
            new() { Id = 3, FirstName = "Dr. Maria", LastName = "Santos", Email = "maria.santos@happypaws.com", Phone = "555-0203", Specialization = "Exotic Animals", LicenseNumber = "VET-2020-003", HireDate = new DateOnly(2020, 1, 10) }
        };
        db.Veterinarians.AddRange(vets);

        await db.SaveChangesAsync();

        // Appointments — mix of past/future, various statuses
        var now = DateTime.UtcNow;
        var appointments = new List<Appointment>
        {
            new() { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = now.AddDays(-30), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup" },
            new() { Id = 2, PetId = 2, VeterinarianId = 1, AppointmentDate = now.AddDays(-20), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Dental cleaning" },
            new() { Id = 3, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(-15), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on right front leg" },
            new() { Id = 4, PetId = 5, VeterinarianId = 1, AppointmentDate = now.AddDays(-10), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Vaccination booster" },
            new() { Id = 5, PetId = 4, VeterinarianId = 3, AppointmentDate = now.AddDays(-5), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Skin irritation", CancellationReason = "Owner rescheduled" },
            new() { Id = 6, PetId = 1, VeterinarianId = 1, AppointmentDate = now.AddDays(2).Date.AddHours(9), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up checkup" },
            new() { Id = 7, PetId = 6, VeterinarianId = 3, AppointmentDate = now.AddDays(3).Date.AddHours(10), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Wing feather trimming" },
            new() { Id = 8, PetId = 7, VeterinarianId = 3, AppointmentDate = now.AddDays(5).Date.AddHours(14), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Nail trimming" },
            new() { Id = 9, PetId = 8, VeterinarianId = 2, AppointmentDate = now.AddDays(7).Date.AddHours(11), DurationMinutes = 60, Status = AppointmentStatus.Scheduled, Reason = "Hip evaluation" },
            new() { Id = 10, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(-2), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Follow-up on leg" }
        };
        db.Appointments.AddRange(appointments);
        await db.SaveChangesAsync();

        // Medical Records for completed appointments
        var records = new List<MedicalRecord>
        {
            new() { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy, no concerns", Treatment = "Routine examination completed. All vitals normal.", Notes = "Weight stable. Coat in good condition.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(6)) },
            new() { Id = 2, AppointmentId = 2, PetId = 2, VeterinarianId = 1, Diagnosis = "Mild tartar buildup", Treatment = "Professional dental cleaning performed under light sedation.", Notes = "Recommend dental treats for maintenance." },
            new() { Id = 3, AppointmentId = 3, PetId = 3, VeterinarianId = 2, Diagnosis = "Minor sprain in right front leg", Treatment = "Anti-inflammatory medication prescribed. Rest recommended for 2 weeks.", FollowUpDate = DateOnly.FromDateTime(now.AddDays(14)) },
            new() { Id = 4, AppointmentId = 4, PetId = 5, VeterinarianId = 1, Diagnosis = "Healthy, vaccination administered", Treatment = "DHPP booster vaccination administered.", Notes = "No adverse reactions observed." }
        };
        db.MedicalRecords.AddRange(records);
        await db.SaveChangesAsync();

        // Prescriptions
        var prescriptions = new List<Prescription>
        {
            new() { Id = 1, MedicalRecordId = 3, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = DateOnly.FromDateTime(now.AddDays(-15)), Instructions = "Give with food. Monitor for GI upset." },
            new() { Id = 2, MedicalRecordId = 3, MedicationName = "Gabapentin", Dosage = "100mg once daily", DurationDays = 7, StartDate = DateOnly.FromDateTime(now.AddDays(-15)), Instructions = "For pain management. May cause drowsiness." },
            new() { Id = 3, MedicalRecordId = 2, MedicationName = "Amoxicillin", Dosage = "50mg twice daily", DurationDays = 10, StartDate = DateOnly.FromDateTime(now.AddDays(-20)), Instructions = "Complete full course even if symptoms improve." },
            new() { Id = 4, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 180, StartDate = DateOnly.FromDateTime(now.AddDays(-30)), Instructions = "Monthly heartworm prevention." },
            new() { Id = 5, MedicalRecordId = 4, MedicationName = "Frontline Plus", Dosage = "1 application monthly", DurationDays = 90, StartDate = DateOnly.FromDateTime(now.AddDays(-10)), Instructions = "Topical flea and tick prevention. Apply between shoulder blades." }
        };
        db.Prescriptions.AddRange(prescriptions);

        // Vaccinations
        var vaccinations = new List<Vaccination>
        {
            new() { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-6)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(6)), BatchNumber = "RAB-2024-A1", AdministeredByVetId = 1 },
            new() { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = DateOnly.FromDateTime(now.AddDays(-30)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(12)), BatchNumber = "DHPP-2024-B2", AdministeredByVetId = 1 },
            new() { Id = 3, PetId = 3, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-11)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(20)), BatchNumber = "RAB-2024-C3", AdministeredByVetId = 2, Notes = "Due for renewal soon" },
            new() { Id = 4, PetId = 5, VaccineName = "DHPP", DateAdministered = DateOnly.FromDateTime(now.AddDays(-10)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(12)), BatchNumber = "DHPP-2024-D4", AdministeredByVetId = 1 },
            new() { Id = 5, PetId = 2, VaccineName = "FVRCP", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-14)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(-30)), BatchNumber = "FVR-2023-E5", AdministeredByVetId = 1, Notes = "Expired — needs renewal" },
            new() { Id = 6, PetId = 8, VaccineName = "Bordetella", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-5)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(1)), BatchNumber = "BOR-2024-F6", AdministeredByVetId = 2 }
        };
        db.Vaccinations.AddRange(vaccinations);

        await db.SaveChangesAsync();
    }
}
