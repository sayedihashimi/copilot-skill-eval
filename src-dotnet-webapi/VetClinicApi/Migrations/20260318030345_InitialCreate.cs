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
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
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
                    Species = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Breed = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    MicrochipNumber = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
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
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
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
                    { 1, "123 Oak Street", "Springfield", new DateTime(2024, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "sarah.johnson@email.com", "Sarah", "Johnson", "555-0101", "IL", new DateTime(2024, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "62701" },
                    { 2, "456 Maple Avenue", "Portland", new DateTime(2024, 2, 1, 10, 0, 0, 0, DateTimeKind.Utc), "michael.chen@email.com", "Michael", "Chen", "555-0102", "OR", new DateTime(2024, 2, 1, 10, 0, 0, 0, DateTimeKind.Utc), "97201" },
                    { 3, "789 Pine Road", "Austin", new DateTime(2024, 2, 15, 10, 0, 0, 0, DateTimeKind.Utc), "emily.rodriguez@email.com", "Emily", "Rodriguez", "555-0103", "TX", new DateTime(2024, 2, 15, 10, 0, 0, 0, DateTimeKind.Utc), "73301" },
                    { 4, "321 Elm Drive", "Denver", new DateTime(2024, 3, 1, 10, 0, 0, 0, DateTimeKind.Utc), "david.thompson@email.com", "David", "Thompson", "555-0104", "CO", new DateTime(2024, 3, 1, 10, 0, 0, 0, DateTimeKind.Utc), "80201" },
                    { 5, "654 Birch Lane", "Seattle", new DateTime(2024, 3, 15, 10, 0, 0, 0, DateTimeKind.Utc), "jessica.williams@email.com", "Jessica", "Williams", "555-0105", "WA", new DateTime(2024, 3, 15, 10, 0, 0, 0, DateTimeKind.Utc), "98101" }
                });

            migrationBuilder.InsertData(
                table: "Veterinarians",
                columns: new[] { "Id", "Email", "FirstName", "HireDate", "IsAvailable", "LastName", "LicenseNumber", "Phone", "Specialization" },
                values: new object[,]
                {
                    { 1, "amanda.foster@happypaws.com", "Dr. Amanda", new DateOnly(2015, 6, 1), true, "Foster", "VET-2015-001", "555-0201", "General Practice" },
                    { 2, "robert.kim@happypaws.com", "Dr. Robert", new DateOnly(2018, 3, 15), true, "Kim", "VET-2018-002", "555-0202", "Surgery" },
                    { 3, "lisa.patel@happypaws.com", "Dr. Lisa", new DateOnly(2020, 9, 1), true, "Patel", "VET-2020-003", "555-0203", "Exotic Animals" }
                });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Breed", "Color", "CreatedAt", "DateOfBirth", "IsActive", "MicrochipNumber", "Name", "OwnerId", "Species", "UpdatedAt", "Weight" },
                values: new object[,]
                {
                    { 1, "Golden Retriever", "Golden", new DateTime(2024, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2020, 3, 15), true, "MC-001-2020", "Buddy", 1, "Dog", new DateTime(2024, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 32.5m },
                    { 2, "Siamese", "Cream", new DateTime(2024, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2019, 7, 22), true, "MC-002-2019", "Whiskers", 1, "Cat", new DateTime(2024, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 4.8m },
                    { 3, "German Shepherd", "Black and Tan", new DateTime(2024, 2, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2021, 1, 10), true, "MC-003-2021", "Max", 2, "Dog", new DateTime(2024, 2, 1, 10, 0, 0, 0, DateTimeKind.Utc), 38.0m },
                    { 4, "Maine Coon", "Tabby", new DateTime(2024, 2, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2022, 5, 8), true, "MC-004-2022", "Luna", 3, "Cat", new DateTime(2024, 2, 15, 10, 0, 0, 0, DateTimeKind.Utc), 6.2m },
                    { 5, "Beagle", "Tricolor", new DateTime(2024, 2, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2021, 11, 3), true, "MC-005-2021", "Charlie", 3, "Dog", new DateTime(2024, 2, 15, 10, 0, 0, 0, DateTimeKind.Utc), 12.5m },
                    { 6, "Cockatiel", "Yellow", new DateTime(2024, 3, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2023, 2, 14), true, null, "Tweety", 4, "Bird", new DateTime(2024, 3, 1, 10, 0, 0, 0, DateTimeKind.Utc), 0.09m },
                    { 7, "Holland Lop", "Brown", new DateTime(2024, 3, 15, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2023, 6, 20), true, "MC-007-2023", "Thumper", 5, "Rabbit", new DateTime(2024, 3, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1.8m },
                    { 8, "Russian Blue", "Blue-Gray", new DateTime(2024, 3, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2018, 9, 1), false, "MC-008-2018", "Shadow", 4, "Cat", new DateTime(2024, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), 5.5m }
                });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "AppointmentDate", "CancellationReason", "CreatedAt", "DurationMinutes", "Notes", "PetId", "Reason", "Status", "UpdatedAt", "VeterinarianId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 10, 9, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 5, 10, 0, 0, 0, DateTimeKind.Utc), 30, "All vitals normal", 1, "Annual wellness checkup", "Completed", new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), 30, "Vaccines administered successfully", 2, "Vaccination update", "Completed", new DateTime(2025, 1, 15, 11, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 3, new DateTime(2025, 2, 5, 14, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 2, 1, 10, 0, 0, 0, DateTimeKind.Utc), 60, "X-ray showed minor sprain", 3, "Limping on right front leg", "Completed", new DateTime(2025, 2, 5, 15, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 4, new DateTime(2025, 3, 1, 11, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 2, 25, 10, 0, 0, 0, DateTimeKind.Utc), 30, "Teeth cleaned, no issues found", 4, "Dental cleaning", "Completed", new DateTime(2025, 3, 1, 12, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 5, new DateTime(2025, 3, 10, 9, 0, 0, 0, DateTimeKind.Utc), "Owner rescheduled", new DateTime(2025, 3, 5, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 5, "Skin irritation check", "Cancelled", new DateTime(2025, 3, 8, 10, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 6, new DateTime(2025, 3, 20, 15, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 3, 15, 10, 0, 0, 0, DateTimeKind.Utc), 45, null, 6, "Wing clipping and health check", "NoShow", new DateTime(2025, 3, 20, 16, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 7, new DateTime(2025, 8, 15, 9, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 1, "Follow-up wellness checkup", "Scheduled", new DateTime(2025, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 8, new DateTime(2025, 8, 20, 10, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 6, 10, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 5, "Skin irritation follow-up", "Scheduled", new DateTime(2025, 6, 10, 10, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 9, new DateTime(2025, 8, 25, 14, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 6, 15, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 7, "Routine rabbit health check", "Scheduled", new DateTime(2025, 6, 15, 10, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 10, new DateTime(2025, 9, 1, 11, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 6, 20, 10, 0, 0, 0, DateTimeKind.Utc), 30, null, 3, "Annual vaccination", "Scheduled", new DateTime(2025, 6, 20, 10, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "Vaccinations",
                columns: new[] { "Id", "AdministeredByVetId", "BatchNumber", "CreatedAt", "DateAdministered", "ExpirationDate", "Notes", "PetId", "VaccineName" },
                values: new object[,]
                {
                    { 1, 1, "RAB-2025-001", new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 1, 10), new DateOnly(2026, 1, 10), "3-year rabies vaccine", 1, "Rabies" },
                    { 2, 1, "DHPP-2025-001", new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 1, 10), new DateOnly(2026, 1, 10), null, 1, "DHPP" },
                    { 3, 1, "FVR-2025-001", new DateTime(2025, 1, 15, 11, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 1, 15), new DateOnly(2026, 1, 15), null, 2, "FVRCP" },
                    { 4, 2, "BOR-2024-003", new DateTime(2024, 7, 20, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 7, 20), new DateOnly(2025, 7, 20), "Annual booster", 3, "Bordetella" },
                    { 5, 1, "RAB-2023-010", new DateTime(2023, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2023, 6, 1), new DateOnly(2024, 6, 1), "Needs renewal", 4, "Rabies" },
                    { 6, 2, "DHPP-2024-015", new DateTime(2024, 11, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 11, 1), new DateOnly(2025, 11, 1), null, 5, "DHPP" }
                });

            migrationBuilder.InsertData(
                table: "MedicalRecords",
                columns: new[] { "Id", "AppointmentId", "CreatedAt", "Diagnosis", "FollowUpDate", "Notes", "PetId", "Treatment", "VeterinarianId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), "Healthy - no issues detected", new DateOnly(2025, 7, 10), "Weight is ideal for breed and age.", 1, "No treatment required. Recommended continued regular exercise and balanced diet.", 1 },
                    { 2, 2, new DateTime(2025, 1, 15, 11, 0, 0, 0, DateTimeKind.Utc), "Healthy - vaccines due", null, "Cat was slightly anxious but cooperative.", 2, "Administered FVRCP and Rabies boosters.", 1 },
                    { 3, 3, new DateTime(2025, 2, 5, 15, 0, 0, 0, DateTimeKind.Utc), "Minor sprain in right front leg", new DateOnly(2025, 2, 19), "X-ray confirmed no fracture. Recommend limiting activity.", 3, "Prescribed anti-inflammatory medication and rest for 2 weeks. Applied cold compress.", 2 },
                    { 4, 4, new DateTime(2025, 3, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Mild tartar buildup, otherwise healthy gums", null, "Recommend dental treats to help maintain oral health.", 4, "Professional dental cleaning performed. Polished all teeth.", 1 }
                });

            migrationBuilder.InsertData(
                table: "Prescriptions",
                columns: new[] { "Id", "CreatedAt", "Dosage", "DurationDays", "Instructions", "MedicalRecordId", "MedicationName", "StartDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 2, 5, 15, 0, 0, 0, DateTimeKind.Utc), "25mg twice daily", 14, "Give with food. Monitor for any GI issues.", 3, "Carprofen", new DateOnly(2025, 2, 5) },
                    { 2, new DateTime(2025, 2, 5, 15, 0, 0, 0, DateTimeKind.Utc), "500mg once daily", 30, "Can be mixed with food.", 3, "Glucosamine Supplement", new DateOnly(2025, 2, 5) },
                    { 3, new DateTime(2025, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), "1 chewable monthly", 180, "Give on the same day each month.", 1, "Heartgard Plus", new DateOnly(2025, 1, 10) },
                    { 4, new DateTime(2025, 3, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Apply once daily", 90, "Brush teeth gently. Use pet-specific toothbrush.", 4, "Dental Enzyme Toothpaste", new DateOnly(2025, 3, 1) },
                    { 5, new DateTime(2025, 1, 15, 11, 0, 0, 0, DateTimeKind.Utc), "250mg once daily", 7, "Mix with wet food.", 2, "Lysine Supplement", new DateOnly(2025, 1, 15) }
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
                filter: "MicrochipNumber IS NOT NULL");

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
