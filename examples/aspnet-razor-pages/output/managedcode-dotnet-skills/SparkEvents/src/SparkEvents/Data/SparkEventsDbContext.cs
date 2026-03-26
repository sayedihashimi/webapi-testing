using Microsoft.EntityFrameworkCore;
using SparkEvents.Models;

namespace SparkEvents.Data;

public class SparkEventsDbContext(DbContextOptions<SparkEventsDbContext> options) : DbContext(options)
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
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasIndex(e => new { e.Status, e.StartDate });

            entity.Property(e => e.Status)
                .HasConversion<string>();

            entity.HasOne(e => e.EventCategory)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.EventCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.Property(t => t.Price).HasPrecision(10, 2);
            entity.Property(t => t.EarlyBirdPrice).HasPrecision(10, 2);

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
            entity.HasIndex(r => r.Status);

            // Prevent duplicate active registrations for the same attendee and event
            entity.HasIndex(r => new { r.AttendeeId, r.EventId })
                .IsUnique()
                .HasFilter("\"Status\" <> 'Cancelled'");

            entity.Property(r => r.Status)
                .HasConversion<string>();

            entity.Property(r => r.AmountPaid).HasPrecision(10, 2);

            entity.HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Attendee)
                .WithMany(a => a.Registrations)
                .HasForeignKey(r => r.AttendeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.TicketType)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);
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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Added)
            {
                if (entry.Metadata.FindProperty("CreatedAt") is not null)
                    entry.Property("CreatedAt").CurrentValue = now;
                if (entry.Metadata.FindProperty("UpdatedAt") is not null)
                    entry.Property("UpdatedAt").CurrentValue = now;
            }
            else if (entry.State is EntityState.Modified)
            {
                if (entry.Metadata.FindProperty("UpdatedAt") is not null)
                    entry.Property("UpdatedAt").CurrentValue = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
