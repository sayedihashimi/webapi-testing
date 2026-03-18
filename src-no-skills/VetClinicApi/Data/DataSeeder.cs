using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class DataSeeder
{
    public static void Seed(VetClinicDbContext db)
    {
        if (db.Owners.Any()) return; // already seeded

        // --- Owners ---
        var owners = new List<Owner>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62704", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0104", Address = "321 Elm Boulevard", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 5, FirstName = "Lisa", LastName = "Thompson", Email = "lisa.thompson@email.com", Phone = "555-0105", Address = "654 Birch Lane", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };
        db.Owners.AddRange(owners);
        db.SaveChanges();

        // --- Pets ---
        var pets = new List<Pet>
        {
            new() { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", IsActive = true, OwnerId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m, Color = "Cream with brown points", MicrochipNumber = "MC-002-2019", IsActive = true, OwnerId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", IsActive = true, OwnerId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 3), Weight = 6.2m, Color = "Brown Tabby", IsActive = true, OwnerId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2020, 11, 28), Weight = 12.3m, Color = "Tricolor", MicrochipNumber = "MC-005-2020", IsActive = true, OwnerId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.09m, Color = "Yellow and Grey", IsActive = true, OwnerId = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 7, Name = "Daisy", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2019, 9, 5), Weight = 28.7m, Color = "Chocolate", MicrochipNumber = "MC-007-2019", IsActive = true, OwnerId = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 8, Name = "Oliver", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 20), Weight = 1.8m, Color = "White and Brown", IsActive = true, OwnerId = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };
        db.Pets.AddRange(pets);
        db.SaveChanges();

        // --- Veterinarians ---
        var vets = new List<Veterinarian>
        {
            new() { Id = 1, FirstName = "Amanda", LastName = "Foster", Email = "dr.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new() { Id = 2, FirstName = "Robert", LastName = "Kim", Email = "dr.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new() { Id = 3, FirstName = "Patricia", LastName = "Nguyen", Email = "dr.nguyen@happypaws.com", Phone = "555-0203", Specialization = "Dentistry", LicenseNumber = "VET-2020-003", IsAvailable = true, HireDate = new DateOnly(2020, 1, 10) },
        };
        db.Veterinarians.AddRange(vets);
        db.SaveChanges();

        // --- Appointments ---
        var now = DateTime.UtcNow;
        var appointments = new List<Appointment>
        {
            // Completed appointments (past)
            new() { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = now.AddDays(-30), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual wellness exam", CreatedAt = now.AddDays(-35), UpdatedAt = now.AddDays(-30) },
            new() { Id = 2, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(-25), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on front left leg", CreatedAt = now.AddDays(-28), UpdatedAt = now.AddDays(-25) },
            new() { Id = 3, PetId = 2, VeterinarianId = 1, AppointmentDate = now.AddDays(-20), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-20) },
            new() { Id = 4, PetId = 7, VeterinarianId = 3, AppointmentDate = now.AddDays(-15), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Teeth cleaning and examination", CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-15) },
            // Cancelled
            new() { Id = 5, PetId = 5, VeterinarianId = 1, AppointmentDate = now.AddDays(2), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Vaccination booster", CancellationReason = "Owner rescheduling due to travel", CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-5) },
            // Scheduled (future)
            new() { Id = 6, PetId = 4, VeterinarianId = 1, AppointmentDate = now.AddDays(3).Date.AddHours(9), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Routine checkup", CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2) },
            new() { Id = 7, PetId = 1, VeterinarianId = 2, AppointmentDate = now.AddDays(5).Date.AddHours(10), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Follow-up for leg injury", CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1) },
            new() { Id = 8, PetId = 6, VeterinarianId = 1, AppointmentDate = now.AddDays(7).Date.AddHours(14), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Wing feather trimming", CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, PetId = 8, VeterinarianId = 3, AppointmentDate = now.AddDays(10).Date.AddHours(11), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "General health check", CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, PetId = 5, VeterinarianId = 1, AppointmentDate = now.AddDays(14).Date.AddHours(9).AddMinutes(30), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Vaccination booster (rescheduled)", CreatedAt = now, UpdatedAt = now },
        };
        db.Appointments.AddRange(appointments);
        db.SaveChanges();

        // --- Medical Records (for completed appointments) ---
        var records = new List<MedicalRecord>
        {
            new() { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy — no issues found", Treatment = "No treatment needed. Recommended continued heartworm prevention.", Notes = "Weight is ideal. Coat in excellent condition.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(12)), CreatedAt = now.AddDays(-30) },
            new() { Id = 2, AppointmentId = 2, PetId = 3, VeterinarianId = 2, Diagnosis = "Mild sprain of the left forelimb", Treatment = "Prescribed anti-inflammatory medication and rest for 2 weeks. Apply cold compress twice daily.", Notes = "X-ray showed no fracture. Likely caused by overexertion during play.", FollowUpDate = DateOnly.FromDateTime(now.AddDays(-11)), CreatedAt = now.AddDays(-25) },
            new() { Id = 3, AppointmentId = 3, PetId = 2, VeterinarianId = 1, Diagnosis = "Mild gingivitis", Treatment = "Professional dental cleaning performed. Prescribed dental treats.", Notes = "Recommend switching to dental-care specific food.", CreatedAt = now.AddDays(-20) },
            new() { Id = 4, AppointmentId = 4, PetId = 7, VeterinarianId = 3, Diagnosis = "Moderate tartar buildup with early periodontal disease", Treatment = "Full dental cleaning under sedation. Extracted one loose premolar.", Notes = "Recovery expected within 3 days. Soft food only for one week.", FollowUpDate = DateOnly.FromDateTime(now.AddDays(-8)), CreatedAt = now.AddDays(-15) },
        };
        db.MedicalRecords.AddRange(records);
        db.SaveChanges();

        // --- Prescriptions ---
        var today = DateOnly.FromDateTime(now);
        var prescriptions = new List<Prescription>
        {
            new() { Id = 1, MedicalRecordId = 2, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = today.AddDays(-25), EndDate = today.AddDays(-11), Instructions = "Give with food. Monitor for vomiting or diarrhea.", CreatedAt = now.AddDays(-25) },
            new() { Id = 2, MedicalRecordId = 2, MedicationName = "Gabapentin", Dosage = "100mg once daily", DurationDays = 10, StartDate = today.AddDays(-25), EndDate = today.AddDays(-15), Instructions = "Administer at bedtime for pain management.", CreatedAt = now.AddDays(-25) },
            new() { Id = 3, MedicalRecordId = 3, MedicationName = "Clindamycin", Dosage = "75mg once daily", DurationDays = 7, StartDate = today.AddDays(-20), EndDate = today.AddDays(-13), Instructions = "Antibiotic for gum inflammation. Complete full course.", CreatedAt = now.AddDays(-20) },
            new() { Id = 4, MedicalRecordId = 4, MedicationName = "Meloxicam", Dosage = "0.1mg/kg once daily", DurationDays = 5, StartDate = today.AddDays(-15), EndDate = today.AddDays(-10), Instructions = "Post-extraction pain relief. Give with food.", CreatedAt = now.AddDays(-15) },
            new() { Id = 5, MedicalRecordId = 4, MedicationName = "Amoxicillin", Dosage = "250mg twice daily", DurationDays = 30, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Instructions = "Antibiotic to prevent post-surgical infection. Complete full course.", CreatedAt = now.AddDays(-15) },
        };
        db.Prescriptions.AddRange(prescriptions);
        db.SaveChanges();

        // --- Vaccinations ---
        var vaccinations = new List<Vaccination>
        {
            // Current
            new() { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6), BatchNumber = "RAB-2024-A1", AdministeredByVetId = 1, Notes = "3-year rabies vaccine", CreatedAt = now.AddMonths(-6) },
            new() { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6), BatchNumber = "DHPP-2024-B2", AdministeredByVetId = 1, CreatedAt = now.AddMonths(-6) },
            // Expiring soon (within 30 days)
            new() { Id = 3, PetId = 3, VaccineName = "Rabies", DateAdministered = today.AddYears(-1), ExpirationDate = today.AddDays(15), BatchNumber = "RAB-2023-C3", AdministeredByVetId = 2, Notes = "1-year rabies vaccine — due for renewal soon", CreatedAt = now.AddYears(-1) },
            new() { Id = 4, PetId = 5, VaccineName = "Bordetella", DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(20), BatchNumber = "BOR-2024-D4", AdministeredByVetId = 1, CreatedAt = now.AddMonths(-11) },
            // Expired
            new() { Id = 5, PetId = 7, VaccineName = "DHPP", DateAdministered = today.AddYears(-2), ExpirationDate = today.AddDays(-30), BatchNumber = "DHPP-2022-E5", AdministeredByVetId = 3, Notes = "Overdue for booster", CreatedAt = now.AddYears(-2) },
            new() { Id = 6, PetId = 2, VaccineName = "FVRCP", DateAdministered = today.AddMonths(-14), ExpirationDate = today.AddDays(-60), BatchNumber = "FVR-2023-F6", AdministeredByVetId = 1, Notes = "Feline core vaccine — expired, needs renewal", CreatedAt = now.AddMonths(-14) },
        };
        db.Vaccinations.AddRange(vaccinations);
        db.SaveChanges();
    }
}
