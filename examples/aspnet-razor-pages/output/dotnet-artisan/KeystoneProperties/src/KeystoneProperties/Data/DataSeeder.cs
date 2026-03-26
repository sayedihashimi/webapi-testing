using KeystoneProperties.Models;

namespace KeystoneProperties.Data;

public static class DataSeeder
{
    public static void Seed(ApplicationDbContext db)
    {
        if (db.Properties.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Properties
        var properties = new List<Property>
        {
            new()
            {
                Id = 1, Name = "Maple Ridge Apartments", Address = "1200 Maple Ridge Dr",
                City = "Austin", State = "TX", ZipCode = "78701", PropertyType = PropertyType.Apartment,
                YearBuilt = 2015, TotalUnits = 12, Description = "Modern apartment complex with pool and fitness center",
                IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 2, Name = "Cedar Park Townhomes", Address = "450 Cedar Park Blvd",
                City = "Austin", State = "TX", ZipCode = "78613", PropertyType = PropertyType.Townhouse,
                YearBuilt = 2018, TotalUnits = 8, Description = "Spacious townhomes in quiet neighborhood",
                IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 3, Name = "Lakeview Condos", Address = "789 Lakeshore Way",
                City = "Austin", State = "TX", ZipCode = "78703", PropertyType = PropertyType.Condo,
                YearBuilt = 2020, TotalUnits = 6, Description = "Luxury condos with lake views",
                IsActive = true, CreatedAt = now, UpdatedAt = now
            }
        };
        db.Properties.AddRange(properties);
        db.SaveChanges();

        // Units - 16 total across properties
        var units = new List<Unit>
        {
            // Maple Ridge Apartments (12 units, showing 8)
            new() { Id = 1, PropertyId = 1, UnitNumber = "101", Floor = 1, Bedrooms = 1, Bathrooms = 1.0m, SquareFeet = 650, MonthlyRent = 950, DepositAmount = 950, Status = UnitStatus.Occupied, Amenities = "Parking, Laundry", CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, PropertyId = 1, UnitNumber = "102", Floor = 1, Bedrooms = 2, Bathrooms = 1.0m, SquareFeet = 850, MonthlyRent = 1200, DepositAmount = 1200, Status = UnitStatus.Occupied, Amenities = "Parking, Laundry, Balcony", CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, PropertyId = 1, UnitNumber = "201", Floor = 2, Bedrooms = 2, Bathrooms = 2.0m, SquareFeet = 950, MonthlyRent = 1400, DepositAmount = 1400, Status = UnitStatus.Occupied, Amenities = "Washer/Dryer, Balcony, Parking", CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, PropertyId = 1, UnitNumber = "202", Floor = 2, Bedrooms = 0, Bathrooms = 1.0m, SquareFeet = 500, MonthlyRent = 800, DepositAmount = 800, Status = UnitStatus.Available, Amenities = "Parking", CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, PropertyId = 1, UnitNumber = "301", Floor = 3, Bedrooms = 3, Bathrooms = 2.0m, SquareFeet = 1200, MonthlyRent = 1800, DepositAmount = 1800, Status = UnitStatus.Occupied, Amenities = "Washer/Dryer, Balcony, Parking, Storage", CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, PropertyId = 1, UnitNumber = "302", Floor = 3, Bedrooms = 1, Bathrooms = 1.0m, SquareFeet = 700, MonthlyRent = 1000, DepositAmount = 1000, Status = UnitStatus.Maintenance, Amenities = "Parking, Laundry", CreatedAt = now, UpdatedAt = now },

            // Cedar Park Townhomes (8 units, showing 5)
            new() { Id = 7, PropertyId = 2, UnitNumber = "A", Floor = 1, Bedrooms = 3, Bathrooms = 2.5m, SquareFeet = 1500, MonthlyRent = 1800, DepositAmount = 1800, Status = UnitStatus.Occupied, Amenities = "Garage, Patio, Washer/Dryer", CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, PropertyId = 2, UnitNumber = "B", Floor = 1, Bedrooms = 3, Bathrooms = 2.5m, SquareFeet = 1500, MonthlyRent = 1850, DepositAmount = 1850, Status = UnitStatus.Occupied, Amenities = "Garage, Patio, Washer/Dryer", CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, PropertyId = 2, UnitNumber = "C", Floor = 1, Bedrooms = 2, Bathrooms = 1.5m, SquareFeet = 1200, MonthlyRent = 1500, DepositAmount = 1500, Status = UnitStatus.Available, Amenities = "Garage, Patio", CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, PropertyId = 2, UnitNumber = "D", Floor = 1, Bedrooms = 4, Bathrooms = 3.0m, SquareFeet = 1800, MonthlyRent = 2200, DepositAmount = 2200, Status = UnitStatus.Occupied, Amenities = "Garage, Patio, Washer/Dryer, Fireplace", CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, PropertyId = 2, UnitNumber = "E", Floor = 1, Bedrooms = 2, Bathrooms = 1.5m, SquareFeet = 1200, MonthlyRent = 1500, DepositAmount = 1500, Status = UnitStatus.Occupied, Amenities = "Garage, Patio", CreatedAt = now, UpdatedAt = now },

            // Lakeview Condos (6 units, showing 5)
            new() { Id = 12, PropertyId = 3, UnitNumber = "1A", Floor = 1, Bedrooms = 2, Bathrooms = 2.0m, SquareFeet = 1100, MonthlyRent = 1600, DepositAmount = 1600, Status = UnitStatus.Occupied, Amenities = "Lake View, Balcony, Parking", CreatedAt = now, UpdatedAt = now },
            new() { Id = 13, PropertyId = 3, UnitNumber = "1B", Floor = 1, Bedrooms = 1, Bathrooms = 1.0m, SquareFeet = 750, MonthlyRent = 1100, DepositAmount = 1100, Status = UnitStatus.Occupied, Amenities = "Parking, Laundry", CreatedAt = now, UpdatedAt = now },
            new() { Id = 14, PropertyId = 3, UnitNumber = "2A", Floor = 2, Bedrooms = 3, Bathrooms = 2.0m, SquareFeet = 1400, MonthlyRent = 2100, DepositAmount = 2100, Status = UnitStatus.Occupied, Amenities = "Lake View, Balcony, Washer/Dryer, Parking", CreatedAt = now, UpdatedAt = now },
            new() { Id = 15, PropertyId = 3, UnitNumber = "2B", Floor = 2, Bedrooms = 2, Bathrooms = 2.0m, SquareFeet = 1050, MonthlyRent = 1550, DepositAmount = 1550, Status = UnitStatus.Available, Amenities = "Lake View, Parking", CreatedAt = now, UpdatedAt = now },
            new() { Id = 16, PropertyId = 3, UnitNumber = "3A", Floor = 3, Bedrooms = 2, Bathrooms = 2.0m, SquareFeet = 1150, MonthlyRent = 1700, DepositAmount = 1700, Status = UnitStatus.Occupied, Amenities = "Lake View, Balcony, Washer/Dryer, Parking", CreatedAt = now, UpdatedAt = now },
        };
        db.Units.AddRange(units);
        db.SaveChanges();

        // Tenants
        var tenants = new List<Tenant>
        {
            new() { Id = 1, FirstName = "James", LastName = "Wilson", Email = "james.wilson@email.com", Phone = "512-555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Sarah Wilson", EmergencyContactPhone = "512-555-0102", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@email.com", Phone = "512-555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Carlos Garcia", EmergencyContactPhone = "512-555-0202", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "David", LastName = "Chen", Email = "david.chen@email.com", Phone = "512-555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Li Chen", EmergencyContactPhone = "512-555-0302", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "Emily", LastName = "Johnson", Email = "emily.johnson@email.com", Phone = "512-555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Robert Johnson", EmergencyContactPhone = "512-555-0402", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Michael", LastName = "Brown", Email = "michael.brown@email.com", Phone = "512-555-0501", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Linda Brown", EmergencyContactPhone = "512-555-0502", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Sarah", LastName = "Davis", Email = "sarah.davis@email.com", Phone = "512-555-0601", DateOfBirth = new DateOnly(1991, 9, 25), EmergencyContactName = "Tom Davis", EmergencyContactPhone = "512-555-0602", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, FirstName = "Robert", LastName = "Martinez", Email = "robert.martinez@email.com", Phone = "512-555-0701", DateOfBirth = new DateOnly(1987, 4, 18), EmergencyContactName = "Ana Martinez", EmergencyContactPhone = "512-555-0702", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, FirstName = "Jessica", LastName = "Taylor", Email = "jessica.taylor@email.com", Phone = "512-555-0801", DateOfBirth = new DateOnly(1993, 12, 3), EmergencyContactName = "Mark Taylor", EmergencyContactPhone = "512-555-0802", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, FirstName = "Daniel", LastName = "Anderson", Email = "daniel.anderson@email.com", Phone = "512-555-0901", DateOfBirth = new DateOnly(1989, 6, 14), EmergencyContactName = "Karen Anderson", EmergencyContactPhone = "512-555-0902", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, FirstName = "Amanda", LastName = "Thomas", Email = "amanda.thomas@email.com", Phone = "512-555-1001", DateOfBirth = new DateOnly(1994, 8, 7), EmergencyContactName = "Chris Thomas", EmergencyContactPhone = "512-555-1002", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, FirstName = "Kevin", LastName = "Wright", Email = "kevin.wright@email.com", Phone = "512-555-1101", DateOfBirth = new DateOnly(1986, 2, 20), EmergencyContactName = "Nancy Wright", EmergencyContactPhone = "512-555-1102", IsActive = false, CreatedAt = now, UpdatedAt = now },
        };
        db.Tenants.AddRange(tenants);
        db.SaveChanges();

        // Leases
        var leases = new List<Lease>
        {
            // Active leases
            new() { Id = 1, UnitId = 1, TenantId = 1, StartDate = new DateOnly(2025, 1, 1), EndDate = new DateOnly(2026, 1, 1), MonthlyRentAmount = 950, DepositAmount = 950, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, UnitId = 2, TenantId = 2, StartDate = new DateOnly(2025, 3, 1), EndDate = new DateOnly(2026, 3, 1), MonthlyRentAmount = 1200, DepositAmount = 1200, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, UnitId = 3, TenantId = 3, StartDate = new DateOnly(2025, 2, 1), EndDate = new DateOnly(2026, 2, 1), MonthlyRentAmount = 1400, DepositAmount = 1400, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, UnitId = 5, TenantId = 4, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2026, 6, 1), MonthlyRentAmount = 1800, DepositAmount = 1800, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, UnitId = 7, TenantId = 5, StartDate = new DateOnly(2025, 4, 1), EndDate = new DateOnly(2026, 4, 1), MonthlyRentAmount = 1800, DepositAmount = 1800, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, UnitId = 8, TenantId = 6, StartDate = new DateOnly(2025, 5, 1), EndDate = new DateOnly(2026, 5, 1), MonthlyRentAmount = 1850, DepositAmount = 1850, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, UnitId = 10, TenantId = 7, StartDate = new DateOnly(2025, 1, 15), EndDate = new DateOnly(2026, 1, 15), MonthlyRentAmount = 2200, DepositAmount = 2200, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, UnitId = 12, TenantId = 8, StartDate = new DateOnly(2025, 7, 1), EndDate = new DateOnly(2026, 7, 1), MonthlyRentAmount = 1600, DepositAmount = 1600, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, UnitId = 14, TenantId = 9, StartDate = new DateOnly(2025, 3, 1), EndDate = new DateOnly(2026, 3, 1), MonthlyRentAmount = 2100, DepositAmount = 2100, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, UnitId = 16, TenantId = 10, StartDate = new DateOnly(2025, 8, 1), EndDate = new DateOnly(2026, 8, 1), MonthlyRentAmount = 1700, DepositAmount = 1700, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },

            // Expired lease (original of renewal chain)
            new() { Id = 11, UnitId = 11, TenantId = 9, StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2025, 1, 1), MonthlyRentAmount = 1450, DepositAmount = 1500, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Renewed, CreatedAt = now, UpdatedAt = now },
            // Renewed lease (continuation)
            new() { Id = 12, UnitId = 11, TenantId = 9, StartDate = new DateOnly(2025, 1, 2), EndDate = new DateOnly(2026, 1, 2), MonthlyRentAmount = 1500, DepositAmount = 1500, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, RenewalOfLeaseId = 11, CreatedAt = now, UpdatedAt = now },

            // Terminated lease
            new() { Id = 13, UnitId = 13, TenantId = 10, StartDate = new DateOnly(2024, 6, 1), EndDate = new DateOnly(2025, 6, 1), MonthlyRentAmount = 1050, DepositAmount = 1100, DepositStatus = DepositStatus.Returned, Status = LeaseStatus.Terminated, TerminationDate = new DateOnly(2025, 2, 28), TerminationReason = "Tenant relocated for work", CreatedAt = now, UpdatedAt = now },
            // New active lease for same unit
            new() { Id = 14, UnitId = 13, TenantId = 8, StartDate = new DateOnly(2025, 4, 1), EndDate = new DateOnly(2026, 4, 1), MonthlyRentAmount = 1100, DepositAmount = 1100, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },

            // Pending lease
            new() { Id = 15, UnitId = 4, TenantId = 11, StartDate = new DateOnly(2026, 5, 1), EndDate = new DateOnly(2027, 5, 1), MonthlyRentAmount = 825, DepositAmount = 800, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Pending, CreatedAt = now, UpdatedAt = now },
        };
        db.Leases.AddRange(leases);
        db.SaveChanges();

