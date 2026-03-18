using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public class VetClinicDbContext(DbContextOptions<VetClinicDbContext> options) : DbContext(options)
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
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).IsRequired();
            entity.Property(e => e.State).HasMaxLength(2);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Species).IsRequired();
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.MicrochipNumber).IsUnique().HasFilter("[MicrochipNumber] IS NOT NULL");
            entity.HasOne(e => e.Owner)
                .WithMany(o => o.Pets)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.LicenseNumber).IsRequired();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Reason).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Appointments)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Veterinarian)
                .WithMany(v => v.Appointments)
                .HasForeignKey(e => e.VeterinarianId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Diagnosis).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Treatment).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.HasIndex(e => e.AppointmentId).IsUnique();
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

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MedicationName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Dosage).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Instructions).HasMaxLength(500);
            entity.HasOne(e => e.MedicalRecord)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(e => e.MedicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VaccineName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Vaccinations)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AdministeredBy)
                .WithMany()
                .HasForeignKey(e => e.AdministeredByVetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
