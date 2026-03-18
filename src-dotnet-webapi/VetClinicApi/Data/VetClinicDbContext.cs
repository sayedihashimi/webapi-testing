using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public class VetClinicDbContext : DbContext
{
    public VetClinicDbContext(DbContextOptions<VetClinicDbContext> options) : base(options) { }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Owner configuration
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

        // Pet configuration
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Species).IsRequired();
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.MicrochipNumber).IsUnique().HasFilter("[MicrochipNumber] IS NOT NULL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.Owner)
                  .WithMany(o => o.Pets)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Veterinarian configuration
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
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
        });

        // Appointment configuration
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.DurationMinutes).HasDefaultValue(30);
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

        // MedicalRecord configuration
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

        // Prescription configuration
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MedicationName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Dosage).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(500);
            entity.Ignore(e => e.IsActive); // Computed in code
            entity.HasOne(e => e.MedicalRecord)
                  .WithMany(mr => mr.Prescriptions)
                  .HasForeignKey(e => e.MedicalRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Vaccination configuration
        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VaccineName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Ignore(e => e.IsExpired); // Computed in code
            entity.Ignore(e => e.IsDueSoon); // Computed in code
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
}