        // Payments
        var payments = new List<Payment>();
        int paymentId = 1;

        // Generate rent payments for active leases (past months)
        void AddRentPayments(int leaseId, decimal rent, DateOnly leaseStart, int months, bool hasLatePayment = false)
        {
            for (int i = 0; i < months; i++)
            {
                var dueDate = leaseStart.AddMonths(i);
                var paymentDate = dueDate;
                var isLate = hasLatePayment && i == months - 2;

                if (isLate)
                {
                    paymentDate = dueDate.AddDays(10); // 10 days late
                }

                payments.Add(new Payment
                {
                    Id = paymentId++, LeaseId = leaseId, Amount = rent, PaymentDate = paymentDate,
                    DueDate = dueDate, PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                    Status = PaymentStatus.Completed, CreatedAt = now
                });

                if (isLate)
                {
                    // Late fee: $50 + $5/day after 5 days grace, capped at $200
                    int daysLate = 10;
                    int additionalDays = daysLate - 5;
                    decimal lateFee = Math.Min(50m + additionalDays * 5m, 200m);
                    payments.Add(new Payment
                    {
                        Id = paymentId++, LeaseId = leaseId, Amount = lateFee, PaymentDate = paymentDate,
                        DueDate = dueDate, PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.LateFee,
                        Status = PaymentStatus.Completed, Notes = $"Late fee: {daysLate} days late", CreatedAt = now
                    });
                }
            }
        }

