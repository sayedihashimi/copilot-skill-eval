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

        // Owner
        modelBuilder.Entity<Owner>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.FirstName).IsRequired().HasMaxLength(100);
            e.Property(o => o.LastName).IsRequired().HasMaxLength(100);
            e.Property(o => o.Email).IsRequired();
            e.HasIndex(o => o.Email).IsUnique();
            e.Property(o => o.Phone).IsRequired();
            e.Property(o => o.State).HasMaxLength(2);
            e.HasMany(o => o.Pets).WithOne(p => p.Owner).HasForeignKey(p => p.OwnerId);
        });

        // Pet
        modelBuilder.Entity<Pet>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Species).IsRequired();
            e.Property(p => p.Breed).HasMaxLength(100);
            e.HasIndex(p => p.MicrochipNumber).IsUnique().HasFilter("[MicrochipNumber] IS NOT NULL");
            e.Property(p => p.IsActive).HasDefaultValue(true);
            e.HasMany(p => p.Appointments).WithOne(a => a.Pet).HasForeignKey(a => a.PetId);
            e.HasMany(p => p.MedicalRecords).WithOne(m => m.Pet).HasForeignKey(m => m.PetId).OnDelete(DeleteBehavior.NoAction);
            e.HasMany(p => p.Vaccinations).WithOne(v => v.Pet).HasForeignKey(v => v.PetId);
        });

        // Veterinarian
        modelBuilder.Entity<Veterinarian>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.FirstName).IsRequired().HasMaxLength(100);
            e.Property(v => v.LastName).IsRequired().HasMaxLength(100);
            e.Property(v => v.Email).IsRequired();
            e.HasIndex(v => v.Email).IsUnique();
            e.Property(v => v.LicenseNumber).IsRequired();
            e.HasIndex(v => v.LicenseNumber).IsUnique();
            e.HasMany(v => v.Appointments).WithOne(a => a.Veterinarian).HasForeignKey(a => a.VeterinarianId);
        });

        // Appointment
        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Status).HasConversion<string>();
            e.Property(a => a.Reason).IsRequired().HasMaxLength(500);
            e.Property(a => a.Notes).HasMaxLength(2000);
            e.Property(a => a.DurationMinutes).HasDefaultValue(30);
            e.HasOne(a => a.MedicalRecord).WithOne(m => m.Appointment).HasForeignKey<MedicalRecord>(m => m.AppointmentId);
        });

        // MedicalRecord
        modelBuilder.Entity<MedicalRecord>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.AppointmentId).IsUnique();
            e.Property(m => m.Diagnosis).IsRequired().HasMaxLength(1000);
            e.Property(m => m.Treatment).IsRequired().HasMaxLength(2000);
            e.Property(m => m.Notes).HasMaxLength(2000);
            e.HasOne(m => m.Veterinarian).WithMany().HasForeignKey(m => m.VeterinarianId).OnDelete(DeleteBehavior.NoAction);
            e.HasMany(m => m.Prescriptions).WithOne(p => p.MedicalRecord).HasForeignKey(p => p.MedicalRecordId);
        });

        // Prescription
        modelBuilder.Entity<Prescription>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.MedicationName).IsRequired().HasMaxLength(200);
            e.Property(p => p.Dosage).IsRequired().HasMaxLength(100);
            e.Property(p => p.Instructions).HasMaxLength(500);
            e.Ignore(p => p.IsActive);
        });

        // Vaccination
        modelBuilder.Entity<Vaccination>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.VaccineName).IsRequired().HasMaxLength(200);
            e.Property(v => v.Notes).HasMaxLength(500);
            e.HasOne(v => v.Pet).WithMany(p => p.Vaccinations).HasForeignKey(v => v.PetId);
            e.HasOne(v => v.AdministeredByVet).WithMany().HasForeignKey(v => v.AdministeredByVetId).OnDelete(DeleteBehavior.NoAction);
            e.Ignore(v => v.IsExpired);
            e.Ignore(v => v.IsDueSoon);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

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
            else if (entry.Entity is Appointment appt)
            {
                appt.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) appt.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is MedicalRecord mr)
            {
                if (entry.State == EntityState.Added) mr.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Prescription rx)
            {
                if (entry.State == EntityState.Added) rx.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Vaccination vax)
            {
                if (entry.State == EntityState.Added) vax.CreatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
