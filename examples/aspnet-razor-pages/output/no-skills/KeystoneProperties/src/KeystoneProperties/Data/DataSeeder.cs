using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Properties.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Properties
        var properties = new List<Property>
        {
            new() { Name = "Maple Ridge Apartments", Address = "1200 Maple Avenue", City = "Springfield", State = "IL", ZipCode = "62701", PropertyType = PropertyType.Apartment, YearBuilt = 2005, TotalUnits = 12, Description = "Modern apartment complex with on-site laundry and parking.", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Cedar Park Townhomes", Address = "450 Cedar Lane", City = "Springfield", State = "IL", ZipCode = "62702", PropertyType = PropertyType.Townhouse, YearBuilt = 2010, TotalUnits = 8, Description = "Spacious townhomes with private garages and yards.", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Lakeview Condos", Address = "88 Lakeshore Drive", City = "Champaign", State = "IL", ZipCode = "61820", PropertyType = PropertyType.Condo, YearBuilt = 2018, TotalUnits = 6, Description = "Luxury condominiums overlooking Lake Champaign.", IsActive = true, CreatedAt = now, UpdatedAt = now },
        };
        context.Properties.AddRange(properties);
        await context.SaveChangesAsync();

        // Units - Maple Ridge (12 units)
        var mapleUnits = new List<Unit>();
        var unitConfigs = new (string num, int floor, int bed, decimal bath, int sqft, decimal rent, decimal dep, UnitStatus status, string? amenities)[]
        {
            ("101", 1, 1, 1.0m, 650, 950.00m, 950.00m, UnitStatus.Occupied, "Parking"),
            ("102", 1, 2, 1.0m, 850, 1200.00m, 1200.00m, UnitStatus.Occupied, "Parking, Balcony"),
            ("103", 1, 0, 1.0m, 450, 800.00m, 800.00m, UnitStatus.Available, "Parking"),
            ("104", 1, 2, 1.5m, 900, 1300.00m, 1300.00m, UnitStatus.Occupied, "Parking, Washer/Dryer"),
            ("201", 2, 1, 1.0m, 650, 975.00m, 975.00m, UnitStatus.Occupied, "Parking, Balcony"),
            ("202", 2, 2, 1.0m, 850, 1250.00m, 1250.00m, UnitStatus.Occupied, "Parking, Balcony"),
            ("203", 2, 3, 2.0m, 1100, 1600.00m, 1600.00m, UnitStatus.Occupied, "Parking, Balcony, Washer/Dryer"),
            ("204", 2, 1, 1.0m, 650, 975.00m, 975.00m, UnitStatus.Available, null),
            ("301", 3, 2, 2.0m, 950, 1400.00m, 1400.00m, UnitStatus.Occupied, "Parking, Balcony, Washer/Dryer"),
            ("302", 3, 3, 2.0m, 1200, 1700.00m, 1700.00m, UnitStatus.Maintenance, "Parking, Balcony, Washer/Dryer, Fireplace"),
            ("303", 3, 1, 1.0m, 650, 1000.00m, 1000.00m, UnitStatus.Occupied, "Parking, Balcony"),
            ("304", 3, 2, 1.5m, 900, 1350.00m, 1350.00m, UnitStatus.Available, "Parking, Balcony"),
        };
        foreach (var uc in unitConfigs)
        {
            mapleUnits.Add(new Unit { PropertyId = properties[0].Id, UnitNumber = uc.num, Floor = uc.floor, Bedrooms = uc.bed, Bathrooms = uc.bath, SquareFeet = uc.sqft, MonthlyRent = uc.rent, DepositAmount = uc.dep, Status = uc.status, Amenities = uc.amenities, CreatedAt = now, UpdatedAt = now });
        }

        // Cedar Park Townhomes (8 units) - use letters
        var cedarConfigs = new (string num, int bed, decimal bath, int sqft, decimal rent, decimal dep, UnitStatus status, string? amenities)[]
        {
            ("A", 3, 2.5m, 1400, 1800.00m, 1800.00m, UnitStatus.Occupied, "Garage, Yard, Washer/Dryer"),
            ("B", 3, 2.5m, 1400, 1800.00m, 1800.00m, UnitStatus.Occupied, "Garage, Yard, Washer/Dryer"),
            ("C", 2, 1.5m, 1100, 1500.00m, 1500.00m, UnitStatus.Available, "Garage, Yard"),
        };
        var cedarUnits = new List<Unit>();
        foreach (var cc in cedarConfigs)
        {
            cedarUnits.Add(new Unit { PropertyId = properties[1].Id, UnitNumber = cc.num, Bedrooms = cc.bed, Bathrooms = cc.bath, SquareFeet = cc.sqft, MonthlyRent = cc.rent, DepositAmount = cc.dep, Status = cc.status, Amenities = cc.amenities, CreatedAt = now, UpdatedAt = now });
        }

        // Lakeview Condos (6 units)
        var lakeConfigs = new (string num, int floor, int bed, decimal bath, int sqft, decimal rent, decimal dep, UnitStatus status, string? amenities)[]
        {
            ("1A", 1, 2, 2.0m, 1050, 1600.00m, 1600.00m, UnitStatus.Occupied, "Washer/Dryer, Lake View, Gym Access"),
            ("1B", 1, 2, 2.0m, 1050, 1600.00m, 1600.00m, UnitStatus.Occupied, "Washer/Dryer, Lake View, Gym Access"),
        };
        var lakeUnits = new List<Unit>();
        foreach (var lc in lakeConfigs)
        {
            lakeUnits.Add(new Unit { PropertyId = properties[2].Id, UnitNumber = lc.num, Floor = lc.floor, Bedrooms = lc.bed, Bathrooms = lc.bath, SquareFeet = lc.sqft, MonthlyRent = lc.rent, DepositAmount = lc.dep, Status = lc.status, Amenities = lc.amenities, CreatedAt = now, UpdatedAt = now });
        }

        var allUnits = mapleUnits.Concat(cedarUnits).Concat(lakeUnits).ToList();
        context.Units.AddRange(allUnits);
        await context.SaveChangesAsync();

        // Tenants
        var tenants = new List<Tenant>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "217-555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "217-555-0102", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Carlos", LastName = "Martinez", Email = "carlos.martinez@email.com", Phone = "217-555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Maria Martinez", EmergencyContactPhone = "217-555-0202", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Diana", LastName = "Patel", Email = "diana.patel@email.com", Phone = "217-555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Raj Patel", EmergencyContactPhone = "217-555-0302", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Ethan", LastName = "Williams", Email = "ethan.williams@email.com", Phone = "217-555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "217-555-0402", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Fatima", LastName = "Al-Hassan", Email = "fatima.alhassan@email.com", Phone = "217-555-0501", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Omar Al-Hassan", EmergencyContactPhone = "217-555-0502", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "George", LastName = "Chen", Email = "george.chen@email.com", Phone = "217-555-0601", DateOfBirth = new DateOnly(1991, 9, 3), EmergencyContactName = "Lin Chen", EmergencyContactPhone = "217-555-0602", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Hannah", LastName = "Brown", Email = "hannah.brown@email.com", Phone = "217-555-0701", DateOfBirth = new DateOnly(1987, 12, 19), EmergencyContactName = "Tom Brown", EmergencyContactPhone = "217-555-0702", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Ivan", LastName = "Kozlov", Email = "ivan.kozlov@email.com", Phone = "217-555-0801", DateOfBirth = new DateOnly(1993, 4, 25), EmergencyContactName = "Natalia Kozlova", EmergencyContactPhone = "217-555-0802", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Julia", LastName = "Nguyen", Email = "julia.nguyen@email.com", Phone = "217-555-0901", DateOfBirth = new DateOnly(1996, 8, 14), EmergencyContactName = "Minh Nguyen", EmergencyContactPhone = "217-555-0902", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Kevin", LastName = "O'Brien", Email = "kevin.obrien@email.com", Phone = "217-555-1001", DateOfBirth = new DateOnly(1989, 2, 7), EmergencyContactName = "Siobhan O'Brien", EmergencyContactPhone = "217-555-1002", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Laura", LastName = "Schmidt", Email = "laura.schmidt@email.com", Phone = "217-555-1101", DateOfBirth = new DateOnly(1994, 6, 28), EmergencyContactName = "Hans Schmidt", EmergencyContactPhone = "217-555-1102", IsActive = false, CreatedAt = now, UpdatedAt = now },
        };
        context.Tenants.AddRange(tenants);
        await context.SaveChangesAsync();

        // Leases
        var leases = new List<Lease>
        {
            // Active leases
            new() { UnitId = mapleUnits[0].Id, TenantId = tenants[0].Id, StartDate = today.AddMonths(-6), EndDate = today.AddMonths(6), MonthlyRentAmount = 950.00m, DepositAmount = 950.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[1].Id, TenantId = tenants[1].Id, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(9), MonthlyRentAmount = 1200.00m, DepositAmount = 1200.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[3].Id, TenantId = tenants[2].Id, StartDate = today.AddMonths(-8), EndDate = today.AddMonths(4), MonthlyRentAmount = 1300.00m, DepositAmount = 1300.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[4].Id, TenantId = tenants[3].Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(10), MonthlyRentAmount = 975.00m, DepositAmount = 975.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[5].Id, TenantId = tenants[4].Id, StartDate = today.AddMonths(-5), EndDate = today.AddMonths(7), MonthlyRentAmount = 1250.00m, DepositAmount = 1250.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[6].Id, TenantId = tenants[5].Id, StartDate = today.AddMonths(-4), EndDate = today.AddDays(20), MonthlyRentAmount = 1600.00m, DepositAmount = 1600.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, Notes = "Lease expiring soon", CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[8].Id, TenantId = tenants[6].Id, StartDate = today.AddMonths(-10), EndDate = today.AddMonths(2), MonthlyRentAmount = 1400.00m, DepositAmount = 1400.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[10].Id, TenantId = tenants[7].Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(11), MonthlyRentAmount = 1000.00m, DepositAmount = 1000.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            // Cedar Park
            new() { UnitId = cedarUnits[0].Id, TenantId = tenants[8].Id, StartDate = today.AddMonths(-7), EndDate = today.AddMonths(5), MonthlyRentAmount = 1800.00m, DepositAmount = 1800.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = cedarUnits[1].Id, TenantId = tenants[9].Id, StartDate = today.AddMonths(-9), EndDate = today.AddMonths(3), MonthlyRentAmount = 1800.00m, DepositAmount = 1800.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            // Lakeview
            new() { UnitId = lakeUnits[0].Id, TenantId = tenants[0].Id, StartDate = today.AddMonths(-4), EndDate = today.AddMonths(8), MonthlyRentAmount = 1600.00m, DepositAmount = 1600.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = lakeUnits[1].Id, TenantId = tenants[5].Id, StartDate = today.AddMonths(-6), EndDate = today.AddMonths(6), MonthlyRentAmount = 1600.00m, DepositAmount = 1600.00m, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
        };
        context.Leases.AddRange(leases);
        await context.SaveChangesAsync();

        // Expired lease (original) and renewal chain
        var expiredLease = new Lease
        {
            UnitId = mapleUnits[0].Id, TenantId = tenants[0].Id,
            StartDate = today.AddMonths(-18), EndDate = today.AddMonths(-6).AddDays(-1),
            MonthlyRentAmount = 900.00m, DepositAmount = 950.00m,
            DepositStatus = DepositStatus.Held, Status = LeaseStatus.Renewed,
            CreatedAt = now, UpdatedAt = now
        };
        context.Leases.Add(expiredLease);
        await context.SaveChangesAsync();
        // Link the active lease as a renewal of the expired one
        leases[0].RenewalOfLeaseId = expiredLease.Id;
        await context.SaveChangesAsync();

        // Terminated lease
        var terminatedLease = new Lease
        {
            UnitId = mapleUnits[2].Id, TenantId = tenants[10].Id,
            StartDate = today.AddMonths(-12), EndDate = today.AddMonths(-2),
            MonthlyRentAmount = 800.00m, DepositAmount = 800.00m,
            DepositStatus = DepositStatus.Returned, Status = LeaseStatus.Terminated,
            TerminationDate = today.AddMonths(-4), TerminationReason = "Tenant relocated for work",
            CreatedAt = now, UpdatedAt = now
        };
        context.Leases.Add(terminatedLease);
        await context.SaveChangesAsync();

        // Pending lease
        var pendingLease = new Lease
        {
            UnitId = mapleUnits[7].Id, TenantId = tenants[4].Id,
            StartDate = today.AddDays(15), EndDate = today.AddMonths(12).AddDays(15),
            MonthlyRentAmount = 975.00m, DepositAmount = 975.00m,
            DepositStatus = DepositStatus.Held, Status = LeaseStatus.Pending,
            Notes = "Move-in scheduled", CreatedAt = now, UpdatedAt = now
        };
        context.Leases.Add(pendingLease);
        await context.SaveChangesAsync();

        // Payments
        var payments = new List<Payment>();
        // Generate rent payments for active leases (last few months)
        foreach (var lease in leases.Take(8))
        {
            for (int m = 0; m < 3; m++)
            {
                var dueDate = today.AddMonths(-m);
                dueDate = new DateOnly(dueDate.Year, dueDate.Month, 1);
                var payDate = dueDate.AddDays(m == 2 ? 8 : 1); // one late payment
                payments.Add(new Payment
                {
                    LeaseId = lease.Id, Amount = lease.MonthlyRentAmount,
                    PaymentDate = payDate, DueDate = dueDate,
                    PaymentMethod = m % 2 == 0 ? PaymentMethod.BankTransfer : PaymentMethod.Check,
                    PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed,
                    ReferenceNumber = $"PAY-{lease.Id:D3}-{dueDate:yyyyMM}",
                    CreatedAt = now
                });
            }
        }

        // Late fee for the late payment (lease index 0, month index 2 => 8 days late, 3 days over grace)
        var lateLeaseRef = leases[0];
        var lateDue = today.AddMonths(-2);
        lateDue = new DateOnly(lateDue.Year, lateDue.Month, 1);
        payments.Add(new Payment
        {
            LeaseId = lateLeaseRef.Id, Amount = 65.00m, // $50 + $5 * 3 days
            PaymentDate = lateDue.AddDays(8), DueDate = lateDue,
            PaymentMethod = PaymentMethod.BankTransfer,
            PaymentType = PaymentType.LateFee, Status = PaymentStatus.Completed,
            Notes = "Late fee: 3 days past grace period",
            CreatedAt = now
        });

        // Deposit payments
        payments.Add(new Payment { LeaseId = leases[0].Id, Amount = 950.00m, PaymentDate = leases[0].StartDate, DueDate = leases[0].StartDate, PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Deposit, Status = PaymentStatus.Completed, CreatedAt = now });
        payments.Add(new Payment { LeaseId = leases[1].Id, Amount = 1200.00m, PaymentDate = leases[1].StartDate, DueDate = leases[1].StartDate, PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Deposit, Status = PaymentStatus.Completed, CreatedAt = now });

        // Deposit return for terminated lease
        payments.Add(new Payment { LeaseId = terminatedLease.Id, Amount = -800.00m, PaymentDate = terminatedLease.TerminationDate!.Value.AddDays(14), DueDate = terminatedLease.TerminationDate!.Value, PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.DepositReturn, Status = PaymentStatus.Completed, Notes = "Full deposit returned", CreatedAt = now });

        context.Payments.AddRange(payments);
        await context.SaveChangesAsync();

        // Maintenance Requests
        var maintenanceRequests = new List<MaintenanceRequest>
        {
            new() { UnitId = mapleUnits[0].Id, TenantId = tenants[0].Id, Title = "Leaky kitchen faucet", Description = "The kitchen faucet has been dripping constantly for the past week.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Completed, Category = MaintenanceCategory.Plumbing, AssignedTo = "Mike the Plumber", SubmittedDate = now.AddDays(-14), AssignedDate = now.AddDays(-13), CompletedDate = now.AddDays(-11), CompletionNotes = "Replaced faucet washer and tightened connections.", EstimatedCost = 150.00m, ActualCost = 85.00m, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[1].Id, TenantId = tenants[1].Id, Title = "AC not cooling", Description = "Air conditioning unit is running but not producing cold air.", Priority = MaintenancePriority.High, Status = MaintenanceStatus.InProgress, Category = MaintenanceCategory.HVAC, AssignedTo = "Cool Air Services", SubmittedDate = now.AddDays(-3), AssignedDate = now.AddDays(-2), EstimatedCost = 500.00m, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[3].Id, TenantId = tenants[2].Id, Title = "Dishwasher not draining", Description = "Dishwasher fills with water but does not drain after cycle.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Assigned, Category = MaintenanceCategory.Appliance, AssignedTo = "Appliance Pros", SubmittedDate = now.AddDays(-5), AssignedDate = now.AddDays(-4), EstimatedCost = 200.00m, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[4].Id, TenantId = tenants[3].Id, Title = "Light fixture flickering", Description = "Bedroom ceiling light flickers intermittently.", Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Submitted, Category = MaintenanceCategory.Electrical, SubmittedDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[9].Id, TenantId = tenants[6].Id, Title = "Water heater burst", Description = "Water heater has burst and is flooding the unit. Emergency!", Priority = MaintenancePriority.Emergency, Status = MaintenanceStatus.InProgress, Category = MaintenanceCategory.Plumbing, AssignedTo = "Emergency Plumbing Co.", SubmittedDate = now.AddDays(-1), AssignedDate = now.AddDays(-1), EstimatedCost = 2000.00m, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = cedarUnits[0].Id, TenantId = tenants[8].Id, Title = "Garage door stuck", Description = "Garage door opener is not responding to remote.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Submitted, Category = MaintenanceCategory.General, SubmittedDate = now.AddDays(-2), CreatedAt = now, UpdatedAt = now },
            new() { UnitId = lakeUnits[0].Id, TenantId = tenants[0].Id, Title = "Ant infestation in kitchen", Description = "Small ants appearing near the kitchen sink and countertops.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Completed, Category = MaintenanceCategory.Pest, AssignedTo = "Bug Busters Pest Control", SubmittedDate = now.AddDays(-21), AssignedDate = now.AddDays(-20), CompletedDate = now.AddDays(-18), CompletionNotes = "Applied perimeter treatment and sealed entry points.", EstimatedCost = 250.00m, ActualCost = 175.00m, CreatedAt = now, UpdatedAt = now },
            new() { UnitId = mapleUnits[6].Id, TenantId = tenants[5].Id, Title = "Cracked bathroom tile", Description = "Several tiles in the bathroom floor are cracked and coming loose.", Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Cancelled, Category = MaintenanceCategory.Structural, SubmittedDate = now.AddDays(-30), CompletionNotes = "Tenant cancelled — moving out soon.", CreatedAt = now, UpdatedAt = now },
        };
        context.MaintenanceRequests.AddRange(maintenanceRequests);
        await context.SaveChangesAsync();

        // Inspections
        var inspections = new List<Inspection>
        {
            new() { UnitId = mapleUnits[0].Id, InspectionType = InspectionType.MoveIn, ScheduledDate = leases[0].StartDate, CompletedDate = leases[0].StartDate, InspectorName = "James Walker", OverallCondition = OverallCondition.Good, Notes = "Unit in good condition. Minor scuff marks on living room wall noted.", FollowUpRequired = false, LeaseId = leases[0].Id, CreatedAt = now },
            new() { UnitId = mapleUnits[1].Id, InspectionType = InspectionType.MoveIn, ScheduledDate = leases[1].StartDate, CompletedDate = leases[1].StartDate, InspectorName = "James Walker", OverallCondition = OverallCondition.Excellent, Notes = "Unit freshly renovated. All appliances working.", FollowUpRequired = false, LeaseId = leases[1].Id, CreatedAt = now },
            new() { UnitId = mapleUnits[2].Id, InspectionType = InspectionType.MoveOut, ScheduledDate = terminatedLease.TerminationDate!.Value, CompletedDate = terminatedLease.TerminationDate!.Value, InspectorName = "James Walker", OverallCondition = OverallCondition.Fair, Notes = "Some wear on carpet. Kitchen cleaned. Minor wall damage.", FollowUpRequired = true, LeaseId = terminatedLease.Id, CreatedAt = now },
            new() { UnitId = mapleUnits[5].Id, InspectionType = InspectionType.Routine, ScheduledDate = today.AddDays(-7), CompletedDate = today.AddDays(-7), InspectorName = "Sandra Lee", OverallCondition = OverallCondition.Good, Notes = "Routine 6-month inspection. Unit well maintained by tenant.", FollowUpRequired = false, CreatedAt = now },
            new() { UnitId = cedarUnits[0].Id, InspectionType = InspectionType.Routine, ScheduledDate = today.AddDays(14), InspectorName = "Sandra Lee", Notes = "Scheduled routine inspection.", FollowUpRequired = false, CreatedAt = now },
            new() { UnitId = lakeUnits[0].Id, InspectionType = InspectionType.Routine, ScheduledDate = today.AddDays(-30), CompletedDate = today.AddDays(-30), InspectorName = "James Walker", OverallCondition = OverallCondition.Excellent, Notes = "Unit in excellent condition.", FollowUpRequired = false, CreatedAt = now },
        };
        context.Inspections.AddRange(inspections);
        await context.SaveChangesAsync();
    }
}