        // Deposit payments
        payments.Add(new Payment { Id = paymentId++, LeaseId = 1, Amount = 950, PaymentDate = new DateOnly(2024, 12, 15), DueDate = new DateOnly(2024, 12, 15), PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Deposit, Status = PaymentStatus.Completed, CreatedAt = now });
        payments.Add(new Payment { Id = paymentId++, LeaseId = 2, Amount = 1200, PaymentDate = new DateOnly(2025, 2, 15), DueDate = new DateOnly(2025, 2, 15), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Deposit, Status = PaymentStatus.Completed, CreatedAt = now });

        // Rent payments for various leases
        AddRentPayments(1, 950, new DateOnly(2025, 1, 1), 3);
        AddRentPayments(2, 1200, new DateOnly(2025, 3, 1), 2, hasLatePayment: false);
        AddRentPayments(3, 1400, new DateOnly(2025, 2, 1), 2);
        AddRentPayments(5, 1800, new DateOnly(2025, 4, 1), 2, hasLatePayment: true);
        AddRentPayments(7, 2200, new DateOnly(2025, 1, 15), 3);
        AddRentPayments(9, 2100, new DateOnly(2025, 3, 1), 2);

        // Deposit return for terminated lease
        payments.Add(new Payment { Id = paymentId++, LeaseId = 13, Amount = -1100, PaymentDate = new DateOnly(2025, 3, 15), DueDate = new DateOnly(2025, 3, 15), PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.DepositReturn, Status = PaymentStatus.Completed, Notes = "Full deposit returned", CreatedAt = now });

