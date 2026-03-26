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
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
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
            entity.Ignore(e => e.IsActive);
            entity.HasOne(e => e.MedicalRecord).WithMany(m => m.Prescriptions).HasForeignKey(e => e.MedicalRecordId).OnDelete(DeleteBehavior.Cascade);
        });

        // Vaccination configuration
        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VaccineName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsDueSoon);
            entity.HasOne(e => e.Pet).WithMany(p => p.Vaccinations).HasForeignKey(e => e.PetId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AdministeredByVet).WithMany().HasForeignKey(e => e.AdministeredByVetId).OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        // Seed Owners
        modelBuilder.Entity<Owner>().HasData(
            new Owner { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "San Francisco", State = "CA", ZipCode = "94102", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 4, FirstName = "James", LastName = "Wilson", Email = "james.wilson@email.com", Phone = "555-0104", Address = "321 Elm Boulevard", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 5, FirstName = "Lisa", LastName = "Thompson", Email = "lisa.thompson@email.com", Phone = "555-0105", Address = "654 Cedar Lane", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = now, UpdatedAt = now }
        );

        // Seed Pets
        modelBuilder.Entity<Pet>().HasData(
            new Pet { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m, Color = "Cream", MicrochipNumber = "MC-002-2019", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", IsActive = true, OwnerId = 2, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 6.2m, Color = "Tabby", MicrochipNumber = "MC-004-2022", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 12.3m, Color = "Tricolor", MicrochipNumber = "MC-005-2023", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2021, 11, 3), Weight = 0.09m, Color = "Yellow", MicrochipNumber = null, IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 7, Name = "Bella", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2018, 9, 20), Weight = 28.5m, Color = "Chocolate", MicrochipNumber = "MC-007-2018", IsActive = true, OwnerId = 5, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 8, Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 1), Weight = 1.8m, Color = "Brown", MicrochipNumber = null, IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now }
        );

        // Seed Veterinarians
        modelBuilder.Entity<Veterinarian>().HasData(
            new Veterinarian { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new Veterinarian { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new Veterinarian { Id = 3, FirstName = "Dr. Priya", LastName = "Patel", Email = "priya.patel@happypaws.com", Phone = "555-0203", Specialization = "Dentistry", LicenseNumber = "VET-2020-003", IsAvailable = true, HireDate = new DateOnly(2020, 9, 1) }
        );

        // Seed Appointments - past completed, future scheduled, one cancelled
        var pastDate1 = new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc);
        var pastDate2 = new DateTime(2026, 1, 8, 10, 30, 0, DateTimeKind.Utc);
        var pastDate3 = new DateTime(2026, 1, 10, 14, 0, 0, DateTimeKind.Utc);
        var pastDate4 = new DateTime(2026, 1, 12, 11, 0, 0, DateTimeKind.Utc);
        var futureDate1 = new DateTime(2026, 6, 15, 9, 0, 0, DateTimeKind.Utc);
        var futureDate2 = new DateTime(2026, 6, 16, 10, 0, 0, DateTimeKind.Utc);
        var futureDate3 = new DateTime(2026, 6, 17, 14, 30, 0, DateTimeKind.Utc);
        var futureDate4 = new DateTime(2026, 6, 18, 11, 0, 0, DateTimeKind.Utc);
        var futureDate5 = new DateTime(2026, 6, 19, 9, 30, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Appointment>().HasData(
            new Appointment { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = pastDate1, DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual wellness exam", Notes = "Routine checkup", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 2, PetId = 3, VeterinarianId = 2, AppointmentDate = pastDate2, DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on right front leg", Notes = "X-ray recommended", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 3, PetId = 2, VeterinarianId = 3, AppointmentDate = pastDate3, DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", Notes = "Plaque buildup noticed", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 4, PetId = 7, VeterinarianId = 1, AppointmentDate = pastDate4, DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Vaccination booster", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 5, PetId = 5, VeterinarianId = 1, AppointmentDate = futureDate1, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Puppy wellness check", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 6, PetId = 4, VeterinarianId = 2, AppointmentDate = futureDate2, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Skin irritation follow-up", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 7, PetId = 1, VeterinarianId = 1, AppointmentDate = futureDate3, DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Hip evaluation", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 8, PetId = 6, VeterinarianId = 3, AppointmentDate = futureDate4, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Wing feather trim and checkup", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 9, PetId = 8, VeterinarianId = 1, AppointmentDate = futureDate5, DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Routine checkup", CancellationReason = "Owner had scheduling conflict", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 10, PetId = 3, VeterinarianId = 2, AppointmentDate = new DateTime(2026, 1, 6, 15, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Follow-up on ear infection", CreatedAt = now, UpdatedAt = now }
        );

        // Seed Medical Records (for completed appointments)
        modelBuilder.Entity<MedicalRecord>().HasData(
            new MedicalRecord { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy, no concerns", Treatment = "Routine wellness exam completed. All vitals normal.", Notes = "Weight stable, coat healthy", FollowUpDate = null, CreatedAt = now },
            new MedicalRecord { Id = 2, AppointmentId = 2, PetId = 3, VeterinarianId = 2, Diagnosis = "Mild sprain in right front leg", Treatment = "Rest for 2 weeks, anti-inflammatory medication prescribed", Notes = "X-ray showed no fracture", FollowUpDate = new DateOnly(2026, 1, 22), CreatedAt = now },
            new MedicalRecord { Id = 3, AppointmentId = 3, PetId = 2, VeterinarianId = 3, Diagnosis = "Stage 1 periodontal disease", Treatment = "Professional dental cleaning performed under anesthesia", Notes = "Two teeth with early decay, monitoring needed", FollowUpDate = new DateOnly(2026, 4, 10), CreatedAt = now },
            new MedicalRecord { Id = 4, AppointmentId = 4, PetId = 7, VeterinarianId = 1, Diagnosis = "Healthy, vaccination administered", Treatment = "DHPP booster vaccine administered", Notes = "No adverse reactions observed", CreatedAt = now }
        );

        // Seed Prescriptions
        modelBuilder.Entity<Prescription>().HasData(
            new Prescription { Id = 1, MedicalRecordId = 2, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = new DateOnly(2026, 1, 8), EndDate = new DateOnly(2026, 1, 22), Instructions = "Give with food. Stop if vomiting occurs.", CreatedAt = now },
            new Prescription { Id = 2, MedicalRecordId = 2, MedicationName = "Gabapentin", Dosage = "100mg once daily", DurationDays = 7, StartDate = new DateOnly(2026, 1, 8), EndDate = new DateOnly(2026, 1, 15), Instructions = "For pain management. May cause drowsiness.", CreatedAt = now },
            new Prescription { Id = 3, MedicalRecordId = 3, MedicationName = "Clindamycin", Dosage = "75mg twice daily", DurationDays = 10, StartDate = new DateOnly(2026, 1, 10), EndDate = new DateOnly(2026, 1, 20), Instructions = "Complete full course of antibiotics.", CreatedAt = now },
            new Prescription { Id = 4, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 365, StartDate = new DateOnly(2026, 1, 5), EndDate = new DateOnly(2027, 1, 5), Instructions = "Monthly heartworm prevention. Give on the same day each month.", CreatedAt = now },
            new Prescription { Id = 5, MedicalRecordId = 4, MedicationName = "Benadryl", Dosage = "25mg as needed", DurationDays = 3, StartDate = new DateOnly(2026, 1, 12), EndDate = new DateOnly(2026, 1, 15), Instructions = "Only if mild swelling at injection site occurs.", CreatedAt = now }
        );

        // Seed Vaccinations
        modelBuilder.Entity<Vaccination>().HasData(
            new Vaccination { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = new DateOnly(2025, 3, 15), ExpirationDate = new DateOnly(2026, 3, 15), BatchNumber = "RAB-2025-001", AdministeredByVetId = 1, Notes = "3-year vaccine", CreatedAt = now },
            new Vaccination { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = new DateOnly(2026, 1, 5), ExpirationDate = new DateOnly(2027, 1, 5), BatchNumber = "DHPP-2026-010", AdministeredByVetId = 1, CreatedAt = now },
            new Vaccination { Id = 3, PetId = 3, VaccineName = "Rabies", DateAdministered = new DateOnly(2025, 6, 10), ExpirationDate = new DateOnly(2026, 6, 10), BatchNumber = "RAB-2025-045", AdministeredByVetId = 2, CreatedAt = now },
            new Vaccination { Id = 4, PetId = 2, VaccineName = "FVRCP", DateAdministered = new DateOnly(2025, 1, 20), ExpirationDate = new DateOnly(2026, 1, 20), BatchNumber = "FVRCP-2025-012", AdministeredByVetId = 1, Notes = "Annual feline vaccine", CreatedAt = now },
            new Vaccination { Id = 5, PetId = 7, VaccineName = "DHPP", DateAdministered = new DateOnly(2026, 1, 12), ExpirationDate = new DateOnly(2027, 1, 12), BatchNumber = "DHPP-2026-015", AdministeredByVetId = 1, CreatedAt = now },
            new Vaccination { Id = 6, PetId = 5, VaccineName = "Bordetella", DateAdministered = new DateOnly(2025, 8, 1), ExpirationDate = new DateOnly(2026, 2, 1), BatchNumber = "BORD-2025-033", AdministeredByVetId = 3, Notes = "Kennel cough vaccine", CreatedAt = now }
        );
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Owner owner)
            {
                owner.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) owner.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Pet pet)
            {
                pet.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) pet.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Appointment appointment)
            {
                appointment.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) appointment.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is MedicalRecord record && entry.State == EntityState.Added)
            {
                record.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Prescription prescription && entry.State == EntityState.Added)
            {
                prescription.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Vaccination vaccination && entry.State == EntityState.Added)
            {
                vaccination.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
