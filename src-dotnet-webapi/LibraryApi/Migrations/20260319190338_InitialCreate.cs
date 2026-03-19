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
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    ISBN = table.Column<string>(type: "TEXT", nullable: false),
                    Publisher = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PublicationYear = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    PageCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Language = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "English"),
                    TotalCopies = table.Column<int>(type: "INTEGER", nullable: false),
                    AvailableCopies = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    MembershipDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    MembershipType = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    RenewalCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loans_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Loans_Patrons_PatronId",
                        column: x => x.PatronId,
                        principalTable: "Patrons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    QueuePosition = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Patrons_PatronId",
                        column: x => x.PatronId,
                        principalTable: "Patrons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatronId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoanId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fines_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fines_Patrons_PatronId",
                        column: x => x.PatronId,
                        principalTable: "Patrons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "Biography", "BirthDate", "Country", "CreatedAt", "FirstName", "LastName" },
                values: new object[,]
                {
                    { 1, "English novelist known for her six major novels.", new DateOnly(1775, 12, 16), "United Kingdom", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Jane", "Austen" },
                    { 2, "English novelist and essayist, journalist and critic.", new DateOnly(1903, 6, 25), "United Kingdom", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "George", "Orwell" },
                    { 3, "Colombian novelist and Nobel Prize winner.", new DateOnly(1927, 3, 6), "Colombia", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Gabriel", "Garcia Marquez" },
                    { 4, "American novelist and Nobel Prize in Literature laureate.", new DateOnly(1931, 2, 18), "United States", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Toni", "Morrison" },
                    { 5, "Japanese writer known for surreal and dreamlike fiction.", new DateOnly(1949, 1, 12), "Japan", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Haruki", "Murakami" },
                    { 6, "American writer and professor of biochemistry, prolific author of science fiction.", new DateOnly(1920, 1, 2), "United States", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Isaac", "Asimov" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "AvailableCopies", "CreatedAt", "Description", "ISBN", "Language", "PageCount", "PublicationYear", "Publisher", "Title", "TotalCopies", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "A romantic novel of manners.", "978-0-14-028329-7", "English", 432, 1813, "Penguin Classics", "Pride and Prejudice", 3, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "A dystopian social science fiction novel.", "978-0-45-152493-5", "English", 328, 1949, "Signet Classic", "1984", 4, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 1, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "The multi-generational story of the Buendia family.", "978-0-06-088328-7", "English", 417, 1967, "Harper Perennial", "One Hundred Years of Solitude", 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "A novel inspired by the story of Margaret Garner.", "978-1-40-003341-6", "English", 324, 1987, "Vintage", "Beloved", 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 0, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "A nostalgic story of loss and sexuality.", "978-0-37-571894-0", "English", 296, 1987, "Vintage International", "Norwegian Wood", 1, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "The first novel in Isaac Asimov's Foundation series.", "978-0-55-338257-3", "English", 244, 1951, "Bantam Spectra", "Foundation", 3, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 4, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "An allegorical novella about Soviet totalitarianism.", "978-0-45-152634-2", "English", 112, 1945, "Signet Classic", "Animal Farm", 5, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "A metaphysical novel by Haruki Murakami.", "978-1-40-000290-9", "English", 467, 2002, "Vintage International", "Kafka on the Shore", 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 1, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "A collection of nine science fiction short stories.", "978-0-55-338256-6", "English", 224, 1950, "Bantam Spectra", "I, Robot", 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 1, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "A novel by Toni Morrison about African-American identity.", "978-1-40-003342-3", "English", 337, 1977, "Vintage", "Song of Solomon", 1, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Jane Austen's first published novel.", "978-0-14-143966-4", "English", 409, 1811, "Penguin Classics", "Sense and Sensibility", 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "A love story spanning over fifty years.", "978-0-14-024990-3", "English", 348, 1985, "Penguin Books", "Love in the Time of Cholera", 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Literary and general fiction works", "Fiction" },
                    { 2, "Speculative fiction dealing with futuristic concepts", "Science Fiction" },
                    { 3, "Non-fiction works about historical events and periods", "History" },
                    { 4, "Books covering scientific topics and discoveries", "Science" },
                    { 5, "Accounts of a person's life written by someone else", "Biography" },
                    { 6, "Timeless works of literary significance", "Classic Literature" }
                });

            migrationBuilder.InsertData(
                table: "Patrons",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "MembershipDate", "MembershipType", "Phone", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "123 Main St", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "alice.johnson@email.com", "Alice", true, "Johnson", new DateOnly(2023, 1, 15), "Premium", "555-0101", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "456 Oak Ave", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "bob.smith@email.com", "Bob", true, "Smith", new DateOnly(2023, 3, 20), "Standard", "555-0102", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "789 Pine Rd", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "carol.williams@email.com", "Carol", true, "Williams", new DateOnly(2023, 6, 1), "Student", "555-0103", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "321 Elm St", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "david.brown@email.com", "David", true, "Brown", new DateOnly(2023, 9, 10), "Standard", "555-0104", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "654 Birch Ln", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "eva.martinez@email.com", "Eva", true, "Martinez", new DateOnly(2024, 1, 5), "Premium", "555-0105", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Patrons",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FirstName", "LastName", "MembershipDate", "MembershipType", "Phone", "UpdatedAt" },
                values: new object[] { 6, "987 Cedar Dr", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "frank.davis@email.com", "Frank", "Davis", new DateOnly(2022, 5, 20), "Standard", "555-0106", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "BookAuthors",
                columns: new[] { "AuthorId", "BookId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 4 },
                    { 5, 5 },
                    { 6, 6 },
                    { 2, 7 },
                    { 5, 8 },
                    { 6, 9 },
                    { 4, 10 },
                    { 1, 11 },
                    { 3, 12 }
                });

            migrationBuilder.InsertData(
                table: "BookCategories",
                columns: new[] { "BookId", "CategoryId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 6 },
                    { 2, 1 },
                    { 2, 2 },
                    { 3, 1 },
                    { 3, 6 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 2 },
                    { 7, 1 },
                    { 7, 6 },
                    { 8, 1 },
                    { 9, 2 },
                    { 10, 1 },
                    { 11, 1 },
                    { 11, 6 },
                    { 12, 1 }
                });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "Id", "BookId", "CreatedAt", "DueDate", "LoanDate", "PatronId", "RenewalCount", "ReturnDate", "Status" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 5, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 26, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 5, 10, 0, 0, 0, DateTimeKind.Utc), 1, 0, null, "Active" },
                    { 2, 2, new DateTime(2025, 1, 8, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 22, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 8, 10, 0, 0, 0, DateTimeKind.Utc), 2, 0, null, "Active" },
                    { 3, 5, new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 17, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), 3, 0, null, "Active" },
                    { 4, 3, new DateTime(2024, 12, 26, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 9, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 12, 26, 10, 0, 0, 0, DateTimeKind.Utc), 2, 0, null, "Overdue" },
                    { 5, 9, new DateTime(2024, 12, 28, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 11, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 12, 28, 10, 0, 0, 0, DateTimeKind.Utc), 4, 0, null, "Overdue" },
                    { 6, 7, new DateTime(2024, 12, 16, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 6, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 12, 16, 10, 0, 0, 0, DateTimeKind.Utc), 1, 0, new DateTime(2025, 1, 3, 10, 0, 0, 0, DateTimeKind.Utc), "Returned" },
                    { 7, 2, new DateTime(2024, 12, 21, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 11, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 12, 21, 10, 0, 0, 0, DateTimeKind.Utc), 5, 1, new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), "Returned" },
                    { 8, 6, new DateTime(2024, 12, 31, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 14, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 12, 31, 10, 0, 0, 0, DateTimeKind.Utc), 4, 0, new DateTime(2025, 1, 13, 10, 0, 0, 0, DateTimeKind.Utc), "Returned" }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "BookId", "CreatedAt", "ExpirationDate", "PatronId", "QueuePosition", "ReservationDate", "Status" },
                values: new object[,]
                {
                    { 1, 5, new DateTime(2025, 1, 12, 10, 0, 0, 0, DateTimeKind.Utc), null, 1, 1, new DateTime(2025, 1, 12, 10, 0, 0, 0, DateTimeKind.Utc), "Pending" },
                    { 2, 5, new DateTime(2025, 1, 13, 10, 0, 0, 0, DateTimeKind.Utc), null, 4, 2, new DateTime(2025, 1, 13, 10, 0, 0, 0, DateTimeKind.Utc), "Pending" },
                    { 3, 2, new DateTime(2025, 1, 14, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 17, 10, 0, 0, 0, DateTimeKind.Utc), 3, 1, new DateTime(2025, 1, 14, 10, 0, 0, 0, DateTimeKind.Utc), "Ready" }
                });

            migrationBuilder.InsertData(
                table: "Fines",
                columns: new[] { "Id", "Amount", "CreatedAt", "IssuedDate", "LoanId", "PaidDate", "PatronId", "Reason", "Status" },
                values: new object[,]
                {
                    { 1, 1.50m, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 4, null, 2, "Overdue return", "Unpaid" },
                    { 2, 1.00m, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 5, null, 4, "Overdue return", "Unpaid" },
                    { 3, 0.25m, new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), 7, new DateTime(2025, 1, 11, 10, 0, 0, 0, DateTimeKind.Utc), 5, "Overdue return - 1 day late", "Paid" }
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
