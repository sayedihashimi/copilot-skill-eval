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
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CancellationReason = table.Column<string>(type: "TEXT", nullable: true),
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
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Owners",
                columns: new[] { "Id", "Address", "City", "CreatedAt", "Email", "FirstName", "LastName", "Phone", "State", "UpdatedAt", "ZipCode" },
                values: new object[,]
                {
                    { 1, "123 Oak Street", "Portland", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "sarah.johnson@email.com", "Sarah", "Johnson", "555-0101", "OR", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "97201" },
                    { 2, "456 Maple Avenue", "Seattle", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "michael.chen@email.com", "Michael", "Chen", "555-0102", "WA", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "98101" },
                    { 3, "789 Pine Road", "San Francisco", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "emily.rodriguez@email.com", "Emily", "Rodriguez", "555-0103", "CA", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "94102" },
                    { 4, "321 Elm Boulevard", "Denver", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "james.wilson@email.com", "James", "Wilson", "555-0104", "CO", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "80201" },
                    { 5, "654 Cedar Lane", "Austin", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "lisa.thompson@email.com", "Lisa", "Thompson", "555-0105", "TX", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "73301" }
                });

            migrationBuilder.InsertData(
                table: "Veterinarians",
                columns: new[] { "Id", "Email", "FirstName", "HireDate", "IsAvailable", "LastName", "LicenseNumber", "Phone", "Specialization" },
                values: new object[,]
                {
                    { 1, "amanda.foster@happypaws.com", "Dr. Amanda", new DateOnly(2015, 6, 1), true, "Foster", "VET-2015-001", "555-0201", "General Practice" },
                    { 2, "robert.kim@happypaws.com", "Dr. Robert", new DateOnly(2018, 3, 15), true, "Kim", "VET-2018-002", "555-0202", "Surgery" },
                    { 3, "priya.patel@happypaws.com", "Dr. Priya", new DateOnly(2020, 9, 1), true, "Patel", "VET-2020-003", "555-0203", "Dentistry" }
                });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Breed", "Color", "CreatedAt", "DateOfBirth", "IsActive", "MicrochipNumber", "Name", "OwnerId", "Species", "UpdatedAt", "Weight" },
                values: new object[,]
                {
                    { 1, "Golden Retriever", "Golden", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2020, 3, 15), true, "MC-001-2020", "Buddy", 1, "Dog", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 32.5m },
                    { 2, "Siamese", "Cream", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2019, 7, 22), true, "MC-002-2019", "Whiskers", 1, "Cat", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 4.8m },
                    { 3, "German Shepherd", "Black and Tan", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2021, 1, 10), true, "MC-003-2021", "Max", 2, "Dog", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 38.0m },
                    { 4, "Maine Coon", "Tabby", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2022, 5, 8), true, "MC-004-2022", "Luna", 3, "Cat", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 6.2m },
                    { 5, "Beagle", "Tricolor", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2023, 2, 14), true, "MC-005-2023", "Charlie", 3, "Dog", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 12.3m },
                    { 6, "Cockatiel", "Yellow", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2021, 11, 3), true, null, "Tweety", 4, "Bird", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 0.09m },
                    { 7, "Labrador Retriever", "Chocolate", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2018, 9, 20), true, "MC-007-2018", "Bella", 5, "Dog", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 28.5m },
                    { 8, "Holland Lop", "Brown", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2023, 6, 1), true, null, "Thumper", 4, "Rabbit", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1.8m }
                });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "AppointmentDate", "CancellationReason", "CreatedAt", "DurationMinutes", "Notes", "PetId", "Reason", "Status", "UpdatedAt", "VeterinarianId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 5, 9, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 30, "Routine checkup", 1, "Annual wellness exam", "Completed", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, new DateTime(2026, 1, 8, 10, 30, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 60, "X-ray recommended", 3, "Limping on right front leg", "Completed", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 3, new DateTime(2026, 1, 10, 14, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 45, "Plaque buildup noticed", 2, "Dental cleaning", "Completed", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 4, new DateTime(2026, 1, 12, 11, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 7, "Vaccination booster", "Completed", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 5, new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 5, "Puppy wellness check", "Scheduled", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 6, new DateTime(2026, 6, 16, 10, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 4, "Skin irritation follow-up", "Scheduled", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 7, new DateTime(2026, 6, 17, 14, 30, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 45, null, 1, "Hip evaluation", "Scheduled", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 8, new DateTime(2026, 6, 18, 11, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 6, "Wing feather trim and checkup", "Scheduled", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 9, new DateTime(2026, 6, 19, 9, 30, 0, 0, DateTimeKind.Utc), "Owner had scheduling conflict", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 8, "Routine checkup", "Cancelled", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 10, new DateTime(2026, 1, 6, 15, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 3, "Follow-up on ear infection", "NoShow", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 2 }
                });

            migrationBuilder.InsertData(
                table: "Vaccinations",
                columns: new[] { "Id", "AdministeredByVetId", "BatchNumber", "CreatedAt", "DateAdministered", "ExpirationDate", "Notes", "PetId", "VaccineName" },
                values: new object[,]
                {
                    { 1, 1, "RAB-2025-001", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 3, 15), new DateOnly(2026, 3, 15), "3-year vaccine", 1, "Rabies" },
                    { 2, 1, "DHPP-2026-010", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 1, 5), new DateOnly(2027, 1, 5), null, 1, "DHPP" },
                    { 3, 2, "RAB-2025-045", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 6, 10), new DateOnly(2026, 6, 10), null, 3, "Rabies" },
                    { 4, 1, "FVRCP-2025-012", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 1, 20), new DateOnly(2026, 1, 20), "Annual feline vaccine", 2, "FVRCP" },
                    { 5, 1, "DHPP-2026-015", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 1, 12), new DateOnly(2027, 1, 12), null, 7, "DHPP" },
                    { 6, 3, "BORD-2025-033", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 8, 1), new DateOnly(2026, 2, 1), "Kennel cough vaccine", 5, "Bordetella" }
                });

            migrationBuilder.InsertData(
                table: "MedicalRecords",
                columns: new[] { "Id", "AppointmentId", "CreatedAt", "Diagnosis", "FollowUpDate", "Notes", "PetId", "Treatment", "VeterinarianId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Healthy, no concerns", null, "Weight stable, coat healthy", 1, "Routine wellness exam completed. All vitals normal.", 1 },
                    { 2, 2, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Mild sprain in right front leg", new DateOnly(2026, 1, 22), "X-ray showed no fracture", 3, "Rest for 2 weeks, anti-inflammatory medication prescribed", 2 },
                    { 3, 3, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Stage 1 periodontal disease", new DateOnly(2026, 4, 10), "Two teeth with early decay, monitoring needed", 2, "Professional dental cleaning performed under anesthesia", 3 },
                    { 4, 4, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Healthy, vaccination administered", null, "No adverse reactions observed", 7, "DHPP booster vaccine administered", 1 }
                });

            migrationBuilder.InsertData(
                table: "Prescriptions",
                columns: new[] { "Id", "CreatedAt", "Dosage", "DurationDays", "EndDate", "Instructions", "MedicalRecordId", "MedicationName", "StartDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "25mg twice daily", 14, new DateOnly(2026, 1, 22), "Give with food. Stop if vomiting occurs.", 2, "Carprofen", new DateOnly(2026, 1, 8) },
                    { 2, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "100mg once daily", 7, new DateOnly(2026, 1, 15), "For pain management. May cause drowsiness.", 2, "Gabapentin", new DateOnly(2026, 1, 8) },
                    { 3, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "75mg twice daily", 10, new DateOnly(2026, 1, 20), "Complete full course of antibiotics.", 3, "Clindamycin", new DateOnly(2026, 1, 10) },
                    { 4, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "1 chewable monthly", 365, new DateOnly(2027, 1, 5), "Monthly heartworm prevention. Give on the same day each month.", 1, "Heartgard Plus", new DateOnly(2026, 1, 5) },
                    { 5, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "25mg as needed", 3, new DateOnly(2026, 1, 15), "Only if mild swelling at injection site occurs.", 4, "Benadryl", new DateOnly(2026, 1, 12) }
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
