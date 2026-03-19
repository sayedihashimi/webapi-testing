using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public sealed class VetClinicDbContext(DbContextOptions<VetClinicDbContext> options)
    : DbContext(options)
{
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureOwner(modelBuilder);
        ConfigurePet(modelBuilder);
        ConfigureVeterinarian(modelBuilder);
        ConfigureAppointment(modelBuilder);
        ConfigureMedicalRecord(modelBuilder);
        ConfigurePrescription(modelBuilder);
        ConfigureVaccination(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigureOwner(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).IsRequired();
            entity.Property(e => e.State).HasMaxLength(2);
        });
    }

    private static void ConfigurePet(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Species).IsRequired();
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.MicrochipNumber).IsUnique().HasFilter("[MicrochipNumber] IS NOT NULL");
            entity.HasOne(e => e.Owner)
                .WithMany(o => o.Pets)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureVeterinarian(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).IsRequired();
            entity.Property(e => e.LicenseNumber).IsRequired();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
        });
    }

    private static void ConfigureAppointment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Appointments)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Veterinarian)
                .WithMany(v => v.Appointments)
                .HasForeignKey(e => e.VeterinarianId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureMedicalRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.Property(e => e.Diagnosis).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Treatment).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.HasOne(e => e.Appointment)
                .WithOne(a => a.MedicalRecord)
                .HasForeignKey<MedicalRecord>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Pet)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Veterinarian)
                .WithMany()
                .HasForeignKey(e => e.VeterinarianId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePrescription(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MedicationName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Dosage).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(500);
            entity.HasOne(e => e.MedicalRecord)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(e => e.MedicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureVaccination(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VaccineName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Vaccinations)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AdministeredByVet)
                .WithMany()
                .HasForeignKey(e => e.AdministeredByVetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        // Owners
        modelBuilder.Entity<Owner>().HasData(
            new Owner { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Ave", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Blvd", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 4, FirstName = "James", LastName = "Wilson", Email = "james.wilson@email.com", Phone = "555-0104", Address = "321 Elm Drive", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 5, FirstName = "Lisa", LastName = "Patel", Email = "lisa.patel@email.com", Phone = "555-0105", Address = "654 Cedar Lane", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = now, UpdatedAt = now }
        );

        // Pets
        modelBuilder.Entity<Pet>().HasData(
            new Pet { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.2m, Color = "Cream", MicrochipNumber = "MC-002-2019", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", IsActive = true, OwnerId = 2, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 4, Name = "Luna", Species = "Cat", Breed = "Persian", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 3.8m, Color = "White", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2018, 11, 30), Weight = 12.5m, Color = "Tricolor", MicrochipNumber = "MC-005-2018", IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Canary", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.025m, Color = "Yellow", IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 7, Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 1), Weight = 1.8m, Color = "Brown", MicrochipNumber = "MC-007-2023", IsActive = true, OwnerId = 5, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 8, Name = "Shadow", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2017, 9, 20), Weight = 6.5m, Color = "Gray Tabby", MicrochipNumber = "MC-008-2017", IsActive = false, OwnerId = 5, CreatedAt = now, UpdatedAt = now }
        );

        // Veterinarians
        modelBuilder.Entity<Veterinarian>().HasData(
            new Veterinarian { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new Veterinarian { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new Veterinarian { Id = 3, FirstName = "Dr. Maria", LastName = "Santos", Email = "maria.santos@happypaws.com", Phone = "555-0203", Specialization = "Exotic Animals", LicenseNumber = "VET-2020-003", IsAvailable = true, HireDate = new DateOnly(2020, 1, 10) }
        );

        // Appointments (mix of statuses, some future)
        modelBuilder.Entity<Appointment>().HasData(
            new Appointment { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 10, 9, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup", Notes = "All vitals normal", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 2, PetId = 3, VeterinarianId = 2, AppointmentDate = new DateTime(2025, 1, 12, 10, 0, 0, DateTimeKind.Utc), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on right front leg", Notes = "X-ray performed", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 3, PetId = 2, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 13, 14, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 4, PetId = 5, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 14, 11, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Vaccination update", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 5, PetId = 4, VeterinarianId = 3, AppointmentDate = new DateTime(2025, 1, 8, 15, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Eye irritation", CancellationReason = "Owner had emergency", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 6, PetId = 1, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 7, 20, 9, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up checkup", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 7, PetId = 6, VeterinarianId = 3, AppointmentDate = new DateTime(2025, 7, 21, 10, 0, 0, DateTimeKind.Utc), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Wing feather trim", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 8, PetId = 7, VeterinarianId = 3, AppointmentDate = new DateTime(2025, 7, 22, 11, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Nail trimming and checkup", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 9, PetId = 3, VeterinarianId = 2, AppointmentDate = new DateTime(2025, 7, 23, 14, 0, 0, DateTimeKind.Utc), DurationMinutes = 60, Status = AppointmentStatus.Scheduled, Reason = "Post-surgery follow up", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 10, PetId = 5, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 9, 10, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Regular checkup", CreatedAt = now, UpdatedAt = now }
        );

        // Medical Records
        modelBuilder.Entity<MedicalRecord>().HasData(
            new MedicalRecord { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy - no issues found", Treatment = "None required. Continue current diet and exercise.", Notes = "Weight stable, teeth in good condition", FollowUpDate = new DateOnly(2025, 7, 10), CreatedAt = now },
            new MedicalRecord { Id = 2, AppointmentId = 2, PetId = 3, VeterinarianId = 2, Diagnosis = "Mild sprain in right front leg", Treatment = "Anti-inflammatory medication prescribed. Rest for 2 weeks. Ice 15 min twice daily.", Notes = "No fracture visible on X-ray", FollowUpDate = new DateOnly(2025, 1, 26), CreatedAt = now },
            new MedicalRecord { Id = 3, AppointmentId = 3, PetId = 2, VeterinarianId = 1, Diagnosis = "Mild tartar buildup", Treatment = "Professional dental cleaning performed. Recommend dental treats.", CreatedAt = now },
            new MedicalRecord { Id = 4, AppointmentId = 4, PetId = 5, VeterinarianId = 1, Diagnosis = "Due for DHPP booster", Treatment = "Administered DHPP vaccine booster. Observed for 15 minutes post-injection.", Notes = "No adverse reaction", CreatedAt = now }
        );

        // Prescriptions
        modelBuilder.Entity<Prescription>().HasData(
            new Prescription { Id = 1, MedicalRecordId = 2, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = new DateOnly(2025, 1, 12), EndDate = new DateOnly(2025, 1, 26), Instructions = "Give with food. Monitor for stomach upset.", CreatedAt = now },
            new Prescription { Id = 2, MedicalRecordId = 2, MedicationName = "Glucosamine Supplement", Dosage = "500mg once daily", DurationDays = 30, StartDate = new DateOnly(2025, 1, 12), EndDate = new DateOnly(2025, 2, 11), Instructions = "Mix with food. Long-term joint support.", CreatedAt = now },
            new Prescription { Id = 3, MedicalRecordId = 3, MedicationName = "Chlorhexidine Oral Rinse", Dosage = "5ml twice daily", DurationDays = 7, StartDate = new DateOnly(2025, 1, 13), EndDate = new DateOnly(2025, 1, 20), Instructions = "Apply directly to gums after meals.", CreatedAt = now },
            new Prescription { Id = 4, MedicalRecordId = 1, MedicationName = "Heartworm Prevention", Dosage = "1 tablet monthly", DurationDays = 180, StartDate = new DateOnly(2025, 1, 10), EndDate = new DateOnly(2025, 7, 9), Instructions = "Give on the same day each month with a meal.", CreatedAt = now },
            new Prescription { Id = 5, MedicalRecordId = 1, MedicationName = "Fish Oil Supplement", Dosage = "1000mg once daily", DurationDays = 90, StartDate = new DateOnly(2024, 10, 1), EndDate = new DateOnly(2024, 12, 30), Instructions = "For coat health. Can be mixed with food.", CreatedAt = now }
        );

        // Vaccinations
        modelBuilder.Entity<Vaccination>().HasData(
            new Vaccination { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = new DateOnly(2024, 3, 15), ExpirationDate = new DateOnly(2027, 3, 15), BatchNumber = "RAB-2024-1001", AdministeredByVetId = 1, Notes = "3-year rabies vaccine", CreatedAt = now },
            new Vaccination { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = new DateOnly(2025, 1, 10), ExpirationDate = new DateOnly(2026, 1, 10), BatchNumber = "DHPP-2025-2001", AdministeredByVetId = 1, CreatedAt = now },
            new Vaccination { Id = 3, PetId = 2, VaccineName = "FVRCP", DateAdministered = new DateOnly(2024, 7, 22), ExpirationDate = new DateOnly(2025, 7, 22), BatchNumber = "FVRCP-2024-3001", AdministeredByVetId = 1, Notes = "Annual feline distemper combo", CreatedAt = now },
            new Vaccination { Id = 4, PetId = 3, VaccineName = "Rabies", DateAdministered = new DateOnly(2024, 1, 10), ExpirationDate = new DateOnly(2025, 1, 10), BatchNumber = "RAB-2024-1002", AdministeredByVetId = 2, Notes = "1-year rabies vaccine - expired, needs renewal", CreatedAt = now },
            new Vaccination { Id = 5, PetId = 5, VaccineName = "DHPP", DateAdministered = new DateOnly(2025, 1, 14), ExpirationDate = new DateOnly(2026, 1, 14), BatchNumber = "DHPP-2025-2002", AdministeredByVetId = 1, CreatedAt = now },
            new Vaccination { Id = 6, PetId = 4, VaccineName = "Rabies", DateAdministered = new DateOnly(2024, 5, 8), ExpirationDate = new DateOnly(2025, 5, 8), BatchNumber = "RAB-2024-1003", AdministeredByVetId = 3, Notes = "Due for renewal soon", CreatedAt = now }
        );
    }
}
