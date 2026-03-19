using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Biography = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ISBN = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Publisher = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PublicationYear = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    PageCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Language = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "English"),
                    TotalCopies = table.Column<int>(type: "INTEGER", nullable: false),
                    AvailableCopies = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patrons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    MembershipDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    MembershipType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patrons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookAuthors",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAuthors", x => new { x.BookId, x.AuthorId });
                    table.ForeignKey(
                        name: "FK_BookAuthors_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookAuthors_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCategories",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCategories", x => new { x.BookId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_BookCategories_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    PatronId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoanDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RenewalCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loans_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loans_Patrons_PatronId",
                        column: x => x.PatronId,
                        principalTable: "Patrons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    PatronId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    QueuePosition = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Patrons_PatronId",
                        column: x => x.PatronId,
                        principalTable: "Patrons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatronId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoanId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fines_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fines_Patrons_PatronId",
                        column: x => x.PatronId,
                        principalTable: "Patrons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "Biography", "BirthDate", "Country", "CreatedAt", "FirstName", "LastName" },
                values: new object[,]
                {
                    { 1, "American novelist known for To Kill a Mockingbird.", new DateOnly(1926, 4, 28), "United States", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Harper", "Lee" },
                    { 2, "English novelist and essayist, known for 1984 and Animal Farm.", new DateOnly(1903, 6, 25), "United Kingdom", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "George", "Orwell" },
                    { 3, "American author and biochemistry professor, prolific science fiction writer.", new DateOnly(1920, 1, 2), "United States", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Isaac", "Asimov" },
                    { 4, "Israeli historian and author of Sapiens and Homo Deus.", new DateOnly(1976, 2, 24), "Israel", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Yuval Noah", "Harari" },
                    { 5, "American journalist and biographer.", new DateOnly(1952, 5, 20), "United States", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Walter", "Isaacson" },
                    { 6, "English novelist known for Pride and Prejudice and other classic works.", new DateOnly(1775, 12, 16), "United Kingdom", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Jane", "Austen" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "AvailableCopies", "CreatedAt", "Description", "ISBN", "Language", "PageCount", "PublicationYear", "Publisher", "Title", "TotalCopies", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A novel about racial injustice in the American South.", "978-0-06-112008-4", "English", 281, 1960, "J.B. Lippincott & Co.", "To Kill a Mockingbird", 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A dystopian novel set in a totalitarian society.", "978-0-452-28423-4", "English", 328, 1949, "Secker & Warburg", "1984", 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "An allegorical novella about a group of farm animals.", "978-0-452-28424-1", "English", 112, 1945, "Secker & Warburg", "Animal Farm", 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "The first novel in Asimov's Foundation series.", "978-0-553-29335-7", "English", 244, 1951, "Gnome Press", "Foundation", 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A collection of nine science fiction short stories.", "978-0-553-29438-5", "English", 253, 1950, "Gnome Press", "I, Robot", 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A narrative history of humankind from the Stone Age to the present.", "978-0-06-231609-7", "English", 443, 2011, "Harper", "Sapiens: A Brief History of Humankind", 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "An exploration of humanity's future.", "978-0-06-246431-6", "English", 450, 2015, "Harper", "Homo Deus: A Brief History of Tomorrow", 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "The authorized biography of Apple co-founder Steve Jobs.", "978-1-4516-4853-9", "English", 656, 2011, "Simon & Schuster", "Steve Jobs", 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A classic novel of manners set in Georgian England.", "978-0-14-143951-8", "English", 432, 1813, "T. Egerton", "Pride and Prejudice", 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A novel about the Dashwood sisters and their romantic pursuits.", "978-0-14-143966-2", "English", 409, 1811, "T. Egerton", "Sense and Sensibility", 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A science fiction detective novel featuring robot R. Daneel Olivaw.", "978-0-553-29340-1", "English", 206, 1954, "Doubleday", "The Caves of Steel", 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Explores the biggest questions facing humanity today.", "978-0-525-51217-2", "English", 372, 2018, "Spiegel & Grau", "21 Lessons for the 21st Century", 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Literary works created from the imagination", "Fiction" },
                    { 2, "Fiction dealing with futuristic science and technology", "Science Fiction" },
                    { 3, "Non-fiction works about past events", "History" },
                    { 4, "Non-fiction works about the natural world", "Science" },
                    { 5, "Non-fiction accounts of a person's life", "Biography" }
                });

            migrationBuilder.InsertData(
                table: "Patrons",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "MembershipDate", "MembershipType", "Phone", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "123 Oak Street", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "alice.johnson@email.com", "Alice", true, "Johnson", new DateOnly(2023, 6, 15), "Premium", "555-0101", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "456 Elm Avenue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bob.smith@email.com", "Bob", true, "Smith", new DateOnly(2024, 1, 10), "Standard", "555-0102", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "789 Pine Road", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "carol.williams@email.com", "Carol", true, "Williams", new DateOnly(2024, 9, 1), "Student", "555-0103", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "321 Maple Drive", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "david.brown@email.com", "David", true, "Brown", new DateOnly(2024, 3, 20), "Standard", "555-0104", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Patrons",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FirstName", "LastName", "MembershipDate", "MembershipType", "Phone", "UpdatedAt" },
                values: new object[] { 5, "654 Cedar Lane", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "eve.davis@email.com", "Eve", "Davis", new DateOnly(2023, 11, 5), "Premium", "555-0105", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Patrons",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "MembershipDate", "MembershipType", "Phone", "UpdatedAt" },
                values: new object[] { 6, "987 Birch Court", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "frank.wilson@email.com", "Frank", true, "Wilson", new DateOnly(2024, 8, 15), "Student", "555-0106", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "BookAuthors",
                columns: new[] { "AuthorId", "BookId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 2, 3 },
                    { 3, 4 },
                    { 3, 5 },
                    { 4, 6 },
                    { 4, 7 },
                    { 5, 8 },
                    { 6, 9 },
                    { 6, 10 },
                    { 3, 11 },
                    { 4, 12 }
                });

            migrationBuilder.InsertData(
                table: "BookCategories",
                columns: new[] { "BookId", "CategoryId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 2, 2 },
                    { 3, 1 },
                    { 4, 2 },
                    { 5, 2 },
                    { 5, 4 },
                    { 6, 3 },
                    { 6, 4 },
                    { 7, 3 },
                    { 7, 4 },
                    { 8, 5 },
                    { 9, 1 },
                    { 10, 1 },
                    { 11, 2 },
                    { 12, 3 }
                });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "Id", "BookId", "CreatedAt", "DueDate", "LoanDate", "PatronId", "RenewalCount", "ReturnDate", "Status" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2025, 3, 10, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 31, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 10, 10, 0, 0, 0, DateTimeKind.Utc), 1, 0, null, "Active" },
                    { 2, 4, new DateTime(2025, 3, 5, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 19, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 5, 10, 0, 0, 0, DateTimeKind.Utc), 2, 0, null, "Active" },
                    { 3, 6, new DateTime(2025, 3, 12, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 19, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 12, 10, 0, 0, 0, DateTimeKind.Utc), 3, 0, null, "Active" },
                    { 4, 9, new DateTime(2025, 2, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 22, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 1, 10, 0, 0, 0, DateTimeKind.Utc), 1, 0, new DateTime(2025, 2, 24, 10, 0, 0, 0, DateTimeKind.Utc), "Returned" },
                    { 5, 1, new DateTime(2025, 2, 23, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 9, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 23, 10, 0, 0, 0, DateTimeKind.Utc), 2, 0, null, "Overdue" },
                    { 6, 5, new DateTime(2025, 3, 8, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 22, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 8, 10, 0, 0, 0, DateTimeKind.Utc), 4, 0, null, "Active" },
                    { 7, 3, new DateTime(2025, 3, 5, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 12, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 5, 10, 0, 0, 0, DateTimeKind.Utc), 6, 0, null, "Overdue" },
                    { 8, 7, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 5, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1, 0, new DateTime(2025, 2, 3, 10, 0, 0, 0, DateTimeKind.Utc), "Returned" }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "BookId", "CreatedAt", "ExpirationDate", "PatronId", "QueuePosition", "ReservationDate", "Status" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 3, 10, 10, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, new DateTime(2025, 3, 10, 10, 0, 0, 0, DateTimeKind.Utc), "Pending" },
                    { 2, 3, new DateTime(2025, 3, 11, 10, 0, 0, 0, DateTimeKind.Utc), null, 3, 1, new DateTime(2025, 3, 11, 10, 0, 0, 0, DateTimeKind.Utc), "Pending" },
                    { 3, 5, new DateTime(2025, 3, 8, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 18, 10, 0, 0, 0, DateTimeKind.Utc), 1, 1, new DateTime(2025, 3, 8, 10, 0, 0, 0, DateTimeKind.Utc), "Ready" }
                });

            migrationBuilder.InsertData(
                table: "Fines",
                columns: new[] { "Id", "Amount", "CreatedAt", "IssuedDate", "LoanId", "PaidDate", "PatronId", "Reason", "Status" },
                values: new object[,]
                {
                    { 1, 1.50m, new DateTime(2025, 3, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 15, 10, 0, 0, 0, DateTimeKind.Utc), 5, null, 2, "Overdue: 6 days late", "Unpaid" },
                    { 2, 0.75m, new DateTime(2025, 3, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 15, 10, 0, 0, 0, DateTimeKind.Utc), 7, null, 6, "Overdue: 3 days late", "Unpaid" },
                    { 3, 0.50m, new DateTime(2025, 2, 24, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 24, 10, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 2, 25, 10, 0, 0, 0, DateTimeKind.Utc), 1, "Overdue: 2 days late", "Paid" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookAuthors_AuthorId",
                table: "BookAuthors",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCategories_CategoryId",
                table: "BookCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_ISBN",
                table: "Books",
                column: "ISBN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fines_LoanId",
                table: "Fines",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_PatronId",
                table: "Fines",
                column: "PatronId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_BookId",
                table: "Loans",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_PatronId",
                table: "Loans",
                column: "PatronId");

            migrationBuilder.CreateIndex(
                name: "IX_Patrons_Email",
                table: "Patrons",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookId",
                table: "Reservations",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PatronId",
                table: "Reservations",
                column: "PatronId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookAuthors");

            migrationBuilder.DropTable(
                name: "BookCategories");

            migrationBuilder.DropTable(
                name: "Fines");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Patrons");
        }
    }
}