        db.Payments.AddRange(payments);
        db.SaveChanges();

        // Maintenance Requests
        var maintenanceRequests = new List<MaintenanceRequest>
        {
            new() { Id = 1, UnitId = 1, TenantId = 1, Title = "Leaky Kitchen Faucet", Description = "Kitchen faucet drips constantly. Water pooling under sink.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Completed, Category = MaintenanceCategory.Plumbing, AssignedTo = "Mike the Plumber", SubmittedDate = now.AddDays(-30), AssignedDate = now.AddDays(-29), CompletedDate = now.AddDays(-27), CompletionNotes = "Replaced faucet cartridge and washers", EstimatedCost = 150, ActualCost = 125, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, UnitId = 3, TenantId = 3, Title = "AC Not Cooling", Description = "Air conditioning unit is running but not producing cold air.", Priority = MaintenancePriority.High, Status = MaintenanceStatus.InProgress, Category = MaintenanceCategory.HVAC, AssignedTo = "Cool Air Services", SubmittedDate = now.AddDays(-5), AssignedDate = now.AddDays(-4), EstimatedCost = 400, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, UnitId = 5, TenantId = 4, Title = "Dishwasher Not Draining", Description = "Dishwasher leaves standing water after cycle completes.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Assigned, Category = MaintenanceCategory.Appliance, AssignedTo = "Appliance Repair Co.", SubmittedDate = now.AddDays(-3), AssignedDate = now.AddDays(-2), EstimatedCost = 200, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, UnitId = 7, TenantId = 5, Title = "Electrical Outlet Not Working", Description = "Two outlets in living room have no power. Breaker not tripped.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Submitted, Category = MaintenanceCategory.Electrical, SubmittedDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, UnitId = 6, TenantId = 1, Title = "Burst Pipe Emergency", Description = "Water pipe burst in bathroom wall. Water spraying everywhere!", Priority = MaintenancePriority.Emergency, Status = MaintenanceStatus.InProgress, Category = MaintenanceCategory.Plumbing, AssignedTo = "Emergency Plumbing 24/7", SubmittedDate = now.AddDays(-1), AssignedDate = now.AddDays(-1), EstimatedCost = 800, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, UnitId = 12, TenantId = 8, Title = "Pest Control - Ants", Description = "Small ants appearing in kitchen near sink area.", Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Completed, Category = MaintenanceCategory.Pest, AssignedTo = "Pest Pro Services", SubmittedDate = now.AddDays(-15), AssignedDate = now.AddDays(-14), CompletedDate = now.AddDays(-12), CompletionNotes = "Applied gel bait treatment. Follow-up in 2 weeks.", EstimatedCost = 100, ActualCost = 85, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, UnitId = 10, TenantId = 7, Title = "Garage Door Stuck", Description = "Garage door opener makes noise but door won't open fully.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Cancelled, Category = MaintenanceCategory.General, SubmittedDate = now.AddDays(-20), CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, UnitId = 14, TenantId = 9, Title = "Ceiling Crack in Bedroom", Description = "A crack has appeared in the bedroom ceiling, about 3 feet long.", Priority = MaintenancePriority.High, Status = MaintenanceStatus.Submitted, Category = MaintenanceCategory.Structural, SubmittedDate = now.AddDays(-2), CreatedAt = now, UpdatedAt = now },
        };
        db.MaintenanceRequests.AddRange(maintenanceRequests);
        db.SaveChanges();

        // Inspections
        var inspections = new List<Inspection>
        {
            new() { Id = 1, UnitId = 1, InspectionType = InspectionType.MoveIn, ScheduledDate = new DateOnly(2025, 1, 1), CompletedDate = new DateOnly(2025, 1, 1), InspectorName = "John Hartley", OverallCondition = OverallCondition.Good, Notes = "Unit in good condition. Minor scuff on living room wall noted. All appliances functional.", FollowUpRequired = false, LeaseId = 1, CreatedAt = now },
            new() { Id = 2, UnitId = 3, InspectionType = InspectionType.MoveIn, ScheduledDate = new DateOnly(2025, 2, 1), CompletedDate = new DateOnly(2025, 2, 1), InspectorName = "John Hartley", OverallCondition = OverallCondition.Excellent, Notes = "Unit in excellent condition. Fresh paint, new carpet. All fixtures working.", FollowUpRequired = false, LeaseId = 3, CreatedAt = now },
            new() { Id = 3, UnitId = 13, InspectionType = InspectionType.MoveOut, ScheduledDate = new DateOnly(2025, 3, 1), CompletedDate = new DateOnly(2025, 3, 1), InspectorName = "Sandra Lopez", OverallCondition = OverallCondition.Fair, Notes = "Some wall damage in hallway. Carpet staining in bedroom. Kitchen in acceptable condition.", FollowUpRequired = true, LeaseId = 13, CreatedAt = now },
            new() { Id = 4, UnitId = 7, InspectionType = InspectionType.Routine, ScheduledDate = new DateOnly(2025, 6, 15), CompletedDate = new DateOnly(2025, 6, 15), InspectorName = "John Hartley", OverallCondition = OverallCondition.Good, Notes = "Regular 6-month inspection. Unit well-maintained by tenant. Smoke detectors tested and working.", FollowUpRequired = false, LeaseId = 5, CreatedAt = now },
            new() { Id = 5, UnitId = 14, InspectionType = InspectionType.Routine, ScheduledDate = new DateOnly(2026, 4, 15), InspectorName = "Sandra Lopez", Notes = "Scheduled routine inspection", FollowUpRequired = false, LeaseId = 9, CreatedAt = now },
            new() { Id = 6, UnitId = 6, InspectionType = InspectionType.Emergency, ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow), InspectorName = "John Hartley", Notes = "Emergency inspection due to burst pipe report", FollowUpRequired = false, CreatedAt = now },
        };
        db.Inspections.AddRange(inspections);
        db.SaveChanges();
    }
}
