using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public sealed class VetClinicDbContext(DbContextOptions<VetClinicDbContext> options) : DbContext(options)
{
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

        // Owner configuration
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).IsRequired();
            entity.Property(e => e.State).HasMaxLength(2);
            entity.HasMany(e => e.Pets).WithOne(p => p.Owner).HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        // Pet configuration
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Species).IsRequired();
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.MicrochipNumber).IsUnique().HasFilter("[MicrochipNumber] IS NOT NULL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // Veterinarian configuration
        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).IsRequired();
            entity.Property(e => e.LicenseNumber).IsRequired();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
        });

        // Appointment configuration
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>().IsRequired();
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.DurationMinutes).HasDefaultValue(30);
            entity.HasOne(e => e.Pet).WithMany(p => p.Appointments).HasForeignKey(e => e.PetId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Veterinarian).WithMany(v => v.Appointments).HasForeignKey(e => e.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.MedicalRecord).WithOne(m => m.Appointment).HasForeignKey<MedicalRecord>(m => m.AppointmentId);
        });

        // MedicalRecord configuration
        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.Property(e => e.Diagnosis).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Treatment).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.HasOne(e => e.Pet).WithMany(p => p.MedicalRecords).HasForeignKey(e => e.PetId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Veterinarian).WithMany().HasForeignKey(e => e.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
        });

        // Prescription configuration
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MedicationName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Dosage).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(500);
            entity.HasOne(e => e.MedicalRecord).WithMany(m => m.Prescriptions).HasForeignKey(e => e.MedicalRecordId).OnDelete(DeleteBehavior.Cascade);
            entity.Ignore(e => e.IsActive);
        });

        // Vaccination configuration
        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VaccineName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasOne(e => e.Pet).WithMany(p => p.Vaccinations).HasForeignKey(e => e.PetId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AdministeredByVet).WithMany().HasForeignKey(e => e.AdministeredByVetId).OnDelete(DeleteBehavior.Restrict);
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsDueSoon);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        // Owners
        modelBuilder.Entity<Owner>().HasData(
            new Owner { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Maple Street", City = "Springfield", State = "IL", ZipCode = "62701", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Oak Avenue", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 4, FirstName = "James", LastName = "Thompson", Email = "james.thompson@email.com", Phone = "555-0104", Address = "321 Elm Lane", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 5, FirstName = "Lisa", LastName = "Patel", Email = "lisa.patel@email.com", Phone = "555-0105", Address = "654 Birch Court", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = now, UpdatedAt = now }
        );

        // Pets
        modelBuilder.Entity<Pet>().HasData(
            new Pet { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 6.8m, Color = "Tabby", MicrochipNumber = "MC-002-2019", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.2m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", IsActive = true, OwnerId = 2, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 4, Name = "Luna", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2022, 5, 3), Weight = 4.1m, Color = "Seal Point", MicrochipNumber = "MC-004-2022", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2020, 11, 28), Weight = 12.3m, Color = "Tricolor", MicrochipNumber = "MC-005-2020", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 6, Name = "Kiwi", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2021, 8, 14), Weight = 0.09m, Color = "Grey and Yellow", MicrochipNumber = null, IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 7, Name = "Daisy", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 2, 1), Weight = 1.8m, Color = "White", MicrochipNumber = "MC-007-2023", IsActive = true, OwnerId = 5, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 8, Name = "Rocky", Species = "Dog", Breed = "Bulldog", DateOfBirth = new DateOnly(2018, 6, 20), Weight = 25.0m, Color = "Brindle", MicrochipNumber = "MC-008-2018", IsActive = false, OwnerId = 4, CreatedAt = now, UpdatedAt = now }
        );

        // Veterinarians
        modelBuilder.Entity<Veterinarian>().HasData(
            new Veterinarian { Id = 1, FirstName = "Dr. Amanda", LastName = "Wilson", Email = "amanda.wilson@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new Veterinarian { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new Veterinarian { Id = 3, FirstName = "Dr. Maria", LastName = "Garcia", Email = "maria.garcia@happypaws.com", Phone = "555-0203", Specialization = "Dentistry", LicenseNumber = "VET-2020-003", IsAvailable = true, HireDate = new DateOnly(2020, 9, 1) }
        );

        // Appointments (mix of statuses, some in past, some future)
        var futureDate = new DateTime(2025, 7, 20, 9, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Appointment>().HasData(
            new Appointment { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 10, 9, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual wellness checkup", Notes = "All vitals normal", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 2, PetId = 2, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Vaccination update", Notes = "Administered FVRCP booster", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 3, PetId = 3, VeterinarianId = 2, AppointmentDate = new DateTime(2025, 1, 12, 14, 0, 0, DateTimeKind.Utc), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on right front leg", Notes = "X-rays taken, minor sprain diagnosed", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 4, PetId = 4, VeterinarianId = 3, AppointmentDate = new DateTime(2025, 1, 13, 11, 0, 0, DateTimeKind.Utc), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", Notes = "Teeth cleaned, no extractions needed", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 5, PetId = 5, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 14, 9, 30, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Routine checkup", CancellationReason = "Owner rescheduled due to work conflict", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 6, PetId = 1, VeterinarianId = 1, AppointmentDate = futureDate, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up wellness check", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 7, PetId = 3, VeterinarianId = 2, AppointmentDate = futureDate.AddHours(2), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Post-sprain follow-up", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 8, PetId = 5, VeterinarianId = 1, AppointmentDate = futureDate.AddDays(1), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Rescheduled routine checkup", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 9, PetId = 6, VeterinarianId = 3, AppointmentDate = futureDate.AddDays(2), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Wing feather assessment", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 10, PetId = 7, VeterinarianId = 1, AppointmentDate = futureDate.AddDays(3), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Nail trim and general checkup", CreatedAt = now, UpdatedAt = now }
        );

        // Medical Records (linked to completed appointments)
        modelBuilder.Entity<MedicalRecord>().HasData(
            new MedicalRecord { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy - no issues found", Treatment = "No treatment required. Recommended continued current diet and exercise.", Notes = "Weight is ideal. Coat in excellent condition.", FollowUpDate = new DateOnly(2025, 7, 10), CreatedAt = now },
            new MedicalRecord { Id = 2, AppointmentId = 2, PetId = 2, VeterinarianId = 1, Diagnosis = "Due for FVRCP booster", Treatment = "Administered FVRCP vaccine booster. Monitored for 15 minutes post-injection.", Notes = "No adverse reactions observed.", CreatedAt = now },
            new MedicalRecord { Id = 3, AppointmentId = 3, PetId = 3, VeterinarianId = 2, Diagnosis = "Grade 1 sprain of right carpus", Treatment = "Prescribed anti-inflammatory medication and rest for 2 weeks. Apply cold compress twice daily.", Notes = "X-ray showed no fractures. Soft tissue swelling present.", FollowUpDate = new DateOnly(2025, 1, 26), CreatedAt = now },
            new MedicalRecord { Id = 4, AppointmentId = 4, PetId = 4, VeterinarianId = 3, Diagnosis = "Mild tartar buildup, otherwise healthy gums", Treatment = "Professional dental cleaning performed under light sedation. All teeth intact.", Notes = "Recommended dental treats and regular brushing.", CreatedAt = now }
        );

        // Prescriptions
        modelBuilder.Entity<Prescription>().HasData(
            new Prescription { Id = 1, MedicalRecordId = 3, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = new DateOnly(2025, 1, 12), EndDate = new DateOnly(2025, 1, 26), Instructions = "Give with food. Monitor for vomiting or diarrhea.", CreatedAt = now },
            new Prescription { Id = 2, MedicalRecordId = 3, MedicationName = "Gabapentin", Dosage = "100mg once daily at bedtime", DurationDays = 10, StartDate = new DateOnly(2025, 1, 12), EndDate = new DateOnly(2025, 1, 22), Instructions = "For pain management. May cause drowsiness.", CreatedAt = now },
            new Prescription { Id = 3, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 365, StartDate = new DateOnly(2025, 1, 10), EndDate = new DateOnly(2026, 1, 10), Instructions = "Monthly heartworm prevention. Give on the same day each month.", CreatedAt = now },
            new Prescription { Id = 4, MedicalRecordId = 4, MedicationName = "Clindamycin", Dosage = "75mg twice daily", DurationDays = 7, StartDate = new DateOnly(2025, 1, 13), EndDate = new DateOnly(2025, 1, 20), Instructions = "Antibiotic prophylaxis post dental cleaning. Complete full course.", CreatedAt = now },
            new Prescription { Id = 5, MedicalRecordId = 2, MedicationName = "L-Lysine", Dosage = "250mg once daily", DurationDays = 30, StartDate = new DateOnly(2025, 1, 10), EndDate = new DateOnly(2025, 2, 9), Instructions = "Immune support supplement. Mix with wet food.", CreatedAt = now }
        );

        // Vaccinations
        modelBuilder.Entity<Vaccination>().HasData(
            new Vaccination { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = new DateOnly(2024, 3, 15), ExpirationDate = new DateOnly(2027, 3, 15), BatchNumber = "RAB-2024-A1", AdministeredByVetId = 1, Notes = "3-year rabies vaccine", CreatedAt = now },
            new Vaccination { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = new DateOnly(2024, 3, 15), ExpirationDate = new DateOnly(2025, 3, 15), BatchNumber = "DHPP-2024-B2", AdministeredByVetId = 1, Notes = "Annual booster", CreatedAt = now },
            new Vaccination { Id = 3, PetId = 2, VaccineName = "FVRCP", DateAdministered = new DateOnly(2025, 1, 10), ExpirationDate = new DateOnly(2026, 1, 10), BatchNumber = "FVR-2025-C3", AdministeredByVetId = 1, CreatedAt = now },
            new Vaccination { Id = 4, PetId = 3, VaccineName = "Rabies", DateAdministered = new DateOnly(2024, 1, 10), ExpirationDate = new DateOnly(2025, 1, 10), BatchNumber = "RAB-2024-D4", AdministeredByVetId = 2, Notes = "Due for renewal", CreatedAt = now },
            new Vaccination { Id = 5, PetId = 5, VaccineName = "Bordetella", DateAdministered = new DateOnly(2024, 11, 28), ExpirationDate = new DateOnly(2025, 11, 28), BatchNumber = "BOR-2024-E5", AdministeredByVetId = 1, CreatedAt = now },
            new Vaccination { Id = 6, PetId = 4, VaccineName = "Rabies", DateAdministered = new DateOnly(2023, 5, 3), ExpirationDate = new DateOnly(2024, 5, 3), BatchNumber = "RAB-2023-F6", AdministeredByVetId = 3, Notes = "Expired - needs renewal", CreatedAt = now }
        );
    }
}
