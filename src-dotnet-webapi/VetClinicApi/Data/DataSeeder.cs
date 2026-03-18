using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public class DataSeeder(VetClinicDbContext db, ILogger<DataSeeder> logger)
{
    public async Task SeedAsync()
    {
        if (await db.Owners.AnyAsync())
        {
            logger.LogInformation("Database already contains data, skipping seed.");
            return;
        }

        logger.LogInformation("Seeding database with initial data...");

        // Owners
        var owners = new List<Owner>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0104", Address = "321 Elm Court", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 5, FirstName = "Amanda", LastName = "Taylor", Email = "amanda.taylor@email.com", Phone = "555-0105", Address = "654 Cedar Lane", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        db.Owners.AddRange(owners);
        await db.SaveChangesAsync();

        // Pets
        var pets = new List<Pet>
        {
            new() { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", IsActive = true, OwnerId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.2m, Color = "Cream and Brown", MicrochipNumber = "MC-002-2019", IsActive = true, OwnerId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", IsActive = true, OwnerId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 3), Weight = 6.8m, Color = "Tabby", IsActive = true, OwnerId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 5, Name = "Coco", Species = "Bird", Breed = "African Grey Parrot", DateOfBirth = new DateOnly(2018, 11, 8), Weight = 0.45m, Color = "Grey", IsActive = true, OwnerId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 6, Name = "Rocky", Species = "Dog", Breed = "Bulldog", DateOfBirth = new DateOnly(2020, 9, 25), Weight = 25.0m, Color = "White and Brown", MicrochipNumber = "MC-006-2020", IsActive = true, OwnerId = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 7, Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 1.8m, Color = "White", IsActive = true, OwnerId = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 8, Name = "Bella", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2019, 12, 1), Weight = 28.5m, Color = "Chocolate", MicrochipNumber = "MC-008-2019", IsActive = true, OwnerId = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        db.Pets.AddRange(pets);
        await db.SaveChangesAsync();

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { Id = 1, FirstName = "Dr. Robert", LastName = "Smith", Email = "dr.smith@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new() { Id = 2, FirstName = "Dr. Lisa", LastName = "Park", Email = "dr.park@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new() { Id = 3, FirstName = "Dr. David", LastName = "Martinez", Email = "dr.martinez@happypaws.com", Phone = "555-0203", Specialization = "Exotic Animals", LicenseNumber = "VET-2020-003", IsAvailable = true, HireDate = new DateOnly(2020, 1, 10) }
        };
        db.Veterinarians.AddRange(vets);
        await db.SaveChangesAsync();

        // Appointments - mix of past completed, cancelled, and future scheduled
        var now = DateTime.UtcNow;
        var appointments = new List<Appointment>
        {
            // Completed appointments (past)
            new() { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = now.AddDays(-30), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup", Notes = "All vitals normal", CreatedAt = now.AddDays(-35), UpdatedAt = now.AddDays(-30) },
            new() { Id = 2, PetId = 2, VeterinarianId = 1, AppointmentDate = now.AddDays(-25), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", Notes = "Minor tartar buildup removed", CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-25) },
            new() { Id = 3, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(-20), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on left front leg", Notes = "X-ray showed no fracture, mild sprain", CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-20) },
            new() { Id = 4, PetId = 5, VeterinarianId = 3, AppointmentDate = now.AddDays(-15), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Feather plucking behavior", Notes = "Dietary changes recommended", CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-15) },
            // Cancelled appointment
            new() { Id = 5, PetId = 4, VeterinarianId = 1, AppointmentDate = now.AddDays(-10), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Vaccination update", CancellationReason = "Owner travel conflict", CreatedAt = now.AddDays(-15), UpdatedAt = now.AddDays(-11) },
            // Future scheduled appointments
            new() { Id = 6, PetId = 1, VeterinarianId = 1, AppointmentDate = now.AddDays(3), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up checkup", CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5) },
            new() { Id = 7, PetId = 6, VeterinarianId = 2, AppointmentDate = now.AddDays(5), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Skin rash examination", CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3) },
            new() { Id = 8, PetId = 7, VeterinarianId = 3, AppointmentDate = now.AddDays(7), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Routine rabbit wellness check", CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2) },
            new() { Id = 9, PetId = 8, VeterinarianId = 1, AppointmentDate = now.AddDays(10), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Vaccination booster", CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1) },
            new() { Id = 10, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(14), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up for leg sprain", CreatedAt = now, UpdatedAt = now }
        };
        db.Appointments.AddRange(appointments);
        await db.SaveChangesAsync();

        // Medical Records (for completed appointments)
        var medicalRecords = new List<MedicalRecord>
        {
            new() { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy - annual exam", Treatment = "No treatment required. Updated vaccinations.", Notes = "Weight stable, coat healthy. Recommended continued current diet.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(6)), CreatedAt = now.AddDays(-30) },
            new() { Id = 2, AppointmentId = 2, PetId = 2, VeterinarianId = 1, Diagnosis = "Mild periodontal disease", Treatment = "Professional dental cleaning performed. Scaled and polished all teeth.", Notes = "Recommend dental treats and regular brushing.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(6)), CreatedAt = now.AddDays(-25) },
            new() { Id = 3, AppointmentId = 3, PetId = 3, VeterinarianId = 2, Diagnosis = "Left front leg sprain - Grade 1", Treatment = "Rest for 2 weeks. Anti-inflammatory medication prescribed. Cold compresses recommended.", Notes = "No fracture visible on X-ray. Should resolve with rest.", FollowUpDate = DateOnly.FromDateTime(now.AddDays(14)), CreatedAt = now.AddDays(-20) },
            new() { Id = 4, AppointmentId = 4, PetId = 5, VeterinarianId = 3, Diagnosis = "Behavioral feather plucking - stress related", Treatment = "Environmental enrichment plan. Dietary supplement added.", Notes = "Recommend more social interaction and foraging toys.", CreatedAt = now.AddDays(-15) }
        };
        db.MedicalRecords.AddRange(medicalRecords);
        await db.SaveChangesAsync();

        // Prescriptions
        var today = DateOnly.FromDateTime(now);
        var prescriptions = new List<Prescription>
        {
            new() { Id = 1, MedicalRecordId = 3, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = today.AddDays(-20), EndDate = today.AddDays(-6), Instructions = "Give with food. Monitor for GI upset.", CreatedAt = now.AddDays(-20) },
            new() { Id = 2, MedicalRecordId = 3, MedicationName = "Glucosamine Supplement", Dosage = "500mg once daily", DurationDays = 60, StartDate = today.AddDays(-20), EndDate = today.AddDays(40), Instructions = "Mix with food. Long-term joint support.", CreatedAt = now.AddDays(-20) },
            new() { Id = 3, MedicalRecordId = 4, MedicationName = "Avian Calming Supplement", Dosage = "2ml once daily", DurationDays = 30, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Instructions = "Add to water dish. Replace daily.", CreatedAt = now.AddDays(-15) },
            new() { Id = 4, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 180, StartDate = today.AddDays(-30), EndDate = today.AddDays(150), Instructions = "Give on the 1st of each month.", CreatedAt = now.AddDays(-30) },
            new() { Id = 5, MedicalRecordId = 2, MedicationName = "Dental Enzyme Paste", Dosage = "Apply to gums once daily", DurationDays = 7, StartDate = today.AddDays(-25), EndDate = today.AddDays(-18), Instructions = "Apply small amount to gums after meals.", CreatedAt = now.AddDays(-25) }
        };
        db.Prescriptions.AddRange(prescriptions);
        await db.SaveChangesAsync();

        // Vaccinations
        var vaccinations = new List<Vaccination>
        {
            new() { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = today.AddDays(-30), ExpirationDate = today.AddYears(1).AddDays(-30), BatchNumber = "RAB-2024-A1", AdministeredByVetId = 1, Notes = "3-year rabies vaccine", CreatedAt = now.AddDays(-30) },
            new() { Id = 2, PetId = 1, VaccineName = "DHPP (Distemper combo)", DateAdministered = today.AddDays(-30), ExpirationDate = today.AddYears(1).AddDays(-30), BatchNumber = "DHPP-2024-B2", AdministeredByVetId = 1, CreatedAt = now.AddDays(-30) },
            new() { Id = 3, PetId = 2, VaccineName = "FVRCP (Feline Distemper)", DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(20), BatchNumber = "FVR-2024-C1", AdministeredByVetId = 1, Notes = "Due for renewal soon", CreatedAt = now.AddMonths(-11) },
            new() { Id = 4, PetId = 3, VaccineName = "Rabies", DateAdministered = today.AddMonths(-13), ExpirationDate = today.AddDays(-30), BatchNumber = "RAB-2023-D1", AdministeredByVetId = 2, Notes = "EXPIRED - needs renewal", CreatedAt = now.AddMonths(-13) },
            new() { Id = 5, PetId = 6, VaccineName = "Bordetella", DateAdministered = today.AddMonths(-5), ExpirationDate = today.AddMonths(7), BatchNumber = "BOR-2024-E1", AdministeredByVetId = 1, CreatedAt = now.AddMonths(-5) },
            new() { Id = 6, PetId = 8, VaccineName = "Leptospirosis", DateAdministered = today.AddMonths(-10), ExpirationDate = today.AddMonths(2), BatchNumber = "LEP-2024-F1", AdministeredByVetId = 1, Notes = "Annual vaccine", CreatedAt = now.AddMonths(-10) }
        };
        db.Vaccinations.AddRange(vaccinations);
        await db.SaveChangesAsync();

        logger.LogInformation("Database seeded successfully with {Owners} owners, {Pets} pets, {Vets} veterinarians, {Appointments} appointments, {Records} medical records, {Prescriptions} prescriptions, {Vaccinations} vaccinations",
            owners.Count, pets.Count, vets.Count, appointments.Count, medicalRecords.Count, prescriptions.Count, vaccinations.Count);
    }
}
