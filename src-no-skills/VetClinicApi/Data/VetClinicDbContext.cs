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

        // Owner
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasMany(e => e.Pets).WithOne(p => p.Owner).HasForeignKey(p => p.OwnerId);
        });

        // Pet
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasIndex(e => e.MicrochipNumber).IsUnique().HasFilter("MicrochipNumber IS NOT NULL");
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.Ignore(e => e.MedicalRecords); // navigated via Appointments
            entity.HasMany(e => e.Appointments).WithOne(a => a.Pet).HasForeignKey(a => a.PetId);
            entity.HasMany(e => e.Vaccinations).WithOne(v => v.Pet).HasForeignKey(v => v.PetId);
        });

        // Veterinarian
        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.HasMany(e => e.Appointments).WithOne(a => a.Veterinarian).HasForeignKey(a => a.VeterinarianId);
        });

        // Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.MedicalRecord).WithOne(m => m.Appointment).HasForeignKey<MedicalRecord>(m => m.AppointmentId);
        });

        // MedicalRecord
        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.HasOne(e => e.Pet).WithMany().HasForeignKey(e => e.PetId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Veterinarian).WithMany().HasForeignKey(e => e.VeterinarianId).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(e => e.Prescriptions).WithOne(p => p.MedicalRecord).HasForeignKey(p => p.MedicalRecordId);
        });

        // Prescription - ignore computed properties
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.Ignore(e => e.EndDate);
            entity.Ignore(e => e.IsActive);
        });

        // Vaccination
        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasOne(e => e.AdministeredByVet).WithMany().HasForeignKey(e => e.AdministeredByVetId).OnDelete(DeleteBehavior.NoAction);
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsDueSoon);
        });
    }
}
