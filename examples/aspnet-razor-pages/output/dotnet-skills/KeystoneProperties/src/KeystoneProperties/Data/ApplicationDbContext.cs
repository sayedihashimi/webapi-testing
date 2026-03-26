using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<Inspection> Inspections => Set<Inspection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Property
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasMany(p => p.Units)
                  .WithOne(u => u.Property)
                  .HasForeignKey(u => u.PropertyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Unit - unique constraint on (PropertyId, UnitNumber)
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasIndex(u => new { u.PropertyId, u.UnitNumber }).IsUnique();

            entity.Property(u => u.MonthlyRent).HasColumnType("decimal(18,2)");
            entity.Property(u => u.DepositAmount).HasColumnType("decimal(18,2)");
            entity.Property(u => u.Bathrooms).HasColumnType("decimal(3,1)");

            entity.HasMany(u => u.Leases)
                  .WithOne(l => l.Unit)
                  .HasForeignKey(l => l.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.MaintenanceRequests)
                  .WithOne(m => m.Unit)
                  .HasForeignKey(m => m.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.Inspections)
                  .WithOne(i => i.Unit)
                  .HasForeignKey(i => i.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Tenant - unique email
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(t => t.Email).IsUnique();

            entity.HasMany(t => t.Leases)
                  .WithOne(l => l.Tenant)
                  .HasForeignKey(l => l.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(t => t.MaintenanceRequests)
                  .WithOne(m => m.Tenant)
                  .HasForeignKey(m => m.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Lease
        modelBuilder.Entity<Lease>(entity =>
        {
            entity.Property(l => l.MonthlyRentAmount).HasColumnType("decimal(18,2)");
            entity.Property(l => l.DepositAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(l => l.RenewalOfLease)
                  .WithMany()
                  .HasForeignKey(l => l.RenewalOfLeaseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(l => l.Payments)
                  .WithOne(p => p.Lease)
                  .HasForeignKey(p => p.LeaseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(l => l.Inspections)
                  .WithOne(i => i.Lease)
                  .HasForeignKey(i => i.LeaseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        });

        // MaintenanceRequest
        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.Property(m => m.EstimatedCost).HasColumnType("decimal(18,2)");
            entity.Property(m => m.ActualCost).HasColumnType("decimal(18,2)");
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Property p)
            {
                p.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) p.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Unit u)
            {
                u.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) u.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Tenant t)
            {
                t.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) t.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Lease l)
            {
                l.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) l.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is MaintenanceRequest m)
            {
                m.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) m.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Payment pay)
            {
                if (entry.State == EntityState.Added) pay.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Inspection i)
            {
                if (entry.State == EntityState.Added) i.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
