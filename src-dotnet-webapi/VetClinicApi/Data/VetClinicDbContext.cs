using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public class VetClinicDbContext : DbContext
{
    public VetClinicDbContext(DbContextOptions<VetClinicDbContext> options) : base(options) { }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Owner ---
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        // --- Pet ---
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasIndex(e => e.MicrochipNumber).IsUnique().HasFilter("MicrochipNumber IS NOT NULL");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Species).HasMaxLength(50);
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.HasOne(e => e.Owner)
                  .WithMany(o => o.Pets)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Ignore(e => e.MedicalRecords);
        });

        // --- Veterinarian ---
        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        // --- Appointment ---
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.HasOne(e => e.Pet)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(e => e.PetId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Veterinarian)
                  .WithMany(v => v.Appointments)
                  .HasForeignKey(e => e.VeterinarianId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- MedicalRecord ---
        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.Property(e => e.Diagnosis).HasMaxLength(1000);
            entity.Property(e => e.Treatment).HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.HasOne(e => e.Appointment)
                  .WithOne(a => a.MedicalRecord)
                  .HasForeignKey<MedicalRecord>(e => e.AppointmentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Pet)
                  .WithMany()
                  .HasForeignKey(e => e.PetId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Veterinarian)
                  .WithMany()
                  .HasForeignKey(e => e.VeterinarianId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Prescription ---
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.Property(e => e.MedicationName).HasMaxLength(200);
            entity.Property(e => e.Dosage).HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(500);
            entity.Ignore(e => e.EndDate);
            entity.Ignore(e => e.IsActive);
            entity.HasOne(e => e.MedicalRecord)
                  .WithMany(m => m.Prescriptions)
                  .HasForeignKey(e => e.MedicalRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Vaccination ---
        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.Property(e => e.VaccineName).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsDueSoon);
            entity.HasOne(e => e.Pet)
                  .WithMany(p => p.Vaccinations)
                  .HasForeignKey(e => e.PetId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AdministeredByVet)
                  .WithMany()
                  .HasForeignKey(e => e.AdministeredByVetId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // --- Owners ---
        modelBuilder.Entity<Owner>().HasData(
            new Owner { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701", CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Owner { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Owner { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = new DateTime(2024, 2, 15, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 2, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Owner { Id = 4, FirstName = "David", LastName = "Thompson", Email = "david.thompson@email.com", Phone = "555-0104", Address = "321 Elm Drive", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = new DateTime(2024, 3, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 3, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Owner { Id = 5, FirstName = "Jessica", LastName = "Williams", Email = "jessica.williams@email.com", Phone = "555-0105", Address = "654 Birch Lane", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = new DateTime(2024, 3, 15, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 3, 15, 10, 0, 0, DateTimeKind.Utc) }
        );

        // --- Pets ---
        modelBuilder.Entity<Pet>().HasData(
            new Pet { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", IsActive = true, OwnerId = 1, CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Pet { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m, Color = "Cream", MicrochipNumber = "MC-002-2019", IsActive = true, OwnerId = 1, CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Pet { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", IsActive = true, OwnerId = 2, CreatedAt = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Pet { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 6.2m, Color = "Tabby", MicrochipNumber = "MC-004-2022", IsActive = true, OwnerId = 3, CreatedAt = new DateTime(2024, 2, 15, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 2, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Pet { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2021, 11, 3), Weight = 12.5m, Color = "Tricolor", MicrochipNumber = "MC-005-2021", IsActive = true, OwnerId = 3, CreatedAt = new DateTime(2024, 2, 15, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 2, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Pet { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.09m, Color = "Yellow", MicrochipNumber = null, IsActive = true, OwnerId = 4, CreatedAt = new DateTime(2024, 3, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 3, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Pet { Id = 7, Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 20), Weight = 1.8m, Color = "Brown", MicrochipNumber = "MC-007-2023", IsActive = true, OwnerId = 5, CreatedAt = new DateTime(2024, 3, 15, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 3, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Pet { Id = 8, Name = "Shadow", Species = "Cat", Breed = "Russian Blue", DateOfBirth = new DateOnly(2018, 9, 1), Weight = 5.5m, Color = "Blue-Gray", MicrochipNumber = "MC-008-2018", IsActive = false, OwnerId = 4, CreatedAt = new DateTime(2024, 3, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc) }
        );

        // --- Veterinarians ---
        modelBuilder.Entity<Veterinarian>().HasData(
            new Veterinarian { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new Veterinarian { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new Veterinarian { Id = 3, FirstName = "Dr. Lisa", LastName = "Patel", Email = "lisa.patel@happypaws.com", Phone = "555-0203", Specialization = "Exotic Animals", LicenseNumber = "VET-2020-003", IsAvailable = true, HireDate = new DateOnly(2020, 9, 1) }
        );

        // --- Appointments ---
        // Use future dates relative to a "now" of 2025-07-01 for seed data
        modelBuilder.Entity<Appointment>().HasData(
            // Completed appointments (past)
            new Appointment { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 10, 9, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual wellness checkup", Notes = "All vitals normal", CreatedAt = new DateTime(2025, 1, 5, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 2, PetId = 2, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Vaccination update", Notes = "Vaccines administered successfully", CreatedAt = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 15, 11, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 3, PetId = 3, VeterinarianId = 2, AppointmentDate = new DateTime(2025, 2, 5, 14, 0, 0, DateTimeKind.Utc), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on right front leg", Notes = "X-ray showed minor sprain", CreatedAt = new DateTime(2025, 2, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 2, 5, 15, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 4, PetId = 4, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 3, 1, 11, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", Notes = "Teeth cleaned, no issues found", CreatedAt = new DateTime(2025, 2, 25, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 3, 1, 12, 0, 0, DateTimeKind.Utc) },
            // Cancelled
            new Appointment { Id = 5, PetId = 5, VeterinarianId = 2, AppointmentDate = new DateTime(2025, 3, 10, 9, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Skin irritation check", CancellationReason = "Owner rescheduled", CreatedAt = new DateTime(2025, 3, 5, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 3, 8, 10, 0, 0, DateTimeKind.Utc) },
            // NoShow
            new Appointment { Id = 6, PetId = 6, VeterinarianId = 3, AppointmentDate = new DateTime(2025, 3, 20, 15, 0, 0, DateTimeKind.Utc), DurationMinutes = 45, Status = AppointmentStatus.NoShow, Reason = "Wing clipping and health check", CreatedAt = new DateTime(2025, 3, 15, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 3, 20, 16, 0, 0, DateTimeKind.Utc) },
            // Future scheduled
            new Appointment { Id = 7, PetId = 1, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 8, 15, 9, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up wellness checkup", CreatedAt = new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 8, PetId = 5, VeterinarianId = 2, AppointmentDate = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Skin irritation follow-up", CreatedAt = new DateTime(2025, 6, 10, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 10, 10, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 9, PetId = 7, VeterinarianId = 3, AppointmentDate = new DateTime(2025, 8, 25, 14, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Routine rabbit health check", CreatedAt = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 10, PetId = 3, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 9, 1, 11, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Annual vaccination", CreatedAt = new DateTime(2025, 6, 20, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 20, 10, 0, 0, DateTimeKind.Utc) }
        );

        // --- Medical Records (for completed appointments) ---
        modelBuilder.Entity<MedicalRecord>().HasData(
            new MedicalRecord { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy - no issues detected", Treatment = "No treatment required. Recommended continued regular exercise and balanced diet.", Notes = "Weight is ideal for breed and age.", FollowUpDate = new DateOnly(2025, 7, 10), CreatedAt = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc) },
            new MedicalRecord { Id = 2, AppointmentId = 2, PetId = 2, VeterinarianId = 1, Diagnosis = "Healthy - vaccines due", Treatment = "Administered FVRCP and Rabies boosters.", Notes = "Cat was slightly anxious but cooperative.", CreatedAt = new DateTime(2025, 1, 15, 11, 0, 0, DateTimeKind.Utc) },
            new MedicalRecord { Id = 3, AppointmentId = 3, PetId = 3, VeterinarianId = 2, Diagnosis = "Minor sprain in right front leg", Treatment = "Prescribed anti-inflammatory medication and rest for 2 weeks. Applied cold compress.", Notes = "X-ray confirmed no fracture. Recommend limiting activity.", FollowUpDate = new DateOnly(2025, 2, 19), CreatedAt = new DateTime(2025, 2, 5, 15, 0, 0, DateTimeKind.Utc) },
            new MedicalRecord { Id = 4, AppointmentId = 4, PetId = 4, VeterinarianId = 1, Diagnosis = "Mild tartar buildup, otherwise healthy gums", Treatment = "Professional dental cleaning performed. Polished all teeth.", Notes = "Recommend dental treats to help maintain oral health.", CreatedAt = new DateTime(2025, 3, 1, 12, 0, 0, DateTimeKind.Utc) }
        );

        // --- Prescriptions ---
        modelBuilder.Entity<Prescription>().HasData(
            new Prescription { Id = 1, MedicalRecordId = 3, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = new DateOnly(2025, 2, 5), Instructions = "Give with food. Monitor for any GI issues.", CreatedAt = new DateTime(2025, 2, 5, 15, 0, 0, DateTimeKind.Utc) },
            new Prescription { Id = 2, MedicalRecordId = 3, MedicationName = "Glucosamine Supplement", Dosage = "500mg once daily", DurationDays = 30, StartDate = new DateOnly(2025, 2, 5), Instructions = "Can be mixed with food.", CreatedAt = new DateTime(2025, 2, 5, 15, 0, 0, DateTimeKind.Utc) },
            new Prescription { Id = 3, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 180, StartDate = new DateOnly(2025, 1, 10), Instructions = "Give on the same day each month.", CreatedAt = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc) },
            new Prescription { Id = 4, MedicalRecordId = 4, MedicationName = "Dental Enzyme Toothpaste", Dosage = "Apply once daily", DurationDays = 90, StartDate = new DateOnly(2025, 3, 1), Instructions = "Brush teeth gently. Use pet-specific toothbrush.", CreatedAt = new DateTime(2025, 3, 1, 12, 0, 0, DateTimeKind.Utc) },
            new Prescription { Id = 5, MedicalRecordId = 2, MedicationName = "Lysine Supplement", Dosage = "250mg once daily", DurationDays = 7, StartDate = new DateOnly(2025, 1, 15), Instructions = "Mix with wet food.", CreatedAt = new DateTime(2025, 1, 15, 11, 0, 0, DateTimeKind.Utc) }
        );

        // --- Vaccinations ---
        var today = new DateOnly(2025, 7, 1);
        modelBuilder.Entity<Vaccination>().HasData(
            // Current (not expired)
            new Vaccination { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = new DateOnly(2025, 1, 10), ExpirationDate = new DateOnly(2026, 1, 10), BatchNumber = "RAB-2025-001", AdministeredByVetId = 1, Notes = "3-year rabies vaccine", CreatedAt = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc) },
            new Vaccination { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = new DateOnly(2025, 1, 10), ExpirationDate = new DateOnly(2026, 1, 10), BatchNumber = "DHPP-2025-001", AdministeredByVetId = 1, CreatedAt = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc) },
            new Vaccination { Id = 3, PetId = 2, VaccineName = "FVRCP", DateAdministered = new DateOnly(2025, 1, 15), ExpirationDate = new DateOnly(2026, 1, 15), BatchNumber = "FVR-2025-001", AdministeredByVetId = 1, CreatedAt = new DateTime(2025, 1, 15, 11, 0, 0, DateTimeKind.Utc) },
            // Expiring soon (within 30 days of 2025-07-01)
            new Vaccination { Id = 4, PetId = 3, VaccineName = "Bordetella", DateAdministered = new DateOnly(2024, 7, 20), ExpirationDate = new DateOnly(2025, 7, 20), BatchNumber = "BOR-2024-003", AdministeredByVetId = 2, Notes = "Annual booster", CreatedAt = new DateTime(2024, 7, 20, 10, 0, 0, DateTimeKind.Utc) },
            // Expired
            new Vaccination { Id = 5, PetId = 4, VaccineName = "Rabies", DateAdministered = new DateOnly(2023, 6, 1), ExpirationDate = new DateOnly(2024, 6, 1), BatchNumber = "RAB-2023-010", AdministeredByVetId = 1, Notes = "Needs renewal", CreatedAt = new DateTime(2023, 6, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Vaccination { Id = 6, PetId = 5, VaccineName = "DHPP", DateAdministered = new DateOnly(2024, 11, 1), ExpirationDate = new DateOnly(2025, 11, 1), BatchNumber = "DHPP-2024-015", AdministeredByVetId = 2, CreatedAt = new DateTime(2024, 11, 1, 10, 0, 0, DateTimeKind.Utc) }
        );
    }
}
