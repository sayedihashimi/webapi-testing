using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Properties.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var firstOfMonth = new DateOnly(today.Year, today.Month, 1);

        // ========== Properties ==========
        var properties = new List<Property>
        {
            new()
            {
                Id = 1, Name = "Maple Ridge Apartments", Address = "123 Maple Street",
                City = "Portland", State = "OR", ZipCode = "97201",
                PropertyType = PropertyType.Apartment, YearBuilt = 2018, TotalUnits = 12,
                Description = "Modern apartment complex with covered parking and on-site laundry facilities.",
                IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 2, Name = "Cedar Park Townhomes", Address = "456 Cedar Avenue",
                City = "Portland", State = "OR", ZipCode = "97202",
                PropertyType = PropertyType.Townhouse, YearBuilt = 2015, TotalUnits = 8,
                Description = "Spacious townhomes with private patios and attached garages.",
                IsActive = true, CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 3, Name = "Lakeview Condos", Address = "789 Lake Drive",
                City = "Lake Oswego", State = "OR", ZipCode = "97034",
                PropertyType = PropertyType.Condo, YearBuilt = 2020, TotalUnits = 6,
                Description = "Luxury condominiums with lake views and premium finishes.",
                IsActive = true, CreatedAt = now, UpdatedAt = now
            },
        };

        // ========== Units ==========
        var units = new List<Unit>
        {
            // Maple Ridge Apartments (PropertyId=1) — Floor 1: 1BR/1BA, 650 sqft
            new() { Id = 1,  PropertyId = 1, UnitNumber = "101", Floor = 1, Bedrooms = 1, Bathrooms = 1.0m, SquareFeet = 650,  MonthlyRent = 950m,  DepositAmount = 950m,  Status = UnitStatus.Occupied,    Amenities = "Dishwasher, Central AC",                                                      CreatedAt = now, UpdatedAt = now },
            new() { Id = 2,  PropertyId = 1, UnitNumber = "102", Floor = 1, Bedrooms = 1, Bathrooms = 1.0m, SquareFeet = 650,  MonthlyRent = 975m,  DepositAmount = 975m,  Status = UnitStatus.Occupied,    Amenities = "Dishwasher, Central AC",                                                      CreatedAt = now, UpdatedAt = now },
            new() { Id = 3,  PropertyId = 1, UnitNumber = "103", Floor = 1, Bedrooms = 1, Bathrooms = 1.0m, SquareFeet = 650,  MonthlyRent = 1000m, DepositAmount = 1000m, Status = UnitStatus.Available,   Amenities = "Dishwasher, Central AC",                                                      CreatedAt = now, UpdatedAt = now },
            new() { Id = 4,  PropertyId = 1, UnitNumber = "104", Floor = 1, Bedrooms = 1, Bathrooms = 1.0m, SquareFeet = 650,  MonthlyRent = 1050m, DepositAmount = 1050m, Status = UnitStatus.Occupied,    Amenities = "Dishwasher, Central AC, Balcony",                                             CreatedAt = now, UpdatedAt = now },
            // Floor 2: 2BR/1BA, 850 sqft
            new() { Id = 5,  PropertyId = 1, UnitNumber = "201", Floor = 2, Bedrooms = 2, Bathrooms = 1.0m, SquareFeet = 850,  MonthlyRent = 1200m, DepositAmount = 1200m, Status = UnitStatus.Occupied,    Amenities = "Dishwasher, Central AC, In-unit laundry",                                     CreatedAt = now, UpdatedAt = now },
            new() { Id = 6,  PropertyId = 1, UnitNumber = "202", Floor = 2, Bedrooms = 2, Bathrooms = 1.0m, SquareFeet = 850,  MonthlyRent = 1250m, DepositAmount = 1250m, Status = UnitStatus.Occupied,    Amenities = "Dishwasher, Central AC, In-unit laundry",                                     CreatedAt = now, UpdatedAt = now },
            new() { Id = 7,  PropertyId = 1, UnitNumber = "203", Floor = 2, Bedrooms = 2, Bathrooms = 1.0m, SquareFeet = 850,  MonthlyRent = 1300m, DepositAmount = 1300m, Status = UnitStatus.Available,   Amenities = "Dishwasher, Central AC, In-unit laundry",                                     CreatedAt = now, UpdatedAt = now },
            new() { Id = 8,  PropertyId = 1, UnitNumber = "204", Floor = 2, Bedrooms = 2, Bathrooms = 1.0m, SquareFeet = 850,  MonthlyRent = 1350m, DepositAmount = 1350m, Status = UnitStatus.Occupied,    Amenities = "Dishwasher, Central AC, In-unit laundry, Balcony",                            CreatedAt = now, UpdatedAt = now },
            // Floor 3: 3BR/2BA, 1100 sqft
            new() { Id = 9,  PropertyId = 1, UnitNumber = "301", Floor = 3, Bedrooms = 3, Bathrooms = 2.0m, SquareFeet = 1100, MonthlyRent = 1600m, DepositAmount = 1600m, Status = UnitStatus.Occupied,    Amenities = "Dishwasher, Central AC, In-unit laundry, Balcony, Walk-in closet",             CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, PropertyId = 1, UnitNumber = "302", Floor = 3, Bedrooms = 3, Bathrooms = 2.0m, SquareFeet = 1100, MonthlyRent = 1650m, DepositAmount = 1650m, Status = UnitStatus.Maintenance, Amenities = "Dishwasher, Central AC, In-unit laundry, Balcony",                            CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, PropertyId = 1, UnitNumber = "303", Floor = 3, Bedrooms = 3, Bathrooms = 2.0m, SquareFeet = 1100, MonthlyRent = 1750m, DepositAmount = 1750m, Status = UnitStatus.Occupied,    Amenities = "Dishwasher, Central AC, In-unit laundry, Balcony, Walk-in closet",             CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, PropertyId = 1, UnitNumber = "304", Floor = 3, Bedrooms = 3, Bathrooms = 2.0m, SquareFeet = 1100, MonthlyRent = 1800m, DepositAmount = 1800m, Status = UnitStatus.Available,   Amenities = "Dishwasher, Central AC, In-unit laundry, Balcony, Walk-in closet, Mountain view", CreatedAt = now, UpdatedAt = now },

            // Cedar Park Townhomes (PropertyId=2) — A-units: 2BR/1.5BA, 1000 sqft
            new() { Id = 13, PropertyId = 2, UnitNumber = "A1", Floor = 1, Bedrooms = 2, Bathrooms = 1.5m, SquareFeet = 1000, MonthlyRent = 1400m, DepositAmount = 1400m, Status = UnitStatus.Occupied, Amenities = "Attached garage, Private patio, Washer/Dryer hookups",                         CreatedAt = now, UpdatedAt = now },
            new() { Id = 14, PropertyId = 2, UnitNumber = "A2", Floor = 1, Bedrooms = 2, Bathrooms = 1.5m, SquareFeet = 1000, MonthlyRent = 1450m, DepositAmount = 1450m, Status = UnitStatus.Occupied, Amenities = "Attached garage, Private patio, Washer/Dryer hookups",                         CreatedAt = now, UpdatedAt = now },
            new() { Id = 15, PropertyId = 2, UnitNumber = "A3", Floor = 1, Bedrooms = 2, Bathrooms = 1.5m, SquareFeet = 1000, MonthlyRent = 1475m, DepositAmount = 1475m, Status = UnitStatus.Occupied, Amenities = "Attached garage, Private patio, Washer/Dryer hookups",                         CreatedAt = now, UpdatedAt = now },
            new() { Id = 16, PropertyId = 2, UnitNumber = "A4", Floor = 1, Bedrooms = 2, Bathrooms = 1.5m, SquareFeet = 1000, MonthlyRent = 1500m, DepositAmount = 1500m, Status = UnitStatus.Occupied, Amenities = "Attached garage, Private patio, Washer/Dryer hookups, Corner unit",             CreatedAt = now, UpdatedAt = now },
            // B-units: 3BR/2.5BA, 1400 sqft
            new() { Id = 17, PropertyId = 2, UnitNumber = "B1", Floor = 1, Bedrooms = 3, Bathrooms = 2.5m, SquareFeet = 1400, MonthlyRent = 1800m, DepositAmount = 1800m, Status = UnitStatus.Occupied, Amenities = "Attached 2-car garage, Private patio, In-unit laundry, Fireplace",              CreatedAt = now, UpdatedAt = now },
            new() { Id = 18, PropertyId = 2, UnitNumber = "B2", Floor = 1, Bedrooms = 3, Bathrooms = 2.5m, SquareFeet = 1400, MonthlyRent = 1850m, DepositAmount = 1850m, Status = UnitStatus.Occupied, Amenities = "Attached 2-car garage, Private patio, In-unit laundry, Fireplace",              CreatedAt = now, UpdatedAt = now },
            new() { Id = 19, PropertyId = 2, UnitNumber = "B3", Floor = 1, Bedrooms = 3, Bathrooms = 2.5m, SquareFeet = 1400, MonthlyRent = 1900m, DepositAmount = 1900m, Status = UnitStatus.Occupied, Amenities = "Attached 2-car garage, Private patio, In-unit laundry, Fireplace",              CreatedAt = now, UpdatedAt = now },
            new() { Id = 20, PropertyId = 2, UnitNumber = "B4", Floor = 1, Bedrooms = 3, Bathrooms = 2.5m, SquareFeet = 1400, MonthlyRent = 1950m, DepositAmount = 1950m, Status = UnitStatus.Occupied, Amenities = "Attached 2-car garage, Private patio, In-unit laundry, Fireplace, Corner unit",  CreatedAt = now, UpdatedAt = now },

            // Lakeview Condos (PropertyId=3) — 1-units: 1BR/1BA, 700 sqft
            new() { Id = 21, PropertyId = 3, UnitNumber = "1A", Floor = 1, Bedrooms = 1, Bathrooms = 1.0m, SquareFeet = 700, MonthlyRent = 1100m, DepositAmount = 1100m, Status = UnitStatus.Occupied, Amenities = "Lake view, Granite countertops, Stainless steel appliances",                     CreatedAt = now, UpdatedAt = now },
            new() { Id = 22, PropertyId = 3, UnitNumber = "1B", Floor = 1, Bedrooms = 1, Bathrooms = 1.0m, SquareFeet = 700, MonthlyRent = 1150m, DepositAmount = 1150m, Status = UnitStatus.Occupied, Amenities = "Granite countertops, Stainless steel appliances",                                CreatedAt = now, UpdatedAt = now },
            new() { Id = 23, PropertyId = 3, UnitNumber = "1C", Floor = 1, Bedrooms = 1, Bathrooms = 1.0m, SquareFeet = 700, MonthlyRent = 1200m, DepositAmount = 1200m, Status = UnitStatus.Occupied, Amenities = "Lake view, Granite countertops, Stainless steel appliances, Corner unit",         CreatedAt = now, UpdatedAt = now },
            // 2-units: 2BR/2BA, 950 sqft
            new() { Id = 24, PropertyId = 3, UnitNumber = "2A", Floor = 2, Bedrooms = 2, Bathrooms = 2.0m, SquareFeet = 950, MonthlyRent = 1500m, DepositAmount = 1500m, Status = UnitStatus.Occupied, Amenities = "Lake view, Granite countertops, In-unit laundry, Walk-in closet",                 CreatedAt = now, UpdatedAt = now },
            new() { Id = 25, PropertyId = 3, UnitNumber = "2B", Floor = 2, Bedrooms = 2, Bathrooms = 2.0m, SquareFeet = 950, MonthlyRent = 1575m, DepositAmount = 1575m, Status = UnitStatus.Occupied, Amenities = "Lake view, Granite countertops, In-unit laundry, Walk-in closet",                 CreatedAt = now, UpdatedAt = now },
            new() { Id = 26, PropertyId = 3, UnitNumber = "2C", Floor = 2, Bedrooms = 2, Bathrooms = 2.0m, SquareFeet = 950, MonthlyRent = 1650m, DepositAmount = 1650m, Status = UnitStatus.Occupied, Amenities = "Lake view, Granite countertops, In-unit laundry, Walk-in closet, Corner unit",     CreatedAt = now, UpdatedAt = now },
        };

        // ========== Tenants ==========
        var tenants = new List<Tenant>
        {
            new() { Id = 1,  FirstName = "Sarah",       LastName = "Johnson",   Email = "sarah.johnson@email.com",       Phone = "503-555-0101", DateOfBirth = new DateOnly(1992, 3, 15),  EmergencyContactName = "Mark Johnson",       EmergencyContactPhone = "503-555-0201", IsActive = true,  CreatedAt = now, UpdatedAt = now },
            new() { Id = 2,  FirstName = "Michael",     LastName = "Chen",      Email = "michael.chen@email.com",        Phone = "503-555-0102", DateOfBirth = new DateOnly(1988, 7, 22),  EmergencyContactName = "Wei Chen",           EmergencyContactPhone = "503-555-0202", IsActive = true,  CreatedAt = now, UpdatedAt = now },
            new() { Id = 3,  FirstName = "Emily",       LastName = "Rodriguez", Email = "emily.rodriguez@email.com",     Phone = "503-555-0103", DateOfBirth = new DateOnly(1995, 11, 8),  EmergencyContactName = "Carlos Rodriguez",   EmergencyContactPhone = "503-555-0203", IsActive = true,  CreatedAt = now, UpdatedAt = now },
            new() { Id = 4,  FirstName = "David",       LastName = "Kim",       Email = "david.kim@email.com",           Phone = "503-555-0104", DateOfBirth = new DateOnly(1990, 1, 30),  EmergencyContactName = "Susan Kim",          EmergencyContactPhone = "503-555-0204", IsActive = true,  CreatedAt = now, UpdatedAt = now },
            new() { Id = 5,  FirstName = "Jessica",     LastName = "Martinez",  Email = "jessica.martinez@email.com",    Phone = "503-555-0105", DateOfBirth = new DateOnly(1993, 5, 12),  EmergencyContactName = "Maria Martinez",     EmergencyContactPhone = "503-555-0205", IsActive = true,  CreatedAt = now, UpdatedAt = now },
            new() { Id = 6,  FirstName = "Robert",      LastName = "Taylor",    Email = "robert.taylor@email.com",       Phone = "503-555-0106", DateOfBirth = new DateOnly(1985, 9, 3),   EmergencyContactName = "Linda Taylor",       EmergencyContactPhone = "503-555-0206", IsActive = true,  CreatedAt = now, UpdatedAt = now },
            new() { Id = 7,  FirstName = "Amanda",      LastName = "Foster",    Email = "amanda.foster@email.com",       Phone = "503-555-0107", DateOfBirth = new DateOnly(1997, 12, 18), EmergencyContactName = "Brian Foster",       EmergencyContactPhone = "503-555-0207", IsActive = true,  CreatedAt = now, UpdatedAt = now },
            new() { Id = 8,  FirstName = "James",       LastName = "Wilson",    Email = "james.wilson@email.com",        Phone = "503-555-0108", DateOfBirth = new DateOnly(1991, 6, 25),  EmergencyContactName = "Patricia Wilson",    EmergencyContactPhone = "503-555-0208", IsActive = true,  CreatedAt = now, UpdatedAt = now },
            new() { Id = 9,  FirstName = "Lisa",        LastName = "Anderson",  Email = "lisa.anderson@email.com",       Phone = "503-555-0109", DateOfBirth = new DateOnly(1989, 4, 7),   EmergencyContactName = "Thomas Anderson",    EmergencyContactPhone = "503-555-0209", IsActive = true,  CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, FirstName = "Christopher", LastName = "Brown",     Email = "christopher.brown@email.com",   Phone = "503-555-0110", DateOfBirth = new DateOnly(1987, 8, 14),  EmergencyContactName = "Nancy Brown",        EmergencyContactPhone = "503-555-0210", IsActive = false, CreatedAt = now, UpdatedAt = now },
        };

        // ========== Leases ==========
        // Lease 9 (Renewed) is listed before Lease 8 (its renewal) for FK insertion order.
        var leases = new List<Lease>
        {
            // Active leases
            new()
            {
                Id = 1, UnitId = 1, TenantId = 1,
                StartDate = today.AddMonths(-10), EndDate = today.AddMonths(2),
                MonthlyRentAmount = 950m, DepositAmount = 950m,
                DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active,
                CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 2, UnitId = 2, TenantId = 2,
                StartDate = today.AddMonths(-6), EndDate = today.AddMonths(6),
                MonthlyRentAmount = 975m, DepositAmount = 975m,
                DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active,
                CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 3, UnitId = 5, TenantId = 3,
                StartDate = today.AddMonths(-8), EndDate = today.AddMonths(4),
                MonthlyRentAmount = 1200m, DepositAmount = 1200m,
                DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active,
                CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 4, UnitId = 8, TenantId = 4,
                StartDate = today.AddMonths(-3), EndDate = today.AddMonths(9),
                MonthlyRentAmount = 1350m, DepositAmount = 1350m,
                DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active,
                CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 5, UnitId = 13, TenantId = 5,
                StartDate = today.AddMonths(-5), EndDate = today.AddMonths(7),
                MonthlyRentAmount = 1400m, DepositAmount = 1400m,
                DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active,
                CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 6, UnitId = 17, TenantId = 6,
                StartDate = today.AddMonths(-9), EndDate = today.AddMonths(3),
                MonthlyRentAmount = 1800m, DepositAmount = 1800m,
                DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active,
                CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = 7, UnitId = 21, TenantId = 7,
                StartDate = today.AddMonths(-4), EndDate = today.AddMonths(8),
                MonthlyRentAmount = 1100m, DepositAmount = 1100m,
                DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active,
                CreatedAt = now, UpdatedAt = now
            },

            // Renewed lease (old lease that was renewed — must appear before its renewal)
            new()
            {
                Id = 9, UnitId = 24, TenantId = 9,
                StartDate = today.AddMonths(-23), EndDate = today.AddMonths(-11),
                MonthlyRentAmount = 1450m, DepositAmount = 1500m,
                DepositStatus = DepositStatus.Held, Status = LeaseStatus.Renewed,
                Notes = "Renewed into lease #8 with updated rent amount.",
                CreatedAt = now, UpdatedAt = now
            },
            // Active renewal of lease 9
            new()
            {
                Id = 8, UnitId = 24, TenantId = 9,
                StartDate = today.AddMonths(-11), EndDate = today.AddMonths(1),
                MonthlyRentAmount = 1500m, DepositAmount = 1500m,
                DepositStatus = DepositStatus.Held, Status = LeaseStatus.Active,
                RenewalOfLeaseId = 9,
                Notes = "Renewal of previous lease with updated monthly rent.",
                CreatedAt = now, UpdatedAt = now
            },

            // Expired lease
            new()
            {
                Id = 10, UnitId = 3, TenantId = 8,
                StartDate = today.AddMonths(-15), EndDate = today.AddMonths(-3),
                MonthlyRentAmount = 1000m, DepositAmount = 1000m,
                DepositStatus = DepositStatus.Returned, Status = LeaseStatus.Expired,
                Notes = "Tenant chose not to renew.",
                CreatedAt = now, UpdatedAt = now
            },

            // Terminated lease
            new()
            {
                Id = 11, UnitId = 10, TenantId = 10,
                StartDate = today.AddMonths(-8), EndDate = today.AddMonths(4),
                MonthlyRentAmount = 1650m, DepositAmount = 1650m,
                DepositStatus = DepositStatus.PartiallyReturned, Status = LeaseStatus.Terminated,
                TerminationDate = today.AddMonths(-2),
                TerminationReason = "Job relocation",
                Notes = "Early termination due to job relocation. Partial deposit returned after deducting early termination fee.",
                CreatedAt = now, UpdatedAt = now
            },

            // Pending lease
            new()
            {
                Id = 12, UnitId = 12, TenantId = 8,
                StartDate = today.AddMonths(1), EndDate = today.AddMonths(13),
                MonthlyRentAmount = 1800m, DepositAmount = 1800m,
                DepositStatus = DepositStatus.Held, Status = LeaseStatus.Pending,
                Notes = "Tenant transferring from unit 103.",
                CreatedAt = now, UpdatedAt = now
            },
        };

        // ========== Payments ==========
        var payments = new List<Payment>
        {
            // --- Lease 1: Sarah Johnson, Unit 101, $950/mo, started 10 months ago ---
            new()
            {
                Id = 1, LeaseId = 1, Amount = 950m,
                PaymentDate = today.AddMonths(-10), DueDate = today.AddMonths(-10),
                PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Deposit,
                Status = PaymentStatus.Completed, ReferenceNumber = "CHK-1001",
                Notes = "Security deposit", CreatedAt = now
            },
            new()
            {
                Id = 2, LeaseId = 1, Amount = 950m,
                PaymentDate = firstOfMonth.AddMonths(-9), DueDate = firstOfMonth.AddMonths(-9),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2001",
                CreatedAt = now
            },
            new()
            {
                Id = 3, LeaseId = 1, Amount = 950m,
                PaymentDate = firstOfMonth.AddMonths(-8).AddDays(7),
                DueDate = firstOfMonth.AddMonths(-8),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2002",
                Notes = "Late payment — 7 days past due", CreatedAt = now
            },
            new()
            {
                Id = 4, LeaseId = 1, Amount = 50m,
                PaymentDate = firstOfMonth.AddMonths(-8).AddDays(7),
                DueDate = firstOfMonth.AddMonths(-8).AddDays(7),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.LateFee,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2003",
                Notes = "Late fee for overdue rent", CreatedAt = now
            },

            // --- Lease 2: Michael Chen, Unit 102, $975/mo, started 6 months ago ---
            new()
            {
                Id = 5, LeaseId = 2, Amount = 975m,
                PaymentDate = today.AddMonths(-6), DueDate = today.AddMonths(-6),
                PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Deposit,
                Status = PaymentStatus.Completed, ReferenceNumber = "CHK-1002",
                Notes = "Security deposit", CreatedAt = now
            },
            new()
            {
                Id = 6, LeaseId = 2, Amount = 975m,
                PaymentDate = firstOfMonth.AddMonths(-5), DueDate = firstOfMonth.AddMonths(-5),
                PaymentMethod = PaymentMethod.CreditCard, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "CC-3001",
                CreatedAt = now
            },
            new()
            {
                Id = 7, LeaseId = 2, Amount = 975m,
                PaymentDate = firstOfMonth.AddMonths(-4), DueDate = firstOfMonth.AddMonths(-4),
                PaymentMethod = PaymentMethod.CreditCard, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "CC-3002",
                CreatedAt = now
            },

            // --- Lease 3: Emily Rodriguez, Unit 201, $1200/mo, started 8 months ago ---
            new()
            {
                Id = 8, LeaseId = 3, Amount = 1200m,
                PaymentDate = today.AddMonths(-8), DueDate = today.AddMonths(-8),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Deposit,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2004",
                Notes = "Security deposit", CreatedAt = now
            },
            new()
            {
                Id = 9, LeaseId = 3, Amount = 1200m,
                PaymentDate = firstOfMonth.AddMonths(-7), DueDate = firstOfMonth.AddMonths(-7),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2005",
                CreatedAt = now
            },
            new()
            {
                Id = 10, LeaseId = 3, Amount = 1200m,
                PaymentDate = firstOfMonth.AddMonths(-6).AddDays(8),
                DueDate = firstOfMonth.AddMonths(-6),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2006",
                Notes = "Late payment — 8 days past due", CreatedAt = now
            },
            new()
            {
                Id = 11, LeaseId = 3, Amount = 75m,
                PaymentDate = firstOfMonth.AddMonths(-6).AddDays(8),
                DueDate = firstOfMonth.AddMonths(-6).AddDays(8),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.LateFee,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2007",
                Notes = "Late fee for overdue rent", CreatedAt = now
            },

            // --- Lease 4: David Kim, Unit 204, $1350/mo, started 3 months ago ---
            new()
            {
                Id = 12, LeaseId = 4, Amount = 1350m,
                PaymentDate = today.AddMonths(-3), DueDate = today.AddMonths(-3),
                PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Deposit,
                Status = PaymentStatus.Completed, ReferenceNumber = "CHK-1003",
                Notes = "Security deposit", CreatedAt = now
            },
            new()
            {
                Id = 13, LeaseId = 4, Amount = 1350m,
                PaymentDate = firstOfMonth.AddMonths(-2), DueDate = firstOfMonth.AddMonths(-2),
                PaymentMethod = PaymentMethod.MoneyOrder, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "MO-4001",
                CreatedAt = now
            },
            new()
            {
                Id = 14, LeaseId = 4, Amount = 1350m,
                PaymentDate = firstOfMonth.AddMonths(-1), DueDate = firstOfMonth.AddMonths(-1),
                PaymentMethod = PaymentMethod.MoneyOrder, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "MO-4002",
                CreatedAt = now
            },

            // --- Lease 5: Jessica Martinez, Unit A1, $1400/mo, started 5 months ago ---
            new()
            {
                Id = 15, LeaseId = 5, Amount = 1400m,
                PaymentDate = today.AddMonths(-5), DueDate = today.AddMonths(-5),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Deposit,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2008",
                Notes = "Security deposit", CreatedAt = now
            },
            new()
            {
                Id = 16, LeaseId = 5, Amount = 1400m,
                PaymentDate = firstOfMonth.AddMonths(-4), DueDate = firstOfMonth.AddMonths(-4),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2009",
                CreatedAt = now
            },
            new()
            {
                Id = 17, LeaseId = 5, Amount = 1400m,
                PaymentDate = firstOfMonth.AddMonths(-3), DueDate = firstOfMonth.AddMonths(-3),
                PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "CHK-1004",
                CreatedAt = now
            },

            // --- Lease 6: Robert Taylor, Unit B1, $1800/mo, started 9 months ago ---
            new()
            {
                Id = 18, LeaseId = 6, Amount = 1800m,
                PaymentDate = today.AddMonths(-9), DueDate = today.AddMonths(-9),
                PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Deposit,
                Status = PaymentStatus.Completed, ReferenceNumber = "CHK-1005",
                Notes = "Security deposit", CreatedAt = now
            },
            new()
            {
                Id = 19, LeaseId = 6, Amount = 1800m,
                PaymentDate = firstOfMonth.AddMonths(-8), DueDate = firstOfMonth.AddMonths(-8),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2010",
                CreatedAt = now
            },
            new()
            {
                Id = 20, LeaseId = 6, Amount = 1800m,
                PaymentDate = firstOfMonth.AddMonths(-7).AddDays(6),
                DueDate = firstOfMonth.AddMonths(-7),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2011",
                Notes = "Late payment — 6 days past due", CreatedAt = now
            },
            new()
            {
                Id = 21, LeaseId = 6, Amount = 100m,
                PaymentDate = firstOfMonth.AddMonths(-7).AddDays(6),
                DueDate = firstOfMonth.AddMonths(-7).AddDays(6),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.LateFee,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2012",
                Notes = "Late fee for overdue rent", CreatedAt = now
            },

            // --- Lease 7: Amanda Foster, Unit 1A, $1100/mo, started 4 months ago ---
            new()
            {
                Id = 22, LeaseId = 7, Amount = 1100m,
                PaymentDate = today.AddMonths(-4), DueDate = today.AddMonths(-4),
                PaymentMethod = PaymentMethod.CreditCard, PaymentType = PaymentType.Deposit,
                Status = PaymentStatus.Completed, ReferenceNumber = "CC-3003",
                Notes = "Security deposit", CreatedAt = now
            },
            new()
            {
                Id = 23, LeaseId = 7, Amount = 1100m,
                PaymentDate = firstOfMonth.AddMonths(-3), DueDate = firstOfMonth.AddMonths(-3),
                PaymentMethod = PaymentMethod.CreditCard, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "CC-3004",
                CreatedAt = now
            },
            new()
            {
                Id = 24, LeaseId = 7, Amount = 1100m,
                PaymentDate = firstOfMonth.AddMonths(-2), DueDate = firstOfMonth.AddMonths(-2),
                PaymentMethod = PaymentMethod.CreditCard, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "CC-3005",
                CreatedAt = now
            },

            // --- Lease 8: Lisa Anderson, Unit 2A, $1500/mo (renewal), started 11 months ago ---
            new()
            {
                Id = 25, LeaseId = 8, Amount = 1500m,
                PaymentDate = firstOfMonth.AddMonths(-10), DueDate = firstOfMonth.AddMonths(-10),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2013",
                CreatedAt = now
            },
            new()
            {
                Id = 26, LeaseId = 8, Amount = 1500m,
                PaymentDate = firstOfMonth.AddMonths(-9), DueDate = firstOfMonth.AddMonths(-9),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2014",
                CreatedAt = now
            },
            new()
            {
                Id = 27, LeaseId = 8, Amount = 1500m,
                PaymentDate = firstOfMonth.AddMonths(-1), DueDate = firstOfMonth.AddMonths(-1),
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Completed, ReferenceNumber = "ACH-2015",
                CreatedAt = now
            },

            // --- Lease 12: James Wilson, pending lease deposit ---
            new()
            {
                Id = 28, LeaseId = 12, Amount = 1800m,
                PaymentDate = today.AddDays(-7), DueDate = today.AddDays(-7),
                PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Deposit,
                Status = PaymentStatus.Completed, ReferenceNumber = "CHK-1006",
                Notes = "Security deposit for upcoming lease", CreatedAt = now
            },

            // --- Lease 1: current month rent (not yet paid) ---
            new()
            {
                Id = 29, LeaseId = 1, Amount = 950m,
                PaymentDate = firstOfMonth, DueDate = firstOfMonth,
                PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Rent,
                Status = PaymentStatus.Pending, ReferenceNumber = null,
                Notes = "Current month — awaiting payment", CreatedAt = now
            },
        };

        // ========== Maintenance Requests ==========
        var maintenanceRequests = new List<MaintenanceRequest>
        {
            // Completed
            new()
            {
                Id = 1, UnitId = 1, TenantId = 1,
                Title = "Leaky kitchen faucet",
                Description = "The kitchen faucet has been dripping constantly for the past two days. Water is pooling under the sink.",
                Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Completed,
                Category = MaintenanceCategory.Plumbing,
                AssignedTo = "Mike's Plumbing",
                SubmittedDate = now.AddDays(-25), AssignedDate = now.AddDays(-20), CompletedDate = now.AddDays(-15),
                CompletionNotes = "Replaced faucet cartridge and tested for leaks. No further issues found.",
                EstimatedCost = 150m, ActualCost = 125m,
                CreatedAt = now, UpdatedAt = now
            },
            // Assigned (High priority)
            new()
            {
                Id = 2, UnitId = 5, TenantId = 3,
                Title = "AC not cooling properly",
                Description = "The central AC unit is running but not producing cold air. Indoor temperature is above 80°F.",
                Priority = MaintenancePriority.High, Status = MaintenanceStatus.Assigned,
                Category = MaintenanceCategory.HVAC,
                AssignedTo = "Cool Air Services",
                SubmittedDate = now.AddDays(-5), AssignedDate = now.AddDays(-3),
                EstimatedCost = 300m,
                CreatedAt = now, UpdatedAt = now
            },
            // Submitted
            new()
            {
                Id = 3, UnitId = 8, TenantId = 4,
                Title = "Broken window lock",
                Description = "The lock on the bedroom window is broken and the window cannot be secured shut.",
                Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Submitted,
                Category = MaintenanceCategory.General,
                SubmittedDate = now.AddDays(-2),
                CreatedAt = now, UpdatedAt = now
            },
            // InProgress
            new()
            {
                Id = 4, UnitId = 13, TenantId = 5,
                Title = "Dishwasher not draining",
                Description = "The dishwasher fills with water but does not drain at the end of the cycle. Standing water remains after every wash.",
                Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.InProgress,
                Category = MaintenanceCategory.Appliance,
                AssignedTo = "Portland Appliance Repair",
                SubmittedDate = now.AddDays(-7), AssignedDate = now.AddDays(-5),
                EstimatedCost = 200m,
                CreatedAt = now, UpdatedAt = now
            },
            // Completed
            new()
            {
                Id = 5, UnitId = 17, TenantId = 6,
                Title = "Ant infestation in kitchen",
                Description = "Large number of ants found around kitchen countertops and near the pantry area.",
                Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Completed,
                Category = MaintenanceCategory.Pest,
                AssignedTo = "Green Pest Control",
                SubmittedDate = now.AddDays(-35), AssignedDate = now.AddDays(-30), CompletedDate = now.AddDays(-25),
                CompletionNotes = "Treated perimeter and kitchen area with pet-safe solution. Follow-up treatment scheduled in 30 days.",
                EstimatedCost = 200m, ActualCost = 175m,
                CreatedAt = now, UpdatedAt = now
            },
            // Submitted
            new()
            {
                Id = 6, UnitId = 21, TenantId = 7,
                Title = "Ceiling light flickering in bedroom",
                Description = "The overhead light in the master bedroom flickers intermittently. Tried replacing the bulb but the issue persists.",
                Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Submitted,
                Category = MaintenanceCategory.Electrical,
                SubmittedDate = now.AddDays(-1),
                CreatedAt = now, UpdatedAt = now
            },
            // Assigned (Emergency)
            new()
            {
                Id = 7, UnitId = 24, TenantId = 9,
                Title = "Water heater making loud banging noise",
                Description = "The water heater is making loud banging and popping noises. Hot water is discolored and has a metallic smell. Concerned about potential safety hazard.",
                Priority = MaintenancePriority.Emergency, Status = MaintenanceStatus.Assigned,
                Category = MaintenanceCategory.Plumbing,
                AssignedTo = "Emergency Plumbing Co.",
                SubmittedDate = now.AddDays(-1), AssignedDate = now.AddDays(-1),
                EstimatedCost = 500m,
                CreatedAt = now, UpdatedAt = now
            },
            // Cancelled
            new()
            {
                Id = 8, UnitId = 2, TenantId = 2,
                Title = "Garbage disposal jammed",
                Description = "The garbage disposal is stuck and making a humming noise when turned on.",
                Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Cancelled,
                Category = MaintenanceCategory.Appliance,
                SubmittedDate = now.AddDays(-10),
                CompletionNotes = "Tenant resolved the issue themselves using the disposal reset button.",
                CreatedAt = now, UpdatedAt = now
            },
        };

        // ========== Inspections ==========
        var inspections = new List<Inspection>
        {
            // MoveIn — completed
            new()
            {
                Id = 1, UnitId = 1, InspectionType = InspectionType.MoveIn,
                ScheduledDate = today.AddMonths(-10), CompletedDate = today.AddMonths(-10),
                InspectorName = "Tom Richards",
                OverallCondition = OverallCondition.Good,
                Notes = "Unit in good condition. Minor scuff marks on living room wall noted. All appliances tested and functioning properly.",
                FollowUpRequired = false, LeaseId = 1,
                CreatedAt = now
            },
            // MoveIn — completed
            new()
            {
                Id = 2, UnitId = 8, InspectionType = InspectionType.MoveIn,
                ScheduledDate = today.AddMonths(-3), CompletedDate = today.AddMonths(-3),
                InspectorName = "Tom Richards",
                OverallCondition = OverallCondition.Excellent,
                Notes = "Unit in excellent condition. Fresh paint throughout. All appliances tested and working. No pre-existing damage noted.",
                FollowUpRequired = false, LeaseId = 4,
                CreatedAt = now
            },
            // MoveOut — completed
            new()
            {
                Id = 3, UnitId = 3, InspectionType = InspectionType.MoveOut,
                ScheduledDate = today.AddMonths(-3), CompletedDate = today.AddMonths(-3),
                InspectorName = "Sandra Lee",
                OverallCondition = OverallCondition.Fair,
                Notes = "Some carpet staining in master bedroom. Minor wall damage in hallway near entrance. Unit requires professional cleaning before next tenant. Deductions applied to security deposit.",
                FollowUpRequired = true, LeaseId = 10,
                CreatedAt = now
            },
            // Routine — scheduled (future)
            new()
            {
                Id = 4, UnitId = 13, InspectionType = InspectionType.Routine,
                ScheduledDate = today.AddDays(14),
                InspectorName = "Tom Richards",
                FollowUpRequired = false,
                CreatedAt = now
            },
            // Routine — scheduled (future)
            new()
            {
                Id = 5, UnitId = 21, InspectionType = InspectionType.Routine,
                ScheduledDate = today.AddMonths(1),
                InspectorName = "Sandra Lee",
                FollowUpRequired = false,
                CreatedAt = now
            },
        };

        // ========== Persist all seed data ==========
        context.AddRange(properties);
        context.AddRange(units);
        context.AddRange(tenants);
        context.AddRange(leases);
        context.AddRange(payments);
        context.AddRange(maintenanceRequests);
        context.AddRange(inspections);

        await context.SaveChangesAsync();
    }
}
