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
            entity.HasMany(o => o.Pets)
                  .WithOne(p => p.Owner)
                  .HasForeignKey(p => p.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasIndex(p => p.MicrochipNumber).IsUnique().HasFilter("MicrochipNumber IS NOT NULL");
            entity.Property(p => p.Weight).HasColumnType("decimal(8,2)");
            entity.Property(p => p.Species).HasMaxLength(50);
        });

        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasIndex(v => v.Email).IsUnique();
            entity.HasIndex(v => v.LicenseNumber).IsUnique();
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasOne(a => a.Pet)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PetId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Veterinarian)
                  .WithMany(v => v.Appointments)
                  .HasForeignKey(a => a.VeterinarianId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(a => a.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasIndex(m => m.AppointmentId).IsUnique();

            entity.HasOne(m => m.Appointment)
                  .WithOne(a => a.MedicalRecord)
                  .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Pet)
                  .WithMany(p => p.MedicalRecords)
                  .HasForeignKey(m => m.PetId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Veterinarian)
                  .WithMany()
                  .HasForeignKey(m => m.VeterinarianId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasOne(p => p.MedicalRecord)
                  .WithMany(m => m.Prescriptions)
                  .HasForeignKey(p => p.MedicalRecordId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(p => p.IsActive);
        });

        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasOne(v => v.Pet)
                  .WithMany(p => p.Vaccinations)
                  .HasForeignKey(v => v.PetId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.AdministeredByVet)
                  .WithMany()
                  .HasForeignKey(v => v.AdministeredByVetId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(v => v.IsExpired);
            entity.Ignore(v => v.IsDueSoon);
        });
    }
}
