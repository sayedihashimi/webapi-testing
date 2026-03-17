using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(VetClinicDbContext db)
    {
        if (db.Owners.Any()) return;

        // Owners
        var owners = new List<Owner>
        {
            new() { FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701" },
            new() { FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201" },
            new() { FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301" },
            new() { FirstName = "David", LastName = "Kim", Email = "david.kim@email.com", Phone = "555-0104", Address = "321 Elm Boulevard", City = "Seattle", State = "WA", ZipCode = "98101" },
            new() { FirstName = "Jessica", LastName = "Patel", Email = "jessica.patel@email.com", Phone = "555-0105", Address = "654 Birch Lane", City = "Denver", State = "CO", ZipCode = "80201" },
        };
        db.Owners.AddRange(owners);
        await db.SaveChangesAsync();

        // Pets
        var pets = new List<Pet>
        {
            new() { Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", OwnerId = owners[0].Id },
            new() { Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m, Color = "Cream", MicrochipNumber = "MC-002-2019", OwnerId = owners[0].Id },
            new() { Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", OwnerId = owners[1].Id },
            new() { Name = "Luna", Species = "Cat", Breed = "Persian", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 3.9m, Color = "White", OwnerId = owners[2].Id },
            new() { Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2021, 11, 3), Weight = 12.5m, Color = "Tricolor", MicrochipNumber = "MC-005-2021", OwnerId = owners[2].Id },
            new() { Name = "Kiwi", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.09m, Color = "Yellow and Grey", OwnerId = owners[3].Id },
            new() { Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 20), Weight = 1.8m, Color = "Brown", OwnerId = owners[3].Id },
            new() { Name = "Rocky", Species = "Dog", Breed = "Bulldog", DateOfBirth = new DateOnly(2020, 9, 5), Weight = 25.0m, Color = "Brindle", MicrochipNumber = "MC-008-2020", OwnerId = owners[4].Id },
        };
        db.Pets.AddRange(pets);
        await db.SaveChangesAsync();

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", HireDate = new DateOnly(2015, 6, 1) },
            new() { FirstName = "Dr. Robert", LastName = "Martinez", Email = "robert.martinez@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", HireDate = new DateOnly(2018, 3, 15) },
            new() { FirstName = "Dr. Lisa", LastName = "Thompson", Email = "lisa.thompson@happypaws.com", Phone = "555-0203", Specialization = "Exotic Animals", LicenseNumber = "VET-2020-003", HireDate = new DateOnly(2020, 1, 10) },
        };
        db.Veterinarians.AddRange(vets);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Appointments — mix of statuses
        var appointments = new List<Appointment>
        {
            // Completed appointments (past)
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-30), Status = AppointmentStatus.Completed, Reason = "Annual checkup and vaccinations", Notes = "All vaccines up to date" },
            new() { PetId = pets[2].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.AddDays(-20), Status = AppointmentStatus.Completed, Reason = "Limping on left front leg", Notes = "X-ray performed" },
            new() { PetId = pets[1].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-15), Status = AppointmentStatus.Completed, Reason = "Dental cleaning", Notes = "Two teeth extracted" },
            new() { PetId = pets[4].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-10), Status = AppointmentStatus.Completed, Reason = "Skin irritation and scratching", Notes = "Allergy panel ordered" },
            // Cancelled
            new() { PetId = pets[3].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(-5), Status = AppointmentStatus.Cancelled, Reason = "Eye discharge", CancellationReason = "Owner rescheduled due to travel" },
            // Scheduled (future)
            new() { PetId = pets[5].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(3), Status = AppointmentStatus.Scheduled, Reason = "Feather plucking behavior" },
            new() { PetId = pets[6].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(5), Status = AppointmentStatus.Scheduled, Reason = "Routine wellness exam" },
            new() { PetId = pets[7].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.AddDays(7), Status = AppointmentStatus.Scheduled, Reason = "Hip evaluation" },
            // NoShow
            new() { PetId = pets[3].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-7), Status = AppointmentStatus.NoShow, Reason = "Follow-up for eye discharge" },
            // Today scheduled
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.Date.AddHours(14), Status = AppointmentStatus.Scheduled, Reason = "Follow-up from annual checkup" },
        };
        db.Appointments.AddRange(appointments);
        await db.SaveChangesAsync();

        // Medical records for completed appointments
        var records = new List<MedicalRecord>
        {
            new() { AppointmentId = appointments[0].Id, PetId = pets[0].Id, VeterinarianId = vets[0].Id, Diagnosis = "Healthy — no concerns", Treatment = "Administered rabies and DHPP vaccines. Heartworm test negative.", Notes = "Weight stable. Recommend dental cleaning in 6 months.", FollowUpDate = today.AddMonths(6) },
            new() { AppointmentId = appointments[1].Id, PetId = pets[2].Id, VeterinarianId = vets[1].Id, Diagnosis = "Mild sprain of left carpal joint", Treatment = "Anti-inflammatory medication prescribed. Rest for 2 weeks. Cold compress recommended.", Notes = "No fracture on X-ray. Recheck in 2 weeks if not improving.", FollowUpDate = today.AddDays(14) },
            new() { AppointmentId = appointments[2].Id, PetId = pets[1].Id, VeterinarianId = vets[0].Id, Diagnosis = "Periodontal disease grade 2", Treatment = "Full dental cleaning performed under anesthesia. Extracted two pre-molars with advanced decay.", Notes = "Soft food for one week. Antibiotics prescribed." },
            new() { AppointmentId = appointments[3].Id, PetId = pets[4].Id, VeterinarianId = vets[0].Id, Diagnosis = "Suspected environmental allergy", Treatment = "Antihistamine and medicated shampoo prescribed. Allergy panel blood work submitted.", Notes = "Results expected in 5-7 days.", FollowUpDate = today.AddDays(7) },
        };
        db.MedicalRecords.AddRange(records);
        await db.SaveChangesAsync();

        // Prescriptions
        var prescriptions = new List<Prescription>
        {
            new() { MedicalRecordId = records[1].Id, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = today.AddDays(-20), Instructions = "Give with food. Do not combine with other NSAIDs." },
            new() { MedicalRecordId = records[2].Id, MedicationName = "Amoxicillin", Dosage = "50mg twice daily", DurationDays = 10, StartDate = today.AddDays(-15), Instructions = "Complete full course even if symptoms improve." },
            new() { MedicalRecordId = records[3].Id, MedicationName = "Diphenhydramine", Dosage = "25mg once daily", DurationDays = 30, StartDate = today.AddDays(-10), Instructions = "Administer in the evening. May cause drowsiness." },
            new() { MedicalRecordId = records[3].Id, MedicationName = "Chlorhexidine Shampoo", Dosage = "Apply topically every 3 days", DurationDays = 21, StartDate = today.AddDays(-10), Instructions = "Lather and let sit 10 minutes before rinsing." },
            new() { MedicalRecordId = records[0].Id, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 365, StartDate = today.AddDays(-30), Instructions = "Give on the same day each month." },
        };
        db.Prescriptions.AddRange(prescriptions);
        await db.SaveChangesAsync();

        // Vaccinations
        var vaccinations = new List<Vaccination>
        {
            // Current
            new() { PetId = pets[0].Id, VaccineName = "Rabies", DateAdministered = today.AddDays(-30), ExpirationDate = today.AddYears(1), BatchNumber = "RAB-2024-A101", AdministeredByVetId = vets[0].Id, Notes = "3-year rabies vaccine" },
            new() { PetId = pets[0].Id, VaccineName = "DHPP", DateAdministered = today.AddDays(-30), ExpirationDate = today.AddYears(1), BatchNumber = "DHPP-2024-B202", AdministeredByVetId = vets[0].Id },
            // Expiring soon (within 30 days)
            new() { PetId = pets[2].Id, VaccineName = "Rabies", DateAdministered = today.AddYears(-1), ExpirationDate = today.AddDays(15), BatchNumber = "RAB-2023-C303", AdministeredByVetId = vets[0].Id, Notes = "Due for renewal" },
            new() { PetId = pets[4].Id, VaccineName = "Bordetella", DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(20), BatchNumber = "BOR-2024-D404", AdministeredByVetId = vets[0].Id },
            // Expired
            new() { PetId = pets[1].Id, VaccineName = "FVRCP", DateAdministered = today.AddYears(-2), ExpirationDate = today.AddDays(-30), BatchNumber = "FVR-2022-E505", AdministeredByVetId = vets[0].Id, Notes = "Overdue for booster" },
            new() { PetId = pets[7].Id, VaccineName = "Rabies", DateAdministered = today.AddYears(-2), ExpirationDate = today.AddDays(-60), BatchNumber = "RAB-2022-F606", AdministeredByVetId = vets[1].Id },
        };
        db.Vaccinations.AddRange(vaccinations);
        await db.SaveChangesAsync();
    }
}
