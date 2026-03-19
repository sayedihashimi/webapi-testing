using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FitnessStudioApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DefaultDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultCapacity = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPremium = table.Column<bool>(type: "INTEGER", nullable: false),
                    CaloriesPerSession = table.Column<int>(type: "INTEGER", nullable: true),
                    DifficultyLevel = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instructors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Specializations = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    HireDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EmergencyContactName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EmergencyContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    JoinDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MembershipPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DurationMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaxClassBookingsPerWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowsPremiumClasses = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    InstructorId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentEnrollment = table.Column<int>(type: "INTEGER", nullable: false),
                    WaitlistCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Room = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CancellationReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassSchedules_ClassTypes_ClassTypeId",
                        column: x => x.ClassTypeId,
                        principalTable: "ClassTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassSchedules_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    MembershipPlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    PaymentStatus = table.Column<string>(type: "TEXT", nullable: false),
                    FreezeStartDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    FreezeEndDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Memberships_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Memberships_MembershipPlans_MembershipPlanId",
                        column: x => x.MembershipPlanId,
                        principalTable: "MembershipPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    WaitlistPosition = table.Column<int>(type: "INTEGER", nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CancellationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CancellationReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_ClassSchedules_ClassScheduleId",
                        column: x => x.ClassScheduleId,
                        principalTable: "ClassSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ClassTypes",
                columns: new[] { "Id", "CaloriesPerSession", "CreatedAt", "DefaultCapacity", "DefaultDurationMinutes", "Description", "DifficultyLevel", "IsActive", "IsPremium", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 250, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, 60, "Vinyasa flow yoga for flexibility and mindfulness", "AllLevels", true, false, "Yoga", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 500, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, 45, "High-intensity interval training for maximum calorie burn", "Intermediate", true, false, "HIIT", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 450, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, 45, "Indoor cycling for cardiovascular endurance", "Intermediate", true, false, "Spin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 300, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, 60, "Core strengthening and body alignment", "Beginner", true, true, "Pilates", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 600, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, 60, "Boxing fundamentals and cardio boxing workout", "Advanced", true, true, "Boxing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 50, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, 30, "Guided meditation and breathing exercises for stress relief", "Beginner", true, false, "Meditation", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Instructors",
                columns: new[] { "Id", "Bio", "CreatedAt", "Email", "FirstName", "HireDate", "IsActive", "LastName", "Phone", "Specializations", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Certified yoga instructor with 10 years of experience", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sarah.kim@zenithfitness.com", "Sarah", new DateOnly(2020, 1, 15), true, "Kim", "555-1001", "Yoga,Pilates,Meditation", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Former competitive athlete specializing in high-intensity training", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mike.torres@zenithfitness.com", "Mike", new DateOnly(2021, 3, 1), true, "Torres", "555-1002", "HIIT,Boxing,Spin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Pilates and mindfulness expert with a holistic approach", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "priya.patel@zenithfitness.com", "Priya", new DateOnly(2022, 6, 15), true, "Patel", "555-1003", "Pilates,Yoga,Meditation", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "Professional boxing coach and strength training specialist", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "david.okonkwo@zenithfitness.com", "David", new DateOnly(2023, 1, 10), true, "Okonkwo", "555-1004", "Boxing,HIIT,Spin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "CreatedAt", "DateOfBirth", "Email", "EmergencyContactName", "EmergencyContactPhone", "FirstName", "IsActive", "JoinDate", "LastName", "Phone", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(1990, 3, 15), "alice.johnson@email.com", "Bob Johnson", "555-0102", "Alice", true, new DateOnly(2024, 6, 1), "Johnson", "555-0101", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(1985, 7, 22), "marcus.chen@email.com", "Li Chen", "555-0202", "Marcus", true, new DateOnly(2024, 7, 15), "Chen", "555-0201", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(1992, 11, 8), "sophia.martinez@email.com", "Carlos Martinez", "555-0302", "Sophia", true, new DateOnly(2024, 8, 1), "Martinez", "555-0301", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(1988, 1, 30), "james.williams@email.com", "Sarah Williams", "555-0402", "James", true, new DateOnly(2024, 5, 1), "Williams", "555-0401", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(1995, 5, 12), "emma.davis@email.com", "Tom Davis", "555-0502", "Emma", true, new DateOnly(2024, 9, 1), "Davis", "555-0501", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(1993, 9, 25), "daniel.brown@email.com", "Lisa Brown", "555-0602", "Daniel", true, new DateOnly(2024, 4, 15), "Brown", "555-0601", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2000, 2, 14), "olivia.wilson@email.com", "Mark Wilson", "555-0702", "Olivia", true, new DateOnly(2024, 10, 1), "Wilson", "555-0701", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(1987, 12, 3), "ryan.taylor@email.com", "Karen Taylor", "555-0802", "Ryan", false, new DateOnly(2024, 3, 1), "Taylor", "555-0801", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "MembershipPlans",
                columns: new[] { "Id", "AllowsPremiumClasses", "CreatedAt", "Description", "DurationMonths", "IsActive", "MaxClassBookingsPerWeek", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Access to standard classes, 3 bookings per week", 1, true, 3, "Basic", 29.99m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Access to all classes including premium, 5 bookings per week", 1, true, 5, "Premium", 49.99m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unlimited access to all classes and premium features", 1, true, -1, "Elite", 79.99m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "ClassSchedules",
                columns: new[] { "Id", "CancellationReason", "Capacity", "ClassTypeId", "CreatedAt", "CurrentEnrollment", "EndTime", "InstructorId", "Room", "StartTime", "Status", "UpdatedAt", "WaitlistCount" },
                values: new object[,]
                {
                    { 1, null, 20, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, new DateTime(2025, 7, 7, 9, 0, 0, 0, DateTimeKind.Utc), 1, "Studio A", new DateTime(2025, 7, 7, 8, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 2, null, 15, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, new DateTime(2025, 7, 7, 10, 45, 0, 0, DateTimeKind.Utc), 2, "Studio B", new DateTime(2025, 7, 7, 10, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 3, null, 12, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateTime(2025, 7, 8, 7, 45, 0, 0, DateTimeKind.Utc), 2, "Spin Room", new DateTime(2025, 7, 8, 7, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 4, null, 15, 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateTime(2025, 7, 8, 10, 0, 0, 0, DateTimeKind.Utc), 3, "Studio A", new DateTime(2025, 7, 8, 9, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 5, null, 10, 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateTime(2025, 7, 9, 18, 0, 0, 0, DateTimeKind.Utc), 4, "Boxing Ring", new DateTime(2025, 7, 9, 17, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 6, null, 25, 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2025, 7, 9, 19, 30, 0, 0, DateTimeKind.Utc), 1, "Zen Room", new DateTime(2025, 7, 9, 19, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 7, null, 20, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2025, 7, 10, 9, 0, 0, 0, DateTimeKind.Utc), 3, "Studio A", new DateTime(2025, 7, 10, 8, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 8, null, 15, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2025, 7, 10, 18, 45, 0, 0, DateTimeKind.Utc), 4, "Studio B", new DateTime(2025, 7, 10, 18, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 9, null, 12, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, new DateTime(2025, 7, 11, 7, 45, 0, 0, DateTimeKind.Utc), 2, "Spin Room", new DateTime(2025, 7, 11, 7, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 10, null, 10, 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, new DateTime(2025, 7, 11, 18, 0, 0, 0, DateTimeKind.Utc), 4, "Boxing Ring", new DateTime(2025, 7, 11, 17, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 11, null, 3, 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, new DateTime(2025, 7, 12, 10, 0, 0, 0, DateTimeKind.Utc), 3, "Studio A", new DateTime(2025, 7, 12, 9, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 12, null, 25, 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2025, 7, 12, 19, 30, 0, 0, DateTimeKind.Utc), 1, "Zen Room", new DateTime(2025, 7, 12, 19, 0, 0, 0, DateTimeKind.Utc), "Scheduled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 }
                });

            migrationBuilder.InsertData(
                table: "Memberships",
                columns: new[] { "Id", "CreatedAt", "EndDate", "FreezeEndDate", "FreezeStartDate", "MemberId", "MembershipPlanId", "PaymentStatus", "StartDate", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 7, 1), null, null, 1, 2, "Paid", new DateOnly(2025, 6, 1), "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 7, 1), null, null, 2, 3, "Paid", new DateOnly(2025, 6, 1), "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 7, 1), null, null, 3, 1, "Paid", new DateOnly(2025, 6, 1), "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 7, 1), null, null, 4, 2, "Paid", new DateOnly(2025, 6, 1), "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 7, 1), null, null, 5, 1, "Paid", new DateOnly(2025, 6, 1), "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 7, 1), null, null, 6, 3, "Paid", new DateOnly(2025, 6, 1), "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 7, 1), null, null, 1, 1, "Paid", new DateOnly(2024, 6, 1), "Expired", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 4, 1), null, null, 8, 1, "Refunded", new DateOnly(2024, 3, 1), "Cancelled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Bookings",
                columns: new[] { "Id", "BookingDate", "CancellationDate", "CancellationReason", "CheckInTime", "ClassScheduleId", "CreatedAt", "MemberId", "Status", "UpdatedAt", "WaitlistPosition" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 13, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 14, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 16, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 17, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 18, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 19, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Waitlisted", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 20, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Waitlisted", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 21, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "Confirmed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClassScheduleId",
                table: "Bookings",
                column: "ClassScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_MemberId",
                table: "Bookings",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_ClassTypeId",
                table: "ClassSchedules",
                column: "ClassTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_InstructorId",
                table: "ClassSchedules",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTypes_Name",
                table: "ClassTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_Email",
                table: "Instructors",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_Email",
                table: "Members",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlans_Name",
                table: "MembershipPlans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MemberId",
                table: "Memberships",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MembershipPlanId",
                table: "Memberships",
                column: "MembershipPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "ClassSchedules");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "MembershipPlans");

            migrationBuilder.DropTable(
                name: "ClassTypes");

            migrationBuilder.DropTable(
                name: "Instructors");
        }
    }
}
