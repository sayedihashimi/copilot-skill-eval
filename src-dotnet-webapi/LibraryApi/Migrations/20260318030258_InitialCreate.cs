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
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    MembershipDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    MembershipType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
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
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
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
                    { 1, "English novelist and essayist, journalist and critic.", new DateOnly(1903, 6, 25), "United Kingdom", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "George", "Orwell" },
                    { 2, "English novelist known primarily for her six major novels.", new DateOnly(1775, 12, 16), "United Kingdom", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Jane", "Austen" },
                    { 3, "American writer and professor of biochemistry, known for science fiction.", new DateOnly(1920, 1, 2), "United States", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Isaac", "Asimov" },
                    { 4, "Israeli public intellectual, historian and professor.", new DateOnly(1976, 2, 24), "Israel", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Yuval Noah", "Harari" },
                    { 5, "English writer known for her detective novels.", new DateOnly(1890, 9, 15), "United Kingdom", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Agatha", "Christie" },
                    { 6, "American astronomer, planetary scientist, and science communicator.", new DateOnly(1934, 11, 9), "United States", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carl", "Sagan" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "AvailableCopies", "CreatedAt", "Description", "ISBN", "Language", "PageCount", "PublicationYear", "Publisher", "Title", "TotalCopies", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 3, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "A dystopian novel set in a totalitarian society.", "978-0-451-52493-5", "English", 328, 1949, "Secker & Warburg", "1984", 5, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "An allegorical novella about a group of farm animals.", "978-0-451-52634-2", "English", 112, 1945, "Secker & Warburg", "Animal Farm", 3, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 4, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "A romantic novel following Elizabeth Bennet.", "978-0-14-143951-8", "English", 432, 1813, "T. Egerton", "Pride and Prejudice", 4, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "A novel about the Dashwood sisters.", "978-0-14-143966-2", "English", 409, 1811, "T. Egerton", "Sense and Sensibility", 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 1, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "The first novel in the Foundation series.", "978-0-553-29335-7", "English", 244, 1951, "Gnome Press", "Foundation", 3, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 1, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "A collection of robot short stories.", "978-0-553-29438-5", "English", 253, 1950, "Gnome Press", "I, Robot", 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "A book about the history of the human species.", "978-0-06-231609-7", "English", 443, 2011, "Harper", "Sapiens: A Brief History of Humankind", 4, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 3, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "An exploration of humanity's future.", "978-0-06-246431-6", "English", 450, 2015, "Harper", "Homo Deus: A Brief History of Tomorrow", 3, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "A detective novel featuring Hercule Poirot.", "978-0-00-711931-8", "English", 256, 1934, "Collins Crime Club", "Murder on the Orient Express", 3, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "A Poirot mystery novel.", "978-0-00-712717-7", "English", 312, 1926, "Collins Crime Club", "The Murder of Roger Ackroyd", 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "A popular science book on astronomy.", "978-0-345-53943-4", "English", 396, 1980, "Random House", "Cosmos", 3, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "A book about scientific thinking and skepticism.", "978-0-345-40946-1", "English", 457, 1995, "Random House", "The Demon-Haunted World", 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Literary works of imaginative narration", "Fiction" },
                    { 2, "Fiction based on imagined future scientific advances", "Science Fiction" },
                    { 3, "Non-fiction works about historical events", "History" },
                    { 4, "Non-fiction works about scientific topics", "Science" },
                    { 5, "Accounts of someone's life written by another", "Biography" },
                    { 6, "Fiction dealing with the solution of a crime", "Mystery" }
                });

            migrationBuilder.InsertData(
                table: "Patrons",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "MembershipDate", "MembershipType", "Phone", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "123 Main St, Springfield", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "alice.johnson@email.com", "Alice", true, "Johnson", new DateOnly(2023, 1, 15), "Premium", "555-0101", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "456 Oak Ave, Springfield", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "bob.smith@email.com", "Bob", true, "Smith", new DateOnly(2023, 3, 20), "Standard", "555-0102", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "789 Pine Rd, Springfield", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "carol.williams@email.com", "Carol", true, "Williams", new DateOnly(2023, 6, 10), "Student", "555-0103", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "321 Elm St, Springfield", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "david.brown@email.com", "David", true, "Brown", new DateOnly(2023, 9, 5), "Standard", "555-0104", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "654 Birch Ln, Springfield", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "eva.martinez@email.com", "Eva", true, "Martinez", new DateOnly(2024, 1, 1), "Premium", "555-0105", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Patrons",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FirstName", "LastName", "MembershipDate", "MembershipType", "Phone", "UpdatedAt" },
                values: new object[] { 6, "987 Cedar Dr, Springfield", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), "frank.davis@email.com", "Frank", "Davis", new DateOnly(2022, 5, 1), "Standard", "555-0106", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "BookAuthors",
                columns: new[] { "AuthorId", "BookId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 3 },
                    { 2, 4 },
                    { 3, 5 },
                    { 3, 6 },
                    { 4, 7 },
                    { 4, 8 },
                    { 5, 9 },
                    { 5, 10 },
                    { 6, 11 },
                    { 6, 12 }
                });

            migrationBuilder.InsertData(
                table: "BookCategories",
                columns: new[] { "BookId", "CategoryId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 2 },
                    { 6, 2 },
                    { 7, 3 },
                    { 7, 5 },
                    { 8, 3 },
                    { 8, 4 },
                    { 9, 1 },
                    { 9, 6 },
                    { 10, 1 },
                    { 10, 6 },
                    { 11, 4 },
                    { 12, 4 }
                });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "Id", "BookId", "CreatedAt", "DueDate", "LoanDate", "PatronId", "RenewalCount", "ReturnDate", "Status" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 10, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 10, 0, 0, 0, DateTimeKind.Utc), 1, 0, null, "Active" },
                    { 2, 1, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 5, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 22, 10, 0, 0, 0, DateTimeKind.Utc), 2, 0, null, "Active" },
                    { 3, 5, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 5, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1, 1, null, "Active" },
                    { 4, 5, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 25, 10, 0, 0, 0, DateTimeKind.Utc), 3, 0, null, "Active" },
                    { 5, 7, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 24, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 10, 10, 0, 0, 0, DateTimeKind.Utc), 2, 0, null, "Overdue" },
                    { 6, 7, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 18, 10, 0, 0, 0, DateTimeKind.Utc), 4, 0, null, "Active" },
                    { 7, 3, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 4, 22, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 4, 1, 10, 0, 0, 0, DateTimeKind.Utc), 1, 0, new DateTime(2024, 4, 20, 10, 0, 0, 0, DateTimeKind.Utc), "Returned" },
                    { 8, 9, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 4, 10, 10, 0, 0, 0, DateTimeKind.Utc), 5, 0, new DateTime(2024, 5, 5, 10, 0, 0, 0, DateTimeKind.Utc), "Returned" },
                    { 9, 2, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 11, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 28, 10, 0, 0, 0, DateTimeKind.Utc), 4, 0, null, "Active" },
                    { 10, 6, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 10, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 10, 0, 0, 0, DateTimeKind.Utc), 5, 0, null, "Active" },
                    { 11, 9, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 2, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 26, 10, 0, 0, 0, DateTimeKind.Utc), 3, 0, null, "Active" },
                    { 12, 11, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 12, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 22, 10, 0, 0, 0, DateTimeKind.Utc), 1, 0, null, "Active" }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "BookId", "CreatedAt", "ExpirationDate", "PatronId", "QueuePosition", "ReservationDate", "Status" },
                values: new object[,]
                {
                    { 1, 5, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, new DateTime(2024, 5, 28, 10, 0, 0, 0, DateTimeKind.Utc), "Pending" },
                    { 2, 5, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, 5, 2, new DateTime(2024, 5, 29, 10, 0, 0, 0, DateTimeKind.Utc), "Pending" },
                    { 3, 1, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, 3, 1, new DateTime(2024, 5, 30, 10, 0, 0, 0, DateTimeKind.Utc), "Pending" }
                });

            migrationBuilder.InsertData(
                table: "Fines",
                columns: new[] { "Id", "Amount", "CreatedAt", "IssuedDate", "LoanId", "PaidDate", "PatronId", "Reason", "Status" },
                values: new object[,]
                {
                    { 1, 2.00m, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 24, 10, 0, 0, 0, DateTimeKind.Utc), 5, null, 2, "Overdue return - 8 days late", "Unpaid" },
                    { 2, 1.00m, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 5, 10, 0, 0, 0, DateTimeKind.Utc), 8, new DateTime(2024, 5, 10, 10, 0, 0, 0, DateTimeKind.Utc), 5, "Overdue return - 4 days late", "Paid" },
                    { 3, 0.50m, new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 30, 10, 0, 0, 0, DateTimeKind.Utc), 9, null, 4, "Damaged book cover", "Unpaid" }
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
