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
        modelBuilder.Entity<Owner>(e =>
        {
            e.HasIndex(o => o.Email).IsUnique();
        });

        modelBuilder.Entity<Pet>(e =>
        {
            e.HasIndex(p => p.MicrochipNumber).IsUnique().HasFilter("MicrochipNumber IS NOT NULL");
            e.HasOne(p => p.Owner).WithMany(o => o.Pets).HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Veterinarian>(e =>
        {
            e.HasIndex(v => v.Email).IsUnique();
            e.HasIndex(v => v.LicenseNumber).IsUnique();
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasOne(a => a.Pet).WithMany(p => p.Appointments).HasForeignKey(a => a.PetId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Veterinarian).WithMany(v => v.Appointments).HasForeignKey(a => a.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedicalRecord>(e =>
        {
            e.HasIndex(m => m.AppointmentId).IsUnique();
            e.HasOne(m => m.Appointment).WithOne(a => a.MedicalRecord).HasForeignKey<MedicalRecord>(m => m.AppointmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Pet).WithMany(p => p.MedicalRecords).HasForeignKey(m => m.PetId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Veterinarian).WithMany().HasForeignKey(m => m.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Prescription>(e =>
        {
            e.HasOne(p => p.MedicalRecord).WithMany(m => m.Prescriptions).HasForeignKey(p => p.MedicalRecordId).OnDelete(DeleteBehavior.Cascade);
            e.Ignore(p => p.EndDate);
            e.Ignore(p => p.IsActive);
        });

        modelBuilder.Entity<Vaccination>(e =>
        {
            e.HasOne(v => v.Pet).WithMany(p => p.Vaccinations).HasForeignKey(v => v.PetId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.AdministeredByVet).WithMany().HasForeignKey(v => v.AdministeredByVetId).OnDelete(DeleteBehavior.Restrict);
            e.Ignore(v => v.IsExpired);
            e.Ignore(v => v.IsDueSoon);
        });
    }
}
