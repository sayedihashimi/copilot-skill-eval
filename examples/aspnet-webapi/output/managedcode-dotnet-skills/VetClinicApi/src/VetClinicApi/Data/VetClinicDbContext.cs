using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public class VetClinicDbContext(DbContextOptions<VetClinicDbContext> options) : DbContext(options)
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

        // Owner
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.State).HasMaxLength(2);
        });

        // Pet
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasIndex(e => e.MicrochipNumber).IsUnique().HasFilter("\"MicrochipNumber\" IS NOT NULL");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Species).IsRequired();
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.HasOne(e => e.Owner)
                  .WithMany(o => o.Pets)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Veterinarian
        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LicenseNumber).IsRequired();
        });

        // Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Reason).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.HasOne(e => e.Pet)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(e => e.PetId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Veterinarian)
                  .WithMany(v => v.Appointments)
                  .HasForeignKey(e => e.VeterinarianId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.AppointmentDate);
            entity.HasIndex(e => e.Status);
        });

        // MedicalRecord
        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.Property(e => e.Diagnosis).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Treatment).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.HasOne(e => e.Appointment)
                  .WithOne(a => a.MedicalRecord)
                  .HasForeignKey<MedicalRecord>(e => e.AppointmentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Pet)
                  .WithMany(p => p.MedicalRecords)
                  .HasForeignKey(e => e.PetId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Veterinarian)
                  .WithMany()
                  .HasForeignKey(e => e.VeterinarianId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Prescription
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.Property(e => e.MedicationName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Dosage).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Instructions).HasMaxLength(500);
            entity.Ignore(e => e.IsActive); // computed in app, not stored
            entity.HasOne(e => e.MedicalRecord)
                  .WithMany(m => m.Prescriptions)
                  .HasForeignKey(e => e.MedicalRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Vaccination
        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.Property(e => e.VaccineName).HasMaxLength(200).IsRequired();
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
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
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
        }
    }
}
