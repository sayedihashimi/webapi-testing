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
        modelBuilder.Entity<Owner>(e =>
        {
            e.HasIndex(o => o.Email).IsUnique();
            e.HasMany(o => o.Pets).WithOne(p => p.Owner).HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        // Pet
        modelBuilder.Entity<Pet>(e =>
        {
            e.HasIndex(p => p.MicrochipNumber).IsUnique().HasFilter("MicrochipNumber IS NOT NULL");
            e.Property(p => p.Weight).HasColumnType("decimal(8,2)");
        });

        // Veterinarian
        modelBuilder.Entity<Veterinarian>(e =>
        {
            e.HasIndex(v => v.Email).IsUnique();
            e.HasIndex(v => v.LicenseNumber).IsUnique();
        });

        // Appointment
        modelBuilder.Entity<Appointment>(e =>
        {
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(a => a.Pet).WithMany(p => p.Appointments).HasForeignKey(a => a.PetId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Veterinarian).WithMany(v => v.Appointments).HasForeignKey(a => a.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
        });

        // MedicalRecord
        modelBuilder.Entity<MedicalRecord>(e =>
        {
            e.HasIndex(m => m.AppointmentId).IsUnique();
            e.HasOne(m => m.Appointment).WithOne(a => a.MedicalRecord).HasForeignKey<MedicalRecord>(m => m.AppointmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Pet).WithMany(p => p.MedicalRecords).HasForeignKey(m => m.PetId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Veterinarian).WithMany().HasForeignKey(m => m.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
        });

        // Prescription
        modelBuilder.Entity<Prescription>(e =>
        {
            e.HasOne(p => p.MedicalRecord).WithMany(m => m.Prescriptions).HasForeignKey(p => p.MedicalRecordId).OnDelete(DeleteBehavior.Cascade);
            e.Ignore(p => p.IsActive); // computed in code
        });

        // Vaccination
        modelBuilder.Entity<Vaccination>(e =>
        {
            e.HasOne(v => v.Pet).WithMany(p => p.Vaccinations).HasForeignKey(v => v.PetId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.AdministeredByVet).WithMany().HasForeignKey(v => v.AdministeredByVetId).OnDelete(DeleteBehavior.Restrict);
            e.Ignore(v => v.IsExpired);
            e.Ignore(v => v.IsDueSoon);
        });
    }
}
