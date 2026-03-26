using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Properties.AnyAsync())
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        // ── Properties ──────────────────────────────────────────────────
        var mapleRidge = new Property
        {
            Name = "Maple Ridge Apartments",
            Address = "123 Maple Street",
            City = "Springfield",
            State = "IL",
            ZipCode = "62701",
            PropertyType = PropertyType.Apartment,
            YearBuilt = 2005,
            TotalUnits = 12,
            Description = "Modern apartment complex with excellent amenities",
            CreatedAt = new DateTime(2023, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var cedarPark = new Property
        {
            Name = "Cedar Park Townhomes",
            Address = "456 Cedar Lane",
            City = "Springfield",
            State = "IL",
            ZipCode = "62702",
            PropertyType = PropertyType.Townhouse,
            YearBuilt = 2010,
            TotalUnits = 8,
            Description = "Spacious townhomes in a quiet neighborhood",
            CreatedAt = new DateTime(2023, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var lakeview = new Property
        {
            Name = "Lakeview Condos",
            Address = "789 Lake Drive",
            City = "Springfield",
            State = "IL",
            ZipCode = "62703",
            PropertyType = PropertyType.Condo,
            YearBuilt = 2018,
            TotalUnits = 6,
            Description = "Luxury condos with stunning lake views",
            CreatedAt = new DateTime(2023, 6, 20, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        context.Properties.AddRange(mapleRidge, cedarPark, lakeview);
        await context.SaveChangesAsync();

        // ── Units ───────────────────────────────────────────────────────
        // Maple Ridge 1st floor (101-106)
        var mr101 = CreateUnit(mapleRidge.Id, "101", 1, 2, 1.0m, 850, 950.00m, "In-unit laundry, Balcony");
        var mr102 = CreateUnit(mapleRidge.Id, "102", 1, 1, 1.0m, 650, 900.00m, "Patio");
        var mr103 = CreateUnit(mapleRidge.Id, "103", 1, 3, 2.0m, 1100, 1400.00m, "In-unit laundry, Balcony, Dishwasher");
        var mr104 = CreateUnit(mapleRidge.Id, "104", 1, 2, 1.5m, 900, 1100.00m, "Dishwasher, Balcony");
        var mr105 = CreateUnit(mapleRidge.Id, "105", 1, 1, 1.0m, 620, 925.00m, "Patio");
        var mr106 = CreateUnit(mapleRidge.Id, "106", 1, 3, 2.0m, 1150, 1600.00m, "In-unit laundry, Balcony, Walk-in closet");

        // Maple Ridge 2nd floor (201-206)
        var mr201 = CreateUnit(mapleRidge.Id, "201", 2, 0, 1.0m, 500, 800.00m, "City view");
        var mr202 = CreateUnit(mapleRidge.Id, "202", 2, 1, 1.0m, 680, 950.00m, "Balcony");
        var mr203 = CreateUnit(mapleRidge.Id, "203", 2, 2, 1.5m, 900, 1200.00m, "In-unit laundry, Balcony");
        var mr204 = CreateUnit(mapleRidge.Id, "204", 2, 2, 2.0m, 950, 1400.00m, "In-unit laundry, Balcony, Dishwasher");
        var mr205 = CreateUnit(mapleRidge.Id, "205", 2, 1, 1.0m, 660, 900.00m, "City view");
        var mr206 = CreateUnit(mapleRidge.Id, "206", 2, 0, 1.0m, 520, 850.00m, "City view");

        // Cedar Park (A1-A4, B1-B4)
        var cpA1 = CreateUnit(cedarPark.Id, "A1", 1, 2, 2.0m, 1100, 1400.00m, "Garage, Patio, Fireplace");
        var cpA2 = CreateUnit(cedarPark.Id, "A2", 1, 3, 2.5m, 1350, 1800.00m, "Garage, Patio, Fireplace, Basement");
        var cpA3 = CreateUnit(cedarPark.Id, "A3", 1, 2, 1.5m, 1050, 1200.00m, "Garage, Patio");
        var cpA4 = CreateUnit(cedarPark.Id, "A4", 1, 3, 2.5m, 1400, 2000.00m, "Garage, Patio, Fireplace, Basement, Deck");
        var cpB1 = CreateUnit(cedarPark.Id, "B1", 1, 2, 2.0m, 1100, 1350.00m, "Garage, Patio");
        var cpB2 = CreateUnit(cedarPark.Id, "B2", 1, 3, 2.0m, 1300, 1750.00m, "Garage, Patio, Fireplace");
        var cpB3 = CreateUnit(cedarPark.Id, "B3", 1, 2, 1.5m, 1050, 1250.00m, "Garage, Patio");
        var cpB4 = CreateUnit(cedarPark.Id, "B4", 1, 2, 2.0m, 1150, 1450.00m, "Garage, Patio, Deck");

        // Lakeview (1A-1F)
        var lv1A = CreateUnit(lakeview.Id, "1A", 1, 2, 2.0m, 1000, 1500.00m, "Lake view, Balcony, In-unit laundry");
        var lv1B = CreateUnit(lakeview.Id, "1B", 1, 1, 1.0m, 750, 1100.00m, "Lake view, Balcony");
        var lv1C = CreateUnit(lakeview.Id, "1C", 1, 3, 2.0m, 1300, 2200.00m, "Lake view, Balcony, In-unit laundry, Walk-in closet");
        var lv1D = CreateUnit(lakeview.Id, "1D", 1, 2, 1.5m, 1050, 1600.00m, "Lake view, Balcony, In-unit laundry");
        var lv1E = CreateUnit(lakeview.Id, "1E", 1, 1, 1.0m, 780, 1200.00m, "Partial lake view, Balcony");
        var lv1F = CreateUnit(lakeview.Id, "1F", 1, 2, 2.0m, 1100, 1700.00m, "Lake view, Balcony, In-unit laundry");

        var allUnits = new[]
        {
            mr101, mr102, mr103, mr104, mr105, mr106,
            mr201, mr202, mr203, mr204, mr205, mr206,
            cpA1, cpA2, cpA3, cpA4, cpB1, cpB2, cpB3, cpB4,
            lv1A, lv1B, lv1C, lv1D, lv1E, lv1F
        };

        context.Units.AddRange(allUnits);
        await context.SaveChangesAsync();

        // Set unit statuses (most Occupied, some Available, one Maintenance)
        mr105.Status = UnitStatus.Available;
        mr206.Status = UnitStatus.Available;
        cpB3.Status = UnitStatus.Available;
        lv1E.Status = UnitStatus.Available;
        mr203.Status = UnitStatus.Maintenance;

        // Occupied units
        foreach (var u in allUnits)
        {
            if (u.Status == UnitStatus.Available || u.Status == UnitStatus.Maintenance)
                continue;
            u.Status = UnitStatus.Occupied;
        }

        await context.SaveChangesAsync();

        // ── Tenants ─────────────────────────────────────────────────────
        var tenants = new[]
        {
            CreateTenant("John", "Smith", "john.smith@email.com", "217-555-0101",
                new DateOnly(1985, 3, 15), "Mary Smith", "217-555-0102"),
            CreateTenant("Sarah", "Johnson", "sarah.j@email.com", "217-555-0201",
                new DateOnly(1990, 7, 22), "Tom Johnson", "217-555-0202"),
            CreateTenant("Michael", "Williams", "m.williams@email.com", "217-555-0301",
                new DateOnly(1978, 11, 8), "Linda Williams", "217-555-0302"),
            CreateTenant("Emily", "Davis", "emily.davis@email.com", "217-555-0401",
                new DateOnly(1992, 5, 30), "Robert Davis Sr.", "217-555-0402"),
            CreateTenant("Robert", "Brown", "r.brown@email.com", "217-555-0501",
                new DateOnly(1988, 1, 12), "Nancy Brown", "217-555-0502"),
            CreateTenant("Jennifer", "Wilson", "j.wilson@email.com", "217-555-0601",
                new DateOnly(1995, 9, 18), "Bill Wilson", "217-555-0602"),
            CreateTenant("David", "Martinez", "d.martinez@email.com", "217-555-0701",
                new DateOnly(1982, 4, 25), "Carmen Martinez", "217-555-0702"),
            CreateTenant("Lisa", "Anderson", "l.anderson@email.com", "217-555-0801",
                new DateOnly(1975, 12, 3), "Paul Anderson", "217-555-0802"),
            CreateTenant("James", "Taylor", "james.t@email.com", "217-555-0901",
                new DateOnly(1998, 6, 14), "Susan Taylor", "217-555-0902"),
            CreateTenant("Maria", "Garcia", "m.garcia@email.com", "217-555-1001",
                new DateOnly(1969, 8, 27), "Carlos Garcia", "217-555-1002"),
        };

        context.Tenants.AddRange(tenants);
        await context.SaveChangesAsync();

        // ── Leases ──────────────────────────────────────────────────────
        // Expired lease (original that was renewed) – John Smith in mr101
        var lease1Expired = new Lease
        {
            UnitId = mr101.Id,
            TenantId = tenants[0].Id, // John Smith
            StartDate = new DateOnly(2023, 3, 1),
            EndDate = new DateOnly(2024, 2, 29),
            MonthlyRentAmount = 900.00m,
            DepositAmount = 900.00m,
            DepositStatus = DepositStatus.Held, // rolled into renewal
            Status = LeaseStatus.Renewed,
            CreatedAt = new DateTime(2023, 2, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc)
        };

        // Active lease (renewal of lease1Expired) – John Smith in mr101
        var lease2Active = new Lease
        {
            UnitId = mr101.Id,
            TenantId = tenants[0].Id,
            StartDate = new DateOnly(2024, 3, 1),
            EndDate = new DateOnly(2025, 2, 28),
            MonthlyRentAmount = 950.00m,
            DepositAmount = 950.00m,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            CreatedAt = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc)
        };

        // Active – Sarah Johnson in mr102
        var lease3Active = new Lease
        {
            UnitId = mr102.Id,
            TenantId = tenants[1].Id,
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2025, 6, 30),
            MonthlyRentAmount = 900.00m,
            DepositAmount = 900.00m,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            CreatedAt = new DateTime(2023, 12, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2023, 12, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        // Active – Michael Williams in mr103
        var lease4Active = new Lease
        {
            UnitId = mr103.Id,
            TenantId = tenants[2].Id,
            StartDate = new DateOnly(2024, 6, 1),
            EndDate = new DateOnly(2025, 5, 31),
            MonthlyRentAmount = 1400.00m,
            DepositAmount = 1400.00m,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            CreatedAt = new DateTime(2024, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 5, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        // Active – Emily Davis in mr104
        var lease5Active = new Lease
        {
            UnitId = mr104.Id,
            TenantId = tenants[3].Id,
            StartDate = new DateOnly(2024, 4, 1),
            EndDate = new DateOnly(2025, 3, 31),
            MonthlyRentAmount = 1100.00m,
            DepositAmount = 1100.00m,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            CreatedAt = new DateTime(2024, 3, 20, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 3, 20, 0, 0, 0, DateTimeKind.Utc)
        };

        // Active – Robert Brown in cpA1
        var lease6Active = new Lease
        {
            UnitId = cpA1.Id,
            TenantId = tenants[4].Id,
            StartDate = new DateOnly(2024, 2, 1),
            EndDate = new DateOnly(2025, 1, 31),
            MonthlyRentAmount = 1400.00m,
            DepositAmount = 1400.00m,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        // Active – Jennifer Wilson in cpA2
        var lease7Active = new Lease
        {
            UnitId = cpA2.Id,
            TenantId = tenants[5].Id,
            StartDate = new DateOnly(2024, 5, 1),
            EndDate = new DateOnly(2025, 4, 30),
            MonthlyRentAmount = 1800.00m,
            DepositAmount = 1800.00m,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            CreatedAt = new DateTime(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        // Active – David Martinez in lv1A
        var lease8Active = new Lease
        {
            UnitId = lv1A.Id,
            TenantId = tenants[6].Id,
            StartDate = new DateOnly(2024, 7, 1),
            EndDate = new DateOnly(2025, 6, 30),
            MonthlyRentAmount = 1500.00m,
            DepositAmount = 1500.00m,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            CreatedAt = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        // Expired – Lisa Anderson was in mr106
        var lease9Expired = new Lease
        {
            UnitId = mr106.Id,
            TenantId = tenants[7].Id,
            StartDate = new DateOnly(2023, 1, 1),
            EndDate = new DateOnly(2023, 12, 31),
            MonthlyRentAmount = 1550.00m,
            DepositAmount = 1550.00m,
            DepositStatus = DepositStatus.Returned,
            Status = LeaseStatus.Expired,
            CreatedAt = new DateTime(2022, 12, 10, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        // Terminated – James Taylor was in cpA3
        var lease10Terminated = new Lease
        {
            UnitId = cpA3.Id,
            TenantId = tenants[8].Id,
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            MonthlyRentAmount = 1200.00m,
            DepositAmount = 1200.00m,
            DepositStatus = DepositStatus.Forfeited,
            Status = LeaseStatus.Terminated,
            TerminationDate = new DateOnly(2024, 6, 15),
            TerminationReason = "Repeated lease violations including unauthorized pets and noise complaints",
            CreatedAt = new DateTime(2023, 12, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        // Pending – Maria Garcia for lv1C (future start)
        var lease11Pending = new Lease
        {
            UnitId = lv1C.Id,
            TenantId = tenants[9].Id,
            StartDate = new DateOnly(2025, 2, 1),
            EndDate = new DateOnly(2026, 1, 31),
            MonthlyRentAmount = 2200.00m,
            DepositAmount = 2200.00m,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Pending,
            CreatedAt = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc)
        };

        // Active – Lisa Anderson now in lv1B
        var lease12Active = new Lease
        {
            UnitId = lv1B.Id,
            TenantId = tenants[7].Id,
            StartDate = new DateOnly(2024, 3, 1),
            EndDate = new DateOnly(2025, 2, 28),
            MonthlyRentAmount = 1100.00m,
            DepositAmount = 1100.00m,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            CreatedAt = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        // Add leases in order so the expired one gets its ID first
        context.Leases.Add(lease1Expired);
        await context.SaveChangesAsync();

        // Set renewal reference
        lease2Active.RenewalOfLeaseId = lease1Expired.Id;

        context.Leases.AddRange(
            lease2Active, lease3Active, lease4Active, lease5Active,
            lease6Active, lease7Active, lease8Active,
            lease9Expired, lease10Terminated, lease11Pending, lease12Active);
        await context.SaveChangesAsync();

        // ── Payments ────────────────────────────────────────────────────
        var payments = new List<Payment>();

        // Deposit payments for active leases
        payments.Add(CreatePayment(lease2Active.Id, 950.00m, PaymentType.Deposit, new DateOnly(2024, 3, 1), new DateOnly(2024, 2, 25)));
        payments.Add(CreatePayment(lease3Active.Id, 900.00m, PaymentType.Deposit, new DateOnly(2024, 1, 1), new DateOnly(2023, 12, 28)));
        payments.Add(CreatePayment(lease4Active.Id, 1400.00m, PaymentType.Deposit, new DateOnly(2024, 6, 1), new DateOnly(2024, 5, 28)));
        payments.Add(CreatePayment(lease6Active.Id, 1400.00m, PaymentType.Deposit, new DateOnly(2024, 2, 1), new DateOnly(2024, 1, 28)));
        payments.Add(CreatePayment(lease8Active.Id, 1500.00m, PaymentType.Deposit, new DateOnly(2024, 7, 1), new DateOnly(2024, 6, 28)));

        // Rent payments for lease2Active (John Smith) – several months
        payments.Add(CreatePayment(lease2Active.Id, 950.00m, PaymentType.Rent, new DateOnly(2024, 4, 1), new DateOnly(2024, 3, 29)));
        payments.Add(CreatePayment(lease2Active.Id, 950.00m, PaymentType.Rent, new DateOnly(2024, 5, 1), new DateOnly(2024, 4, 30)));
        payments.Add(CreatePayment(lease2Active.Id, 950.00m, PaymentType.Rent, new DateOnly(2024, 6, 1), new DateOnly(2024, 5, 31)));
        payments.Add(CreatePayment(lease2Active.Id, 950.00m, PaymentType.Rent, new DateOnly(2024, 7, 1), new DateOnly(2024, 7, 1)));

        // Rent payments for lease3Active (Sarah Johnson) – with a late payment
        payments.Add(CreatePayment(lease3Active.Id, 900.00m, PaymentType.Rent, new DateOnly(2024, 2, 1), new DateOnly(2024, 1, 30)));
        payments.Add(CreatePayment(lease3Active.Id, 900.00m, PaymentType.Rent, new DateOnly(2024, 3, 1), new DateOnly(2024, 3, 1)));
        // Late payment: paid 8 days after due date
        payments.Add(CreatePayment(lease3Active.Id, 900.00m, PaymentType.Rent, new DateOnly(2024, 4, 1), new DateOnly(2024, 4, 9)));
        payments.Add(CreatePayment(lease3Active.Id, 50.00m, PaymentType.LateFee, new DateOnly(2024, 4, 1), new DateOnly(2024, 4, 9)));

        // Rent payments for lease4Active (Michael Williams)
        payments.Add(CreatePayment(lease4Active.Id, 1400.00m, PaymentType.Rent, new DateOnly(2024, 7, 1), new DateOnly(2024, 6, 30)));
        payments.Add(CreatePayment(lease4Active.Id, 1400.00m, PaymentType.Rent, new DateOnly(2024, 8, 1), new DateOnly(2024, 8, 1)));

        // Rent payments for lease6Active (Robert Brown) – with a late payment
        payments.Add(CreatePayment(lease6Active.Id, 1400.00m, PaymentType.Rent, new DateOnly(2024, 3, 1), new DateOnly(2024, 2, 28)));
        payments.Add(CreatePayment(lease6Active.Id, 1400.00m, PaymentType.Rent, new DateOnly(2024, 4, 1), new DateOnly(2024, 4, 1)));
        // Late payment: paid 7 days after due date
        payments.Add(CreatePayment(lease6Active.Id, 1400.00m, PaymentType.Rent, new DateOnly(2024, 5, 1), new DateOnly(2024, 5, 8)));
        payments.Add(CreatePayment(lease6Active.Id, 75.00m, PaymentType.LateFee, new DateOnly(2024, 5, 1), new DateOnly(2024, 5, 8)));

        // Rent payments for lease8Active (David Martinez)
        payments.Add(CreatePayment(lease8Active.Id, 1500.00m, PaymentType.Rent, new DateOnly(2024, 8, 1), new DateOnly(2024, 7, 30)));

        // Rent payments for lease12Active (Lisa Anderson)
        payments.Add(CreatePayment(lease12Active.Id, 1100.00m, PaymentType.Rent, new DateOnly(2024, 4, 1), new DateOnly(2024, 3, 30)));
        payments.Add(CreatePayment(lease12Active.Id, 1100.00m, PaymentType.Rent, new DateOnly(2024, 5, 1), new DateOnly(2024, 5, 1)));

        context.Payments.AddRange(payments);
        await context.SaveChangesAsync();

        // ── Maintenance Requests ────────────────────────────────────────
        var maintenanceRequests = new[]
        {
            // Submitted
            new MaintenanceRequest
            {
                UnitId = mr102.Id,
                TenantId = tenants[1].Id,
                Title = "Leaking kitchen faucet",
                Description = "The kitchen faucet has been dripping constantly for the past two days. Water is pooling under the sink.",
                Category = MaintenanceCategory.Plumbing,
                Priority = MaintenancePriority.Medium,
                Status = MaintenanceStatus.Submitted,
                SubmittedDate = new DateTime(2024, 8, 10, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2024, 8, 10, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 8, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new MaintenanceRequest
            {
                UnitId = lv1A.Id,
                TenantId = tenants[6].Id,
                Title = "Broken window lock",
                Description = "The lock on the bedroom window is broken and the window cannot be secured properly.",
                Category = MaintenanceCategory.Structural,
                Priority = MaintenancePriority.High,
                Status = MaintenanceStatus.Submitted,
                SubmittedDate = new DateTime(2024, 8, 12, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2024, 8, 12, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 8, 12, 0, 0, 0, DateTimeKind.Utc)
            },
            // Assigned
            new MaintenanceRequest
            {
                UnitId = mr104.Id,
                TenantId = tenants[3].Id,
                Title = "AC not cooling",
                Description = "The air conditioning unit is running but not producing cold air. Temperature stays above 80°F.",
                Category = MaintenanceCategory.HVAC,
                Priority = MaintenancePriority.High,
                Status = MaintenanceStatus.Assigned,
                AssignedTo = "Mike's HVAC Services",
                SubmittedDate = new DateTime(2024, 7, 28, 0, 0, 0, DateTimeKind.Utc),
                AssignedDate = new DateTime(2024, 7, 30, 0, 0, 0, DateTimeKind.Utc),
                EstimatedCost = 350.00m,
                CreatedAt = new DateTime(2024, 7, 28, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 7, 30, 0, 0, 0, DateTimeKind.Utc)
            },
            // Assigned – Emergency (must have AssignedTo)
            new MaintenanceRequest
            {
                UnitId = cpA1.Id,
                TenantId = tenants[4].Id,
                Title = "Gas smell in kitchen",
                Description = "Strong gas smell detected near the stove. Turned off gas valve as precaution. Need immediate inspection.",
                Category = MaintenanceCategory.Appliance,
                Priority = MaintenancePriority.Emergency,
                Status = MaintenanceStatus.Assigned,
                AssignedTo = "Springfield Gas & Electric Emergency",
                SubmittedDate = new DateTime(2024, 8, 14, 0, 0, 0, DateTimeKind.Utc),
                AssignedDate = new DateTime(2024, 8, 14, 0, 0, 0, DateTimeKind.Utc),
                EstimatedCost = 200.00m,
                CreatedAt = new DateTime(2024, 8, 14, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 8, 14, 0, 0, 0, DateTimeKind.Utc)
            },
            // InProgress
            new MaintenanceRequest
            {
                UnitId = cpA2.Id,
                TenantId = tenants[5].Id,
                Title = "Dishwasher not draining",
                Description = "The dishwasher completes its cycle but water remains standing at the bottom. Checked the filter, it's clean.",
                Category = MaintenanceCategory.Appliance,
                Priority = MaintenancePriority.Medium,
                Status = MaintenanceStatus.InProgress,
                AssignedTo = "Bob's Appliance Repair",
                SubmittedDate = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                AssignedDate = new DateTime(2024, 8, 3, 0, 0, 0, DateTimeKind.Utc),
                EstimatedCost = 175.00m,
                CreatedAt = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 8, 5, 0, 0, 0, DateTimeKind.Utc)
            },
            // Completed
            new MaintenanceRequest
            {
                UnitId = mr101.Id,
                TenantId = tenants[0].Id,
                Title = "Bathroom light fixture flickering",
                Description = "The main bathroom ceiling light flickers intermittently and sometimes goes out completely.",
                Category = MaintenanceCategory.Electrical,
                Priority = MaintenancePriority.Low,
                Status = MaintenanceStatus.Completed,
                AssignedTo = "City Electric Co.",
                SubmittedDate = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                AssignedDate = new DateTime(2024, 6, 17, 0, 0, 0, DateTimeKind.Utc),
                CompletedDate = new DateTime(2024, 6, 20, 0, 0, 0, DateTimeKind.Utc),
                CompletionNotes = "Replaced faulty ballast and light fixture. Tested and working properly.",
                EstimatedCost = 120.00m,
                ActualCost = 95.00m,
                CreatedAt = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 6, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new MaintenanceRequest
            {
                UnitId = mr103.Id,
                TenantId = tenants[2].Id,
                Title = "Clogged bathroom drain",
                Description = "The bathtub drain is very slow to empty. Tried using a plunger but it didn't help.",
                Category = MaintenanceCategory.Plumbing,
                Priority = MaintenancePriority.Medium,
                Status = MaintenanceStatus.Completed,
                AssignedTo = "Quick Plumbing LLC",
                SubmittedDate = new DateTime(2024, 7, 5, 0, 0, 0, DateTimeKind.Utc),
                AssignedDate = new DateTime(2024, 7, 6, 0, 0, 0, DateTimeKind.Utc),
                CompletedDate = new DateTime(2024, 7, 8, 0, 0, 0, DateTimeKind.Utc),
                CompletionNotes = "Snaked the drain and removed hair blockage. Drain flowing normally.",
                EstimatedCost = 150.00m,
                ActualCost = 125.00m,
                CreatedAt = new DateTime(2024, 7, 5, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 7, 8, 0, 0, 0, DateTimeKind.Utc)
            },
            // Cancelled
            new MaintenanceRequest
            {
                UnitId = lv1B.Id,
                TenantId = tenants[7].Id,
                Title = "Squeaky front door hinge",
                Description = "The front door hinge squeaks loudly when opening and closing. Very annoying.",
                Category = MaintenanceCategory.General,
                Priority = MaintenancePriority.Low,
                Status = MaintenanceStatus.Cancelled,
                SubmittedDate = new DateTime(2024, 7, 20, 0, 0, 0, DateTimeKind.Utc),
                CompletionNotes = "Tenant resolved issue themselves with WD-40.",
                CreatedAt = new DateTime(2024, 7, 20, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 7, 22, 0, 0, 0, DateTimeKind.Utc)
            },
        };

        context.MaintenanceRequests.AddRange(maintenanceRequests);
        await context.SaveChangesAsync();

        // ── Inspections ─────────────────────────────────────────────────
        var inspections = new[]
        {
            // MoveIn – completed, linked to lease
            new Inspection
            {
                UnitId = mr101.Id,
                LeaseId = lease2Active.Id,
                InspectionType = InspectionType.MoveIn,
                ScheduledDate = new DateOnly(2024, 3, 1),
                CompletedDate = new DateOnly(2024, 3, 1),
                InspectorName = "Patricia Moore",
                OverallCondition = OverallCondition.Good,
                Notes = "Unit in good condition. Minor scuff marks on living room wall. All appliances functioning. Smoke detectors tested and working.",
                FollowUpRequired = false,
                CreatedAt = new DateTime(2024, 2, 25, 0, 0, 0, DateTimeKind.Utc)
            },
            // MoveIn – completed, linked to lease
            new Inspection
            {
                UnitId = cpA2.Id,
                LeaseId = lease7Active.Id,
                InspectionType = InspectionType.MoveIn,
                ScheduledDate = new DateOnly(2024, 5, 1),
                CompletedDate = new DateOnly(2024, 5, 1),
                InspectorName = "Patricia Moore",
                OverallCondition = OverallCondition.Excellent,
                Notes = "Freshly painted unit. All fixtures new or in excellent condition. Garage door opener working. Fireplace inspected and safe.",
                FollowUpRequired = false,
                CreatedAt = new DateTime(2024, 4, 25, 0, 0, 0, DateTimeKind.Utc)
            },
            // MoveOut – completed, linked to expired lease
            new Inspection
            {
                UnitId = mr106.Id,
                LeaseId = lease9Expired.Id,
                InspectionType = InspectionType.MoveOut,
                ScheduledDate = new DateOnly(2024, 1, 2),
                CompletedDate = new DateOnly(2024, 1, 2),
                InspectorName = "Thomas Reed",
                OverallCondition = OverallCondition.Fair,
                Notes = "Some wear on carpet in high traffic areas. Kitchen countertop has a small chip. Walls need repainting. Cleaning required before next tenant.",
                FollowUpRequired = true,
                CreatedAt = new DateTime(2023, 12, 28, 0, 0, 0, DateTimeKind.Utc)
            },
            // Routine – pending (scheduled future)
            new Inspection
            {
                UnitId = cpA1.Id,
                InspectionType = InspectionType.Routine,
                ScheduledDate = new DateOnly(2025, 3, 15),
                InspectorName = "Patricia Moore",
                FollowUpRequired = false,
                CreatedAt = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            // Routine – completed
            new Inspection
            {
                UnitId = lv1A.Id,
                InspectionType = InspectionType.Routine,
                ScheduledDate = new DateOnly(2024, 10, 1),
                CompletedDate = new DateOnly(2024, 10, 1),
                InspectorName = "Thomas Reed",
                OverallCondition = OverallCondition.Good,
                Notes = "Unit well maintained by tenant. HVAC filter needs replacement. Minor caulking needed around bathroom tub. Balcony railing secure.",
                FollowUpRequired = false,
                CreatedAt = new DateTime(2024, 9, 20, 0, 0, 0, DateTimeKind.Utc)
            },
        };

        context.Inspections.AddRange(inspections);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    private static Unit CreateUnit(int propertyId, string unitNumber, int floor, int bedrooms,
        decimal bathrooms, int sqFt, decimal monthlyRent, string? amenities)
    {
        return new Unit
        {
            PropertyId = propertyId,
            UnitNumber = unitNumber,
            Floor = floor,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            SquareFeet = sqFt,
            MonthlyRent = monthlyRent,
            DepositAmount = monthlyRent, // 1x rent
            Amenities = amenities,
            CreatedAt = new DateTime(2023, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    private static Tenant CreateTenant(string firstName, string lastName, string email, string phone,
        DateOnly dob, string emergencyName, string emergencyPhone)
    {
        return new Tenant
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            DateOfBirth = dob,
            EmergencyContactName = emergencyName,
            EmergencyContactPhone = emergencyPhone,
            CreatedAt = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    private static Payment CreatePayment(int leaseId, decimal amount, PaymentType type,
        DateOnly dueDate, DateOnly paidDate)
    {
        return new Payment
        {
            LeaseId = leaseId,
            Amount = amount,
            PaymentType = type,
            PaymentMethod = PaymentMethod.BankTransfer,
            Status = PaymentStatus.Completed,
            DueDate = dueDate,
            PaymentDate = paidDate,
            CreatedAt = new DateTime(paidDate.Year, paidDate.Month, paidDate.Day, 0, 0, 0, DateTimeKind.Utc)
        };
    }
}
