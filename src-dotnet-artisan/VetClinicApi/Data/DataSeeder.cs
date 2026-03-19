using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(VetClinicDbContext context)
    {
        if (context.Owners.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Owners
        var owners = new List<Owner>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701", CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0104", Address = "321 Elm Court", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Olivia", LastName = "Martinez", Email = "olivia.martinez@email.com", Phone = "555-0105", Address = "654 Birch Lane", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = now, UpdatedAt = now },
        };
        context.Owners.AddRange(owners);
        await context.SaveChangesAsync();

        // Pets
        var pets = new List<Pet>
        {
            new() { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m, Color = "Cream and Brown", MicrochipNumber = "MC-002-2019", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", IsActive = true, OwnerId = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 5), Weight = 6.2m, Color = "Tabby", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2020, 11, 30), Weight = 12.0m, Color = "Tricolor", MicrochipNumber = "MC-005-2020", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.09m, Color = "Yellow and Gray", IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Name = "Bella", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2019, 9, 8), Weight = 28.5m, Color = "Chocolate", MicrochipNumber = "MC-007-2019", IsActive = true, OwnerId = 5, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 1), Weight = 1.8m, Color = "White and Brown", IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now },
        };
        context.Pets.AddRange(pets);
        await context.SaveChangesAsync();

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new() { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new() { Id = 3, FirstName = "Dr. Lisa", LastName = "Patel", Email = "lisa.patel@happypaws.com", Phone = "555-0203", Specialization = "Dentistry", LicenseNumber = "VET-2020-003", IsAvailable = true, HireDate = new DateOnly(2020, 1, 10) },
        };
        context.Veterinarians.AddRange(vets);
        await context.SaveChangesAsync();

        // Appointments - mix of statuses
        var futureDate1 = now.AddDays(7).Date.AddHours(9);
        var futureDate2 = now.AddDays(7).Date.AddHours(10);
        var futureDate3 = now.AddDays(14).Date.AddHours(14);
        var pastDate1 = now.AddDays(-30).Date.AddHours(9);
        var pastDate2 = now.AddDays(-25).Date.AddHours(10);
        var pastDate3 = now.AddDays(-20).Date.AddHours(11);
        var pastDate4 = now.AddDays(-15).Date.AddHours(14);
        var pastDate5 = now.AddDays(-10).Date.AddHours(9);
        var todayDate1 = now.Date.AddHours(10);
        var todayDate2 = now.Date.AddHours(14);

        var appointments = new List<Appointment>
        {
            new() { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = pastDate1, DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup and vaccinations", CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, PetId = 3, VeterinarianId = 2, AppointmentDate = pastDate2, DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on right front leg", CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, PetId = 2, VeterinarianId = 1, AppointmentDate = pastDate3, DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, PetId = 5, VeterinarianId = 3, AppointmentDate = pastDate4, DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Ear infection follow-up", CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, PetId = 7, VeterinarianId = 1, AppointmentDate = pastDate5, DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Routine wellness exam", CancellationReason = "Owner had a scheduling conflict", CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, PetId = 4, VeterinarianId = 2, AppointmentDate = pastDate5.AddHours(2), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Skin rash evaluation", CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, PetId = 1, VeterinarianId = 1, AppointmentDate = todayDate1, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Booster vaccination", CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, PetId = 6, VeterinarianId = 3, AppointmentDate = todayDate2, DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Wing feather trimming and checkup", CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, PetId = 3, VeterinarianId = 1, AppointmentDate = futureDate1, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Post-surgery follow-up", CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, PetId = 7, VeterinarianId = 2, AppointmentDate = futureDate3, DurationMinutes = 60, Status = AppointmentStatus.Scheduled, Reason = "Spay surgery", CreatedAt = now, UpdatedAt = now },
        };
        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();

        // Medical Records (for completed appointments)
        var records = new List<MedicalRecord>
        {
            new() { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy - all vitals normal", Treatment = "Administered annual vaccines (DHPP, Rabies). Heartworm test negative.", Notes = "Weight is ideal. Continue current diet.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(12)), CreatedAt = now },
            new() { Id = 2, AppointmentId = 2, PetId = 3, VeterinarianId = 2, Diagnosis = "Mild sprain in right front leg", Treatment = "Anti-inflammatory medication prescribed. Rest recommended for 2 weeks.", Notes = "X-ray showed no fracture. Monitor for improvement.", FollowUpDate = DateOnly.FromDateTime(now.AddDays(14)), CreatedAt = now },
            new() { Id = 3, AppointmentId = 3, PetId = 2, VeterinarianId = 1, Diagnosis = "Mild tartar buildup, otherwise healthy gums", Treatment = "Professional dental cleaning performed under light sedation", Notes = "Recommend dental treats and regular brushing", CreatedAt = now },
            new() { Id = 4, AppointmentId = 4, PetId = 5, VeterinarianId = 3, Diagnosis = "Ear infection resolving well", Treatment = "Continue ear drops for 5 more days. Infection clearing up.", Notes = "Much improved from last visit", CreatedAt = now },
        };
        context.MedicalRecords.AddRange(records);
        await context.SaveChangesAsync();

        // Prescriptions
        var today = DateOnly.FromDateTime(now);
        var prescriptions = new List<Prescription>
        {
            new() { Id = 1, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "1 chewable tablet monthly", DurationDays = 365, StartDate = today.AddDays(-30), EndDate = today.AddDays(335), Instructions = "Give on the 1st of each month with food", CreatedAt = now },
            new() { Id = 2, MedicalRecordId = 2, MedicationName = "Carprofen", Dosage = "75mg twice daily", DurationDays = 14, StartDate = today.AddDays(-25), EndDate = today.AddDays(-11), Instructions = "Give with food. Discontinue if vomiting occurs.", CreatedAt = now },
            new() { Id = 3, MedicalRecordId = 2, MedicationName = "Tramadol", Dosage = "50mg as needed for pain", DurationDays = 7, StartDate = today.AddDays(-25), EndDate = today.AddDays(-18), Instructions = "Give up to twice daily if pet shows signs of pain", CreatedAt = now },
            new() { Id = 4, MedicalRecordId = 4, MedicationName = "Otibiotic Ointment", Dosage = "4 drops in affected ear twice daily", DurationDays = 10, StartDate = today.AddDays(-15), EndDate = today.AddDays(-5), Instructions = "Clean ear before applying. Keep ear dry.", CreatedAt = now },
            new() { Id = 5, MedicalRecordId = 4, MedicationName = "Cephalexin", Dosage = "250mg twice daily", DurationDays = 14, StartDate = today.AddDays(-15), EndDate = today.AddDays(-1), Instructions = "Complete full course even if symptoms improve", CreatedAt = now },
        };
        context.Prescriptions.AddRange(prescriptions);
        await context.SaveChangesAsync();

        // Vaccinations
        var vaccinations = new List<Vaccination>
        {
            new() { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = today.AddDays(-30), ExpirationDate = today.AddYears(1).AddDays(-30), BatchNumber = "RAB-2024-0451", AdministeredByVetId = 1, Notes = "3-year rabies vaccine", CreatedAt = now },
            new() { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = today.AddDays(-30), ExpirationDate = today.AddYears(1).AddDays(-30), BatchNumber = "DHPP-2024-0892", AdministeredByVetId = 1, CreatedAt = now },
            new() { Id = 3, PetId = 3, VaccineName = "Rabies", DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddDays(20), BatchNumber = "RAB-2024-0322", AdministeredByVetId = 2, Notes = "Due for renewal soon", CreatedAt = now },
            new() { Id = 4, PetId = 2, VaccineName = "FVRCP", DateAdministered = today.AddMonths(-14), ExpirationDate = today.AddDays(-30), BatchNumber = "FVR-2023-0567", AdministeredByVetId = 1, Notes = "Overdue - schedule renewal", CreatedAt = now },
            new() { Id = 5, PetId = 5, VaccineName = "Bordetella", DateAdministered = today.AddMonths(-10), ExpirationDate = today.AddMonths(2), BatchNumber = "BOR-2024-0189", AdministeredByVetId = 3, CreatedAt = now },
            new() { Id = 6, PetId = 7, VaccineName = "Rabies", DateAdministered = today.AddMonths(-3), ExpirationDate = today.AddMonths(9), BatchNumber = "RAB-2024-0780", AdministeredByVetId = 1, CreatedAt = now },
        };
        context.Vaccinations.AddRange(vaccinations);
        await context.SaveChangesAsync();
    }
}
