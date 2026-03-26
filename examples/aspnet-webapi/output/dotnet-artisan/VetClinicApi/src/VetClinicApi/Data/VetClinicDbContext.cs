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
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasIndex(o => o.Email).IsUnique();
            entity.HasMany(o => o.Pets).WithOne(p => p.Owner).HasForeignKey(p => p.OwnerId);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasIndex(p => p.MicrochipNumber).IsUnique().HasFilter("MicrochipNumber IS NOT NULL");
            entity.HasMany(p => p.Appointments).WithOne(a => a.Pet).HasForeignKey(a => a.PetId);
            entity.HasMany(p => p.MedicalRecords).WithOne(m => m.Pet).HasForeignKey(m => m.PetId).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(p => p.Vaccinations).WithOne(v => v.Pet).HasForeignKey(v => v.PetId);
        });

        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasIndex(v => v.Email).IsUnique();
            entity.HasIndex(v => v.LicenseNumber).IsUnique();
            entity.HasMany(v => v.Appointments).WithOne(a => a.Veterinarian).HasForeignKey(a => a.VeterinarianId);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasOne(a => a.MedicalRecord).WithOne(m => m.Appointment).HasForeignKey<MedicalRecord>(m => m.AppointmentId);
            entity.Property(a => a.Status).HasConversion<string>();
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasIndex(m => m.AppointmentId).IsUnique();
            entity.HasOne(m => m.Veterinarian).WithMany().HasForeignKey(m => m.VeterinarianId).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(m => m.Prescriptions).WithOne(p => p.MedicalRecord).HasForeignKey(p => p.MedicalRecordId);
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.Ignore(p => p.IsActive);
        });

        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasOne(v => v.AdministeredByVet).WithMany().HasForeignKey(v => v.AdministeredByVetId).OnDelete(DeleteBehavior.NoAction);
            entity.Ignore(v => v.IsExpired);
            entity.Ignore(v => v.IsDueSoon);
        });
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
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is Owner o) { o.CreatedAt = now; o.UpdatedAt = now; }
                else if (entry.Entity is Pet p) { p.CreatedAt = now; p.UpdatedAt = now; }
                else if (entry.Entity is Appointment a) { a.CreatedAt = now; a.UpdatedAt = now; }
                else if (entry.Entity is MedicalRecord m) { m.CreatedAt = now; }
                else if (entry.Entity is Prescription pr) { pr.CreatedAt = now; }
                else if (entry.Entity is Vaccination v) { v.CreatedAt = now; }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is Owner o) { o.UpdatedAt = now; }
                else if (entry.Entity is Pet p) { p.UpdatedAt = now; }
                else if (entry.Entity is Appointment a) { a.UpdatedAt = now; }
            }
        }
    }
}
