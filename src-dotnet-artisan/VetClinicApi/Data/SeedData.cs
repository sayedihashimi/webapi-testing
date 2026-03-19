using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class SeedData
{
    public static void Initialize(VetClinicDbContext context)
    {
        if (context.Owners.Any())
        {
            return;
        }

        var owners = new List<Owner>
        {
            new() { FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701" },
            new() { FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201" },
            new() { FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301" },
            new() { FirstName = "James", LastName = "Wilson", Email = "james.wilson@email.com", Phone = "555-0104", Address = "321 Elm Drive", City = "Denver", State = "CO", ZipCode = "80201" },
            new() { FirstName = "Lisa", LastName = "Thompson", Email = "lisa.thompson@email.com", Phone = "555-0105", Address = "654 Birch Lane", City = "Seattle", State = "WA", ZipCode = "98101" },
        };
        context.Owners.AddRange(owners);
        context.SaveChanges();

        var pets = new List<Pet>
        {
            new() { Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", OwnerId = owners[0].Id },
            new() { Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m, Color = "Cream", MicrochipNumber = "MC-002-2019", OwnerId = owners[0].Id },
            new() { Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.2m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", OwnerId = owners[1].Id },
            new() { Name = "Luna", Species = "Cat", Breed = "Persian", DateOfBirth = new DateOnly(2022, 5, 3), Weight = 3.9m, Color = "White", OwnerId = owners[2].Id },
            new() { Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2021, 8, 18), Weight = 12.3m, Color = "Tricolor", MicrochipNumber = "MC-005-2021", OwnerId = owners[2].Id },
            new() { Name = "Coco", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 1.8m, Color = "Brown", OwnerId = owners[3].Id },
            new() { Name = "Rocky", Species = "Dog", Breed = "Bulldog", DateOfBirth = new DateOnly(2020, 11, 5), Weight = 25.0m, Color = "Brindle", MicrochipNumber = "MC-007-2020", OwnerId = owners[4].Id },
            new() { Name = "Mittens", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2021, 4, 28), Weight = 6.1m, Color = "Tabby", MicrochipNumber = "MC-008-2021", OwnerId = owners[4].Id },
        };
        context.Pets.AddRange(pets);
        context.SaveChanges();

        var vets = new List<Veterinarian>
        {
            new() { FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", HireDate = new DateOnly(2015, 6, 1) },
            new() { FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", HireDate = new DateOnly(2018, 3, 15) },
            new() { FirstName = "Dr. Maria", LastName = "Santos", Email = "maria.santos@happypaws.com", Phone = "555-0203", Specialization = "Exotic Animals", LicenseNumber = "VET-2020-003", HireDate = new DateOnly(2020, 9, 1) },
        };
        context.Veterinarians.AddRange(vets);
        context.SaveChanges();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var appointments = new List<Appointment>
        {
            // Completed appointments (past)
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-30), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual wellness checkup", Notes = "Healthy dog, all vitals normal" },
            new() { PetId = pets[1].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-25), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", Notes = "Minor tartar buildup cleaned" },
            new() { PetId = pets[2].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.AddDays(-20), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on front left leg", Notes = "Minor sprain, prescribed rest" },
            new() { PetId = pets[5].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(-15), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Routine exotic pet exam", Notes = "Rabbit in good health" },
            // Cancelled appointment
            new() { PetId = pets[3].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-10), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Vaccination booster", CancellationReason = "Owner had scheduling conflict" },
            // NoShow appointment
            new() { PetId = pets[4].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-5), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Follow-up visit" },
            // Future scheduled appointments
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(2), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Vaccination booster" },
            new() { PetId = pets[6].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.AddDays(5), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Skin allergy examination" },
            new() { PetId = pets[7].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(7), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Annual wellness checkup" },
            new() { PetId = pets[3].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(10), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Eye discharge examination" },
        };
        context.Appointments.AddRange(appointments);
        context.SaveChanges();

        var medicalRecords = new List<MedicalRecord>
        {
            new() { AppointmentId = appointments[0].Id, PetId = pets[0].Id, VeterinarianId = vets[0].Id, Diagnosis = "Healthy - no issues found", Treatment = "Annual vaccination administered (DHPP, Rabies)", Notes = "Weight stable, coat healthy", FollowUpDate = today.AddMonths(12) },
            new() { AppointmentId = appointments[1].Id, PetId = pets[1].Id, VeterinarianId = vets[0].Id, Diagnosis = "Mild periodontal disease - Stage 1", Treatment = "Professional dental cleaning, fluoride application", Notes = "Recommend dental treats and regular brushing" },
            new() { AppointmentId = appointments[2].Id, PetId = pets[2].Id, VeterinarianId = vets[1].Id, Diagnosis = "Grade 1 sprain - left forelimb", Treatment = "Rest for 2 weeks, anti-inflammatory medication", Notes = "Restrict activity, recheck in 2 weeks", FollowUpDate = today.AddDays(-6) },
            new() { AppointmentId = appointments[3].Id, PetId = pets[5].Id, VeterinarianId = vets[2].Id, Diagnosis = "Healthy rabbit - normal exam", Treatment = "Nail trim, ear cleaning", Notes = "Diet and weight appropriate" },
        };
        context.MedicalRecords.AddRange(medicalRecords);
        context.SaveChanges();

        var prescriptions = new List<Prescription>
        {
            new() { MedicalRecordId = medicalRecords[2].Id, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = today.AddDays(-20), Instructions = "Give with food" },
            new() { MedicalRecordId = medicalRecords[2].Id, MedicationName = "Glucosamine Supplement", Dosage = "500mg once daily", DurationDays = 30, StartDate = today.AddDays(-20), Instructions = "Mix with food" },
            new() { MedicalRecordId = medicalRecords[1].Id, MedicationName = "Chlorhexidine Oral Rinse", Dosage = "5ml twice daily", DurationDays = 10, StartDate = today.AddDays(-25), Instructions = "Apply to gums with applicator" },
            new() { MedicalRecordId = medicalRecords[0].Id, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 180, StartDate = today.AddDays(-30), Instructions = "Give on the first of each month" },
            new() { MedicalRecordId = medicalRecords[3].Id, MedicationName = "Vitamin C Supplement", Dosage = "50mg daily", DurationDays = 60, StartDate = today.AddDays(-15), Instructions = "Dissolve in water bottle" },
        };
        context.Prescriptions.AddRange(prescriptions);
        context.SaveChanges();

        var vaccinations = new List<Vaccination>
        {
            // Current vaccinations
            new() { PetId = pets[0].Id, VaccineName = "Rabies", DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6), BatchNumber = "RAB-2024-1001", AdministeredByVetId = vets[0].Id, Notes = "3-year rabies vaccine" },
            new() { PetId = pets[0].Id, VaccineName = "DHPP", DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6), BatchNumber = "DHPP-2024-2001", AdministeredByVetId = vets[0].Id },
            // Expiring soon (within 30 days)
            new() { PetId = pets[2].Id, VaccineName = "Rabies", DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(20), BatchNumber = "RAB-2024-1002", AdministeredByVetId = vets[1].Id },
            new() { PetId = pets[1].Id, VaccineName = "FVRCP", DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(15), BatchNumber = "FVR-2024-3001", AdministeredByVetId = vets[0].Id },
            // Expired
            new() { PetId = pets[4].Id, VaccineName = "Bordetella", DateAdministered = today.AddMonths(-14), ExpirationDate = today.AddDays(-30), BatchNumber = "BOR-2023-4001", AdministeredByVetId = vets[0].Id },
            new() { PetId = pets[6].Id, VaccineName = "Rabies", DateAdministered = today.AddMonths(-15), ExpirationDate = today.AddDays(-60), BatchNumber = "RAB-2023-1003", AdministeredByVetId = vets[1].Id, Notes = "Overdue for renewal" },
        };
        context.Vaccinations.AddRange(vaccinations);
        context.SaveChanges();
    }
}
