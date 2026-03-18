using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Specializations = table.Column<string>(type: "TEXT", nullable: true),
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
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EmergencyContactName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EmergencyContactPhone = table.Column<string>(type: "TEXT", nullable: false),
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
                    CancellationReason = table.Column<string>(type: "TEXT", nullable: true),
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
                    CancellationReason = table.Column<string>(type: "TEXT", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClassScheduleId_MemberId",
                table: "Bookings",
                columns: new[] { "ClassScheduleId", "MemberId" },
                unique: true,
                filter: "\"Status\" != 'Cancelled'");

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
