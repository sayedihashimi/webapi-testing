using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VetClinicApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    ZipCode = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Veterinarians",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Specialization = table.Column<string>(type: "TEXT", nullable: true),
                    LicenseNumber = table.Column<string>(type: "TEXT", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    HireDate = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veterinarians", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Species = table.Column<string>(type: "TEXT", nullable: false),
                    Breed = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    MicrochipNumber = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    OwnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pets_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PetId = table.Column<int>(type: "INTEGER", nullable: false),
                    VeterinarianId = table.Column<int>(type: "INTEGER", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 30),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CancellationReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Veterinarians_VeterinarianId",
                        column: x => x.VeterinarianId,
                        principalTable: "Veterinarians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vaccinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PetId = table.Column<int>(type: "INTEGER", nullable: false),
                    VaccineName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DateAdministered = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", nullable: true),
                    AdministeredByVetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vaccinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vaccinations_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vaccinations_Veterinarians_AdministeredByVetId",
                        column: x => x.AdministeredByVetId,
                        principalTable: "Veterinarians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppointmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    PetId = table.Column<int>(type: "INTEGER", nullable: false),
                    VeterinarianId = table.Column<int>(type: "INTEGER", nullable: false),
                    Diagnosis = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Treatment = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    FollowUpDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Veterinarians_VeterinarianId",
                        column: x => x.VeterinarianId,
                        principalTable: "Veterinarians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MedicalRecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DurationDays = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Instructions = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Owners",
                columns: new[] { "Id", "Address", "City", "CreatedAt", "Email", "FirstName", "LastName", "Phone", "State", "UpdatedAt", "ZipCode" },
                values: new object[,]
                {
                    { 1, "123 Oak Street", "Springfield", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "sarah.johnson@email.com", "Sarah", "Johnson", "555-0101", "IL", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "62701" },
                    { 2, "456 Maple Ave", "Portland", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "michael.chen@email.com", "Michael", "Chen", "555-0102", "OR", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "97201" },
                    { 3, "789 Pine Road", "Austin", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "emily.rodriguez@email.com", "Emily", "Rodriguez", "555-0103", "TX", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "73301" },
                    { 4, "321 Elm Blvd", "Seattle", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "david.kim@email.com", "David", "Kim", "555-0104", "WA", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "98101" },
                    { 5, "654 Cedar Lane", "Denver", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "jessica.taylor@email.com", "Jessica", "Taylor", "555-0105", "CO", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "80201" }
                });

            migrationBuilder.InsertData(
                table: "Veterinarians",
                columns: new[] { "Id", "Email", "FirstName", "HireDate", "IsAvailable", "LastName", "LicenseNumber", "Phone", "Specialization" },
                values: new object[,]
                {
                    { 1, "amanda.foster@happypaws.com", "Dr. Amanda", new DateOnly(2015, 6, 1), true, "Foster", "VET-2015-001", "555-0201", "Small Animals" },
                    { 2, "robert.martinez@happypaws.com", "Dr. Robert", new DateOnly(2018, 3, 15), true, "Martinez", "VET-2018-002", "555-0202", "Surgery" }
                });

            migrationBuilder.InsertData(
                table: "Veterinarians",
                columns: new[] { "Id", "Email", "FirstName", "HireDate", "LastName", "LicenseNumber", "Phone", "Specialization" },
                values: new object[] { 3, "lisa.park@happypaws.com", "Dr. Lisa", new DateOnly(2020, 9, 1), "Park", "VET-2020-003", "555-0203", "Exotic Animals" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Breed", "Color", "CreatedAt", "DateOfBirth", "IsActive", "MicrochipNumber", "Name", "OwnerId", "Species", "UpdatedAt", "Weight" },
                values: new object[,]
                {
                    { 1, "Golden Retriever", "Golden", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2020, 3, 15), true, "MC-001-2020", "Buddy", 1, "Dog", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 32.5m },
                    { 2, "Siamese", "Cream", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2019, 7, 22), true, "MC-002-2019", "Whiskers", 1, "Cat", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 4.8m },
                    { 3, "German Shepherd", "Black and Tan", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2021, 1, 10), true, "MC-003-2021", "Max", 2, "Dog", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 38.0m },
                    { 4, "Maine Coon", "Tabby", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2022, 5, 8), true, "MC-004-2022", "Luna", 3, "Cat", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 6.2m },
                    { 5, "Beagle", "Tricolor", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2020, 11, 30), true, "MC-005-2020", "Charlie", 3, "Dog", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 12.5m },
                    { 6, "Cockatiel", "Yellow", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2023, 2, 14), true, null, "Tweety", 4, "Bird", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 0.09m },
                    { 7, "Holland Lop", "White", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2023, 6, 20), true, "MC-007-2023", "Thumper", 5, "Rabbit", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 1.8m }
                });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Breed", "Color", "CreatedAt", "DateOfBirth", "MicrochipNumber", "Name", "OwnerId", "Species", "UpdatedAt", "Weight" },
                values: new object[] { 8, "Bombay", "Black", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2018, 9, 3), "MC-008-2018", "Shadow", 4, "Cat", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 5.1m });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "AppointmentDate", "CancellationReason", "CreatedAt", "DurationMinutes", "Notes", "PetId", "Reason", "Status", "UpdatedAt", "VeterinarianId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 30, "All good", 1, "Annual checkup", "Completed", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, new DateTime(2025, 1, 10, 11, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 30, null, 2, "Vaccination update", "Completed", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 3, new DateTime(2025, 1, 12, 9, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 60, null, 3, "Limping on right front leg", "Completed", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 4, new DateTime(2025, 1, 13, 14, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 30, null, 4, "Skin irritation", "Completed", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 5, new DateTime(2025, 1, 14, 10, 0, 0, 0, DateTimeKind.Utc), "Owner rescheduled", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 30, null, 5, "Dental cleaning", "Cancelled", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 6, new DateTime(2025, 2, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 30, null, 1, "Follow-up visit", "Scheduled", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 7, new DateTime(2025, 2, 1, 11, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 45, null, 6, "Wing clipping and checkup", "Scheduled", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 8, new DateTime(2025, 2, 2, 10, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 30, null, 7, "Nail trimming", "Scheduled", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 9, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 30, null, 3, "Post-surgery follow-up", "CheckedIn", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 10, new DateTime(2025, 1, 8, 15, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 30, null, 5, "Routine vaccination", "NoShow", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "Vaccinations",
                columns: new[] { "Id", "AdministeredByVetId", "BatchNumber", "CreatedAt", "DateAdministered", "ExpirationDate", "Notes", "PetId", "VaccineName" },
                values: new object[,]
                {
                    { 1, 1, "RAB-2024-1001", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 3, 15), new DateOnly(2027, 3, 15), "3-year rabies vaccine", 1, "Rabies" },
                    { 2, 1, "DHPP-2024-2001", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 3, 15), new DateOnly(2025, 3, 15), "Annual booster", 1, "DHPP" },
                    { 3, 1, "FVRCP-2025-3001", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 1, 10), new DateOnly(2026, 1, 10), null, 2, "FVRCP" },
                    { 4, 2, "RAB-2023-4001", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2023, 6, 1), new DateOnly(2024, 6, 1), "1-year rabies vaccine - EXPIRED", 3, "Rabies" },
                    { 5, 2, "DHPP-2024-5001", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 6, 15), new DateOnly(2025, 2, 15), "Expiring soon", 5, "DHPP" },
                    { 6, 3, "RHDV-2024-6001", new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 8, 20), new DateOnly(2025, 8, 20), "Rabbit hemorrhagic disease vaccine", 7, "RHDV2" }
                });

            migrationBuilder.InsertData(
                table: "MedicalRecords",
                columns: new[] { "Id", "AppointmentId", "CreatedAt", "Diagnosis", "FollowUpDate", "Notes", "PetId", "Treatment", "VeterinarianId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Healthy - no issues found", new DateOnly(2026, 1, 10), "Weight is ideal. Teeth in good condition.", 1, "No treatment needed. Continue current diet and exercise.", 1 },
                    { 2, 2, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Due for FVRCP booster", null, "Cat was calm during procedure.", 2, "Administered FVRCP vaccine booster.", 1 },
                    { 3, 3, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Mild sprain in right front leg", new DateOnly(2025, 1, 26), "X-ray showed no fracture.", 3, "Rest for 2 weeks. Anti-inflammatory medication prescribed.", 2 },
                    { 4, 4, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Allergic dermatitis", new DateOnly(2025, 2, 13), "Likely food allergy. Trial elimination diet.", 4, "Topical corticosteroid cream. Dietary adjustment recommended.", 1 }
                });

            migrationBuilder.InsertData(
                table: "Prescriptions",
                columns: new[] { "Id", "CreatedAt", "Dosage", "DurationDays", "EndDate", "Instructions", "MedicalRecordId", "MedicationName", "StartDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "25mg twice daily", 14, new DateOnly(2025, 1, 26), "Give with food. Monitor for stomach upset.", 3, "Carprofen", new DateOnly(2025, 1, 12) },
                    { 2, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Apply thin layer twice daily", 21, new DateOnly(2025, 2, 3), "Apply to affected areas. Prevent licking with cone if needed.", 4, "Hydrocortisone Cream", new DateOnly(2025, 1, 13) },
                    { 3, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "50mg as needed", 7, new DateOnly(2025, 1, 19), "Only if showing signs of significant pain.", 3, "Tramadol", new DateOnly(2025, 1, 12) },
                    { 4, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "1 capsule daily", 90, new DateOnly(2025, 4, 13), "Mix with food. Supports skin health.", 4, "Omega-3 Fish Oil", new DateOnly(2025, 1, 13) },
                    { 5, new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "1 tablet monthly", 365, new DateOnly(2026, 1, 10), "Give on the same day each month with a meal.", 1, "Heartworm Prevention", new DateOnly(2025, 1, 10) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PetId",
                table: "Appointments",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_VeterinarianId",
                table: "Appointments",
                column: "VeterinarianId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_AppointmentId",
                table: "MedicalRecords",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PetId",
                table: "MedicalRecords",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_VeterinarianId",
                table: "MedicalRecords",
                column: "VeterinarianId");

            migrationBuilder.CreateIndex(
                name: "IX_Owners_Email",
                table: "Owners",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pets_MicrochipNumber",
                table: "Pets",
                column: "MicrochipNumber",
                unique: true,
                filter: "[MicrochipNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_OwnerId",
                table: "Pets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicalRecordId",
                table: "Prescriptions",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Vaccinations_AdministeredByVetId",
                table: "Vaccinations",
                column: "AdministeredByVetId");

            migrationBuilder.CreateIndex(
                name: "IX_Vaccinations_PetId",
                table: "Vaccinations",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Veterinarians_Email",
                table: "Veterinarians",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Veterinarians_LicenseNumber",
                table: "Veterinarians",
                column: "LicenseNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Vaccinations");

            migrationBuilder.DropTable(
                name: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Veterinarians");

            migrationBuilder.DropTable(
                name: "Owners");
        }
    }
}
