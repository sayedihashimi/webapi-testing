using Microsoft.EntityFrameworkCore;
using SparkEvents.Models;

namespace SparkEvents.Data;

public sealed class SparkEventsDbContext(DbContextOptions<SparkEventsDbContext> options) : DbContext(options)
{
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Attendee> Attendees => Set<Attendee>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventCategory>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.Property(v => v.CreatedAt).HasDefaultValueSql("datetime('now')");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasOne(e => e.EventCategory)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.EventCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.HasOne(t => t.Event)
                .WithMany(e => e.TicketTypes)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attendee>(entity =>
        {
            entity.HasIndex(a => a.Email).IsUnique();
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasIndex(r => r.ConfirmationNumber).IsUnique();

            entity.HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Attendee)
                .WithMany(a => a.Registrations)
                .HasForeignKey(r => r.AttendeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.TicketType)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasIndex(c => c.RegistrationId).IsUnique();

            entity.HasOne(c => c.Registration)
                .WithOne(r => r.CheckIn)
                .HasForeignKey<CheckIn>(c => c.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is Event e)
                {
                    e.CreatedAt = now;
                    e.UpdatedAt = now;
                }
                else if (entry.Entity is Attendee a)
                {
                    a.CreatedAt = now;
                    a.UpdatedAt = now;
                }
                else if (entry.Entity is Registration r)
                {
                    r.CreatedAt = now;
                    r.UpdatedAt = now;
                    if (r.RegistrationDate == default)
                    {
                        r.RegistrationDate = now;
                    }
                }
                else if (entry.Entity is Venue v)
                {
                    v.CreatedAt = now;
                }
                else if (entry.Entity is TicketType t)
                {
                    t.CreatedAt = now;
                }
                else if (entry.Entity is CheckIn ci)
                {
                    if (ci.CheckInTime == default)
                    {
                        ci.CheckInTime = now;
                    }
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is Event e)
                {
                    e.UpdatedAt = now;
                }
                else if (entry.Entity is Attendee a)
                {
                    a.UpdatedAt = now;
                }
                else if (entry.Entity is Registration r)
                {
                    r.UpdatedAt = now;
                }
            }
        }
    }
}
