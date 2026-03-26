using KeystoneProperties.Models;

namespace KeystoneProperties.Data;

public static class DataSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (context.Properties.Any())
            return;

        var now = DateTime.UtcNow;

        // Properties
        var properties = new List<Property>
        {
            new()
            {
                Id = 1, Name = "Maple Ridge Apartments", Address = "1200 Maple Ridge Dr",
                City = "Portland", State = "OR", ZipCode = "97201",
                PropertyType = PropertyType.Apartment, YearBuilt = 2005, TotalUnits = 12,
                Description = "Modern apartment complex with mountain views and updated amenities.",
                IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 2, Name = "Cedar Park Townhomes", Address = "450 Cedar Park Ln",
                City = "Portland", State = "OR", ZipCode = "97210",
                PropertyType = PropertyType.Townhouse, YearBuilt = 2012, TotalUnits = 8,
                Description = "Family-friendly townhome community with private garages.",
                IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 3, Name = "Lakeview Condos", Address = "800 Lakeview Blvd",
                City = "Lake Oswego", State = "OR", ZipCode = "97034",
                PropertyType = PropertyType.Condo, YearBuilt = 2018, TotalUnits = 6,
                Description = "Luxury waterfront condominiums with premium finishes.",
                IsActive = true, CreatedAt = now, UpdatedAt = now
            }
        };

        // Units
        var units = new List<Unit>
        {
            // Maple Ridge (12 units)
            new() { Id = 1, PropertyId = 1, UnitNumber = "101", Floor = 1, Bedrooms = 1, Bathrooms = 1, SquareFeet = 650, MonthlyRent = 950, DepositAmount = 950, Status = UnitStatus.Occupied, Amenities = "Dishwasher, Patio", CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, PropertyId = 1, UnitNumber = "102", Floor = 1, Bedrooms = 2, Bathrooms = 1, SquareFeet = 850, MonthlyRent = 1200, DepositAmount = 1200, Status = UnitStatus.Occupied, Amenities = "Dishwasher, Washer/Dryer", CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, PropertyId = 1, UnitNumber = "103", Floor = 1, Bedrooms = 0, Bathrooms = 1, SquareFeet = 450, MonthlyRent = 800, DepositAmount = 800, Status = UnitStatus.Available, Amenities = "Dishwasher", CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, PropertyId = 1, UnitNumber = "201", Floor = 2, Bedrooms = 2, Bathrooms = 2, SquareFeet = 950, MonthlyRent = 1400, DepositAmount = 1400, Status = UnitStatus.Occupied, Amenities = "Washer/Dryer, Balcony, Dishwasher", CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, PropertyId = 1, UnitNumber = "202", Floor = 2, Bedrooms = 1, Bathrooms = 1, SquareFeet = 650, MonthlyRent = 1000, DepositAmount = 1000, Status = UnitStatus.Occupied, Amenities = "Balcony", CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, PropertyId = 1, UnitNumber = "203", Floor = 2, Bedrooms = 3, Bathrooms = 2, SquareFeet = 1100, MonthlyRent = 1650, DepositAmount = 1650, Status = UnitStatus.Occupied, Amenities = "Washer/Dryer, Balcony, Walk-in Closet", CreatedAt = now, UpdatedAt = now },

            // Cedar Park (8 units)
            new() { Id = 7, PropertyId = 2, UnitNumber = "A", Floor = 1, Bedrooms = 3, Bathrooms = 2.5m, SquareFeet = 1400, MonthlyRent = 1800, DepositAmount = 1800, Status = UnitStatus.Occupied, Amenities = "Garage, Patio, Washer/Dryer", CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, PropertyId = 2, UnitNumber = "B", Floor = 1, Bedrooms = 2, Bathrooms = 1.5m, SquareFeet = 1100, MonthlyRent = 1500, DepositAmount = 1500, Status = UnitStatus.Occupied, Amenities = "Garage, Patio", CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, PropertyId = 2, UnitNumber = "C", Floor = 1, Bedrooms = 3, Bathrooms = 2.5m, SquareFeet = 1450, MonthlyRent = 1850, DepositAmount = 1850, Status = UnitStatus.Available, Amenities = "Garage, Patio, Washer/Dryer", CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, PropertyId = 2, UnitNumber = "D", Floor = 1, Bedrooms = 2, Bathrooms = 1.5m, SquareFeet = 1050, MonthlyRent = 1450, DepositAmount = 1450, Status = UnitStatus.Occupied, Amenities = "Garage, Patio", CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, PropertyId = 2, UnitNumber = "E", Floor = 1, Bedrooms = 4, Bathrooms = 3, SquareFeet = 1800, MonthlyRent = 2200, DepositAmount = 2200, Status = UnitStatus.Maintenance, Amenities = "Garage, Patio, Washer/Dryer, Fireplace", CreatedAt = now, UpdatedAt = now },

            // Lakeview (6 units)
            new() { Id = 12, PropertyId = 3, UnitNumber = "1A", Floor = 1, Bedrooms = 2, Bathrooms = 2, SquareFeet = 1050, MonthlyRent = 1700, DepositAmount = 1700, Status = UnitStatus.Occupied, Amenities = "Lake View, Balcony, Parking", CreatedAt = now, UpdatedAt = now },
            new() { Id = 13, PropertyId = 3, UnitNumber = "1B", Floor = 1, Bedrooms = 1, Bathrooms = 1, SquareFeet = 750, MonthlyRent = 1300, DepositAmount = 1300, Status = UnitStatus.Occupied, Amenities = "Parking", CreatedAt = now, UpdatedAt = now },
            new() { Id = 14, PropertyId = 3, UnitNumber = "2A", Floor = 2, Bedrooms = 3, Bathrooms = 2, SquareFeet = 1300, MonthlyRent = 2100, DepositAmount = 2100, Status = UnitStatus.Occupied, Amenities = "Lake View, Balcony, Parking, Washer/Dryer", CreatedAt = now, UpdatedAt = now },
            new() { Id = 15, PropertyId = 3, UnitNumber = "2B", Floor = 2, Bedrooms = 2, Bathrooms = 2, SquareFeet = 1100, MonthlyRent = 1800, DepositAmount = 1800, Status = UnitStatus.Available, Amenities = "Lake View, Balcony, Parking", CreatedAt = now, UpdatedAt = now },
        };

        // Tenants
        var tenants = new List<Tenant>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "503-555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Michael Johnson", EmergencyContactPhone = "503-555-0102", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "David", LastName = "Chen", Email = "david.chen@email.com", Phone = "503-555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lisa Chen", EmergencyContactPhone = "503-555-0202", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@email.com", Phone = "503-555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Carlos Garcia", EmergencyContactPhone = "503-555-0302", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "James", LastName = "Wilson", Email = "james.wilson@email.com", Phone = "503-555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Emily Wilson", EmergencyContactPhone = "503-555-0402", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Emily", LastName = "Thompson", Email = "emily.thompson@email.com", Phone = "503-555-0501", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Robert Thompson", EmergencyContactPhone = "503-555-0502", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Robert", LastName = "Martinez", Email = "robert.martinez@email.com", Phone = "503-555-0601", DateOfBirth = new DateOnly(1980, 9, 25), EmergencyContactName = "Ana Martinez", EmergencyContactPhone = "503-555-0602", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, FirstName = "Jennifer", LastName = "Brown", Email = "jennifer.brown@email.com", Phone = "503-555-0701", DateOfBirth = new DateOnly(1993, 4, 18), EmergencyContactName = "Mark Brown", EmergencyContactPhone = "503-555-0702", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, FirstName = "Michael", LastName = "Davis", Email = "michael.davis@email.com", Phone = "503-555-0801", DateOfBirth = new DateOnly(1987, 12, 3), EmergencyContactName = "Susan Davis", EmergencyContactPhone = "503-555-0802", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, FirstName = "Amanda", LastName = "Lee", Email = "amanda.lee@email.com", Phone = "503-555-0901", DateOfBirth = new DateOnly(1991, 6, 27), EmergencyContactName = "Kevin Lee", EmergencyContactPhone = "503-555-0902", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, FirstName = "Christopher", LastName = "Taylor", Email = "chris.taylor@email.com", Phone = "503-555-1001", DateOfBirth = new DateOnly(1983, 8, 14), EmergencyContactName = "Patricia Taylor", EmergencyContactPhone = "503-555-1002", IsActive = true, CreatedAt = now, UpdatedAt = now },
        };

        // Leases - original lease for unit 1 (expired, then renewed)
        var leases = new List<Lease>
        {
            // Active leases
            new() { Id = 1, UnitId = 1, TenantId = 1, StartDate = new DateOnly(2025, 1, 1), EndDate = new DateOnly(2025, 12, 31), MonthlyRentAmount = 950, DepositAmount = 950, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, UnitId = 2, TenantId = 2, StartDate = new DateOnly(2025, 3, 1), EndDate = new DateOnly(2026, 2, 28), MonthlyRentAmount = 1200, DepositAmount = 1200, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, UnitId = 4, TenantId = 3, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2026, 5, 31), MonthlyRentAmount = 1400, DepositAmount = 1400, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, UnitId = 5, TenantId = 4, StartDate = new DateOnly(2025, 2, 1), EndDate = new DateOnly(2026, 1, 31), MonthlyRentAmount = 1000, DepositAmount = 1000, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, UnitId = 6, TenantId = 5, StartDate = new DateOnly(2025, 4, 1), EndDate = new DateOnly(2026, 3, 31), MonthlyRentAmount = 1600, DepositAmount = 1650, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, UnitId = 7, TenantId = 6, StartDate = new DateOnly(2025, 1, 1), EndDate = new DateOnly(2025, 12, 31), MonthlyRentAmount = 1800, DepositAmount = 1800, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, UnitId = 8, TenantId = 7, StartDate = new DateOnly(2025, 5, 1), EndDate = new DateOnly(2026, 4, 30), MonthlyRentAmount = 1500, DepositAmount = 1500, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, UnitId = 10, TenantId = 8, StartDate = new DateOnly(2025, 7, 1), EndDate = new DateOnly(2026, 6, 30), MonthlyRentAmount = 1450, DepositAmount = 1450, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, UnitId = 12, TenantId = 9, StartDate = new DateOnly(2025, 3, 1), EndDate = new DateOnly(2026, 2, 28), MonthlyRentAmount = 1700, DepositAmount = 1700, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, UnitId = 14, TenantId = 10, StartDate = new DateOnly(2025, 8, 1), EndDate = new DateOnly(2026, 7, 31), MonthlyRentAmount = 2100, DepositAmount = 2100, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, CreatedAt = now, UpdatedAt = now },
            // Expired lease (original before renewal) for unit 13
            new() { Id = 11, UnitId = 13, TenantId = 9, StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 12, 31), MonthlyRentAmount = 1250, DepositAmount = 1300, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Renewed, CreatedAt = now, UpdatedAt = now },
            // Renewed lease for unit 13
            new() { Id = 12, UnitId = 13, TenantId = 9, StartDate = new DateOnly(2025, 1, 1), EndDate = new DateOnly(2025, 12, 31), MonthlyRentAmount = 1300, DepositAmount = 1300, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active, RenewalOfLeaseId = 11, CreatedAt = now, UpdatedAt = now },
            // Terminated lease
            new() { Id = 13, UnitId = 15, TenantId = 10, StartDate = new DateOnly(2024, 6, 1), EndDate = new DateOnly(2025, 5, 31), MonthlyRentAmount = 1750, DepositAmount = 1800, DepositStatus = DepositStatus.Returned, Status = LeaseStatus.Terminated, TerminationDate = new DateOnly(2025, 2, 15), TerminationReason = "Tenant relocated for work", CreatedAt = now, UpdatedAt = now },
            // Pending lease
            new() { Id = 14, UnitId = 3, TenantId = 5, StartDate = new DateOnly(2026, 4, 1), EndDate = new DateOnly(2027, 3, 31), MonthlyRentAmount = 850, DepositAmount = 800, DepositStatus = DepositStatus.Held, Status = LeaseStatus.Pending, CreatedAt = now, UpdatedAt = now },
        };

        // Payments
        var payments = new List<Payment>
        {
            // Deposit payments
            new() { Id = 1, LeaseId = 1, Amount = 950, PaymentDate = new DateOnly(2024, 12, 20), DueDate = new DateOnly(2024, 12, 31), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Deposit, Status = PaymentStatus.Completed, ReferenceNumber = "DEP-001", CreatedAt = now },
            new() { Id = 2, LeaseId = 2, Amount = 1200, PaymentDate = new DateOnly(2025, 2, 15), DueDate = new DateOnly(2025, 2, 28), PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Deposit, Status = PaymentStatus.Completed, ReferenceNumber = "DEP-002", CreatedAt = now },

            // Rent payments for lease 1 (Jan-Mar 2025)
            new() { Id = 3, LeaseId = 1, Amount = 950, PaymentDate = new DateOnly(2025, 1, 1), DueDate = new DateOnly(2025, 1, 1), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-001", CreatedAt = now },
            new() { Id = 4, LeaseId = 1, Amount = 950, PaymentDate = new DateOnly(2025, 2, 1), DueDate = new DateOnly(2025, 2, 1), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-002", CreatedAt = now },
            new() { Id = 5, LeaseId = 1, Amount = 950, PaymentDate = new DateOnly(2025, 3, 8), DueDate = new DateOnly(2025, 3, 1), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, Notes = "Paid late", ReferenceNumber = "RENT-003", CreatedAt = now },
            // Late fee for late March rent
            new() { Id = 6, LeaseId = 1, Amount = 60, PaymentDate = new DateOnly(2025, 3, 8), DueDate = new DateOnly(2025, 3, 1), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.LateFee, Status = PaymentStatus.Completed, Notes = "Late fee: 7 days late ($50 + $5*2)", ReferenceNumber = "LF-001", CreatedAt = now },

            // Rent payments for lease 2
            new() { Id = 7, LeaseId = 2, Amount = 1200, PaymentDate = new DateOnly(2025, 3, 1), DueDate = new DateOnly(2025, 3, 1), PaymentMethod = PaymentMethod.CreditCard, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-004", CreatedAt = now },
            new() { Id = 8, LeaseId = 2, Amount = 1200, PaymentDate = new DateOnly(2025, 4, 3), DueDate = new DateOnly(2025, 4, 1), PaymentMethod = PaymentMethod.CreditCard, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-005", CreatedAt = now },

            // Rent payments for lease 6
            new() { Id = 9, LeaseId = 6, Amount = 1800, PaymentDate = new DateOnly(2025, 1, 1), DueDate = new DateOnly(2025, 1, 1), PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-006", CreatedAt = now },
            new() { Id = 10, LeaseId = 6, Amount = 1800, PaymentDate = new DateOnly(2025, 2, 1), DueDate = new DateOnly(2025, 2, 1), PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-007", CreatedAt = now },
            new() { Id = 11, LeaseId = 6, Amount = 1800, PaymentDate = new DateOnly(2025, 3, 12), DueDate = new DateOnly(2025, 3, 1), PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, Notes = "Paid late - 11 days", ReferenceNumber = "RENT-008", CreatedAt = now },
            new() { Id = 12, LeaseId = 6, Amount = 80, PaymentDate = new DateOnly(2025, 3, 12), DueDate = new DateOnly(2025, 3, 1), PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.LateFee, Status = PaymentStatus.Completed, Notes = "Late fee: 11 days ($50 + $5*6)", ReferenceNumber = "LF-002", CreatedAt = now },

            // Rent for lease 3
            new() { Id = 13, LeaseId = 3, Amount = 1400, PaymentDate = new DateOnly(2025, 6, 1), DueDate = new DateOnly(2025, 6, 1), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-009", CreatedAt = now },
            new() { Id = 14, LeaseId = 3, Amount = 1400, PaymentDate = new DateOnly(2025, 7, 1), DueDate = new DateOnly(2025, 7, 1), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-010", CreatedAt = now },

            // Rent for lease 9
            new() { Id = 15, LeaseId = 9, Amount = 1700, PaymentDate = new DateOnly(2025, 3, 1), DueDate = new DateOnly(2025, 3, 1), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-011", CreatedAt = now },
            new() { Id = 16, LeaseId = 9, Amount = 1700, PaymentDate = new DateOnly(2025, 4, 1), DueDate = new DateOnly(2025, 4, 1), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-012", CreatedAt = now },

            // Deposit return for terminated lease 13
            new() { Id = 17, LeaseId = 13, Amount = 1800, PaymentDate = new DateOnly(2025, 3, 1), DueDate = new DateOnly(2025, 3, 1), PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.DepositReturn, Status = PaymentStatus.Completed, Notes = "Full deposit returned", ReferenceNumber = "DR-001", CreatedAt = now },

            // More rent
            new() { Id = 18, LeaseId = 4, Amount = 1000, PaymentDate = new DateOnly(2025, 2, 1), DueDate = new DateOnly(2025, 2, 1), PaymentMethod = PaymentMethod.Cash, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-013", CreatedAt = now },
            new() { Id = 19, LeaseId = 5, Amount = 1600, PaymentDate = new DateOnly(2025, 4, 1), DueDate = new DateOnly(2025, 4, 1), PaymentMethod = PaymentMethod.MoneyOrder, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-014", CreatedAt = now },
            new() { Id = 20, LeaseId = 7, Amount = 1500, PaymentDate = new DateOnly(2025, 5, 1), DueDate = new DateOnly(2025, 5, 1), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-015", CreatedAt = now },
            new() { Id = 21, LeaseId = 10, Amount = 2100, PaymentDate = new DateOnly(2025, 8, 1), DueDate = new DateOnly(2025, 8, 1), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent, Status = PaymentStatus.Completed, ReferenceNumber = "RENT-016", CreatedAt = now },
        };

        // Maintenance Requests
        var maintenanceRequests = new List<MaintenanceRequest>
        {
            new() { Id = 1, UnitId = 1, TenantId = 1, Title = "Leaking kitchen faucet", Description = "The kitchen faucet has been dripping constantly for two days.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Completed, Category = MaintenanceCategory.Plumbing, AssignedTo = "Tom Reynolds", SubmittedDate = now.AddDays(-20), AssignedDate = now.AddDays(-19), CompletedDate = now.AddDays(-17), CompletionNotes = "Replaced washer and tightened connections.", EstimatedCost = 75, ActualCost = 60, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, UnitId = 4, TenantId = 3, Title = "AC not cooling properly", Description = "The air conditioning unit is running but not cooling the apartment below 80°F.", Priority = MaintenancePriority.High, Status = MaintenanceStatus.InProgress, Category = MaintenanceCategory.HVAC, AssignedTo = "Mike Torres", SubmittedDate = now.AddDays(-3), AssignedDate = now.AddDays(-2), EstimatedCost = 300, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, UnitId = 7, TenantId = 6, Title = "Garage door opener malfunction", Description = "The garage door opener stops halfway and reverses. Sensor alignment seems off.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Assigned, Category = MaintenanceCategory.General, AssignedTo = "Dave Parker", SubmittedDate = now.AddDays(-5), AssignedDate = now.AddDays(-4), EstimatedCost = 150, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, UnitId = 2, TenantId = 2, Title = "Dishwasher not draining", Description = "Water pools at the bottom of the dishwasher after cycle completes.", Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Submitted, Category = MaintenanceCategory.Appliance, SubmittedDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, UnitId = 11, TenantId = 8, Title = "Burst pipe in bathroom", Description = "Pipe under the bathroom sink burst. Water is flooding the bathroom floor. URGENT.", Priority = MaintenancePriority.Emergency, Status = MaintenanceStatus.InProgress, Category = MaintenanceCategory.Plumbing, AssignedTo = "Tom Reynolds", SubmittedDate = now.AddDays(-1), AssignedDate = now.AddDays(-1), EstimatedCost = 500, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, UnitId = 12, TenantId = 9, Title = "Light fixture flickering", Description = "The ceiling light fixture in the living room flickers intermittently.", Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Completed, Category = MaintenanceCategory.Electrical, AssignedTo = "Sam Green", SubmittedDate = now.AddDays(-30), AssignedDate = now.AddDays(-28), CompletedDate = now.AddDays(-25), CompletionNotes = "Replaced ballast in the fixture.", EstimatedCost = 100, ActualCost = 85, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, UnitId = 5, TenantId = 4, Title = "Ant infestation in kitchen", Description = "There are ants coming in through a crack near the kitchen window.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Cancelled, Category = MaintenanceCategory.Pest, AssignedTo = "Pest Pro LLC", SubmittedDate = now.AddDays(-15), AssignedDate = now.AddDays(-14), CompletionNotes = "Tenant resolved issue on their own.", CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, UnitId = 14, TenantId = 10, Title = "Crack in bedroom wall", Description = "A hairline crack has appeared on the bedroom wall near the window.", Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Submitted, Category = MaintenanceCategory.Structural, SubmittedDate = now.AddDays(-2), CreatedAt = now, UpdatedAt = now },
        };

        // Inspections
        var inspections = new List<Inspection>
        {
            new() { Id = 1, UnitId = 1, InspectionType = InspectionType.MoveIn, ScheduledDate = new DateOnly(2025, 1, 1), CompletedDate = new DateOnly(2025, 1, 1), InspectorName = "Laura Hayes", OverallCondition = OverallCondition.Good, Notes = "Unit in good condition. Minor scuff on hallway wall noted.", FollowUpRequired = false, LeaseId = 1, CreatedAt = now },
            new() { Id = 2, UnitId = 15, InspectionType = InspectionType.MoveOut, ScheduledDate = new DateOnly(2025, 2, 20), CompletedDate = new DateOnly(2025, 2, 20), InspectorName = "Laura Hayes", OverallCondition = OverallCondition.Good, Notes = "Unit left in good condition. Normal wear and tear only. Full deposit returned.", FollowUpRequired = false, LeaseId = 13, CreatedAt = now },
            new() { Id = 3, UnitId = 7, InspectionType = InspectionType.Routine, ScheduledDate = new DateOnly(2025, 3, 15), CompletedDate = new DateOnly(2025, 3, 15), InspectorName = "Dan Miller", OverallCondition = OverallCondition.Excellent, Notes = "Townhome well maintained by tenant. All systems operational.", FollowUpRequired = false, CreatedAt = now },
            new() { Id = 4, UnitId = 4, InspectionType = InspectionType.Routine, ScheduledDate = new DateOnly(2026, 4, 1), InspectorName = "Laura Hayes", FollowUpRequired = false, CreatedAt = now },
            new() { Id = 5, UnitId = 11, InspectionType = InspectionType.Emergency, ScheduledDate = DateOnly.FromDateTime(DateTime.Today), InspectorName = "Dan Miller", Notes = "Emergency inspection due to burst pipe. Assess water damage.", FollowUpRequired = true, CreatedAt = now },
        };

        context.Properties.AddRange(properties);
        context.SaveChanges();

        context.Units.AddRange(units);
        context.SaveChanges();

        context.Tenants.AddRange(tenants);
        context.SaveChanges();

        context.Leases.AddRange(leases);
        context.SaveChanges();

        context.Payments.AddRange(payments);
        context.SaveChanges();

        context.MaintenanceRequests.AddRange(maintenanceRequests);
        context.SaveChanges();

        context.Inspections.AddRange(inspections);
        context.SaveChanges();
    }
}
