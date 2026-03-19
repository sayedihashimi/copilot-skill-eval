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
            entity.Property(o => o.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(o => o.LastName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasIndex(p => p.MicrochipNumber).IsUnique().HasFilter("[MicrochipNumber] IS NOT NULL");
            entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Species).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Weight).HasColumnType("decimal(8,2)");
            entity.HasOne(p => p.Owner)
                  .WithMany(o => o.Pets)
                  .HasForeignKey(p => p.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasIndex(v => v.Email).IsUnique();
            entity.HasIndex(v => v.LicenseNumber).IsUnique();
            entity.Property(v => v.LicenseNumber).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(a => a.Reason).HasMaxLength(500).IsRequired();
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(a => a.Pet)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PetId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Veterinarian)
                  .WithMany(v => v.Appointments)
                  .HasForeignKey(a => a.VeterinarianId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(a => new { a.VeterinarianId, a.AppointmentDate });
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasIndex(mr => mr.AppointmentId).IsUnique();
            entity.Property(mr => mr.Diagnosis).HasMaxLength(1000).IsRequired();
            entity.Property(mr => mr.Treatment).HasMaxLength(2000).IsRequired();
            entity.HasOne(mr => mr.Appointment)
                  .WithOne(a => a.MedicalRecord)
                  .HasForeignKey<MedicalRecord>(mr => mr.AppointmentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(mr => mr.Pet)
                  .WithMany(p => p.MedicalRecords)
                  .HasForeignKey(mr => mr.PetId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.Property(p => p.MedicationName).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Dosage).HasMaxLength(100).IsRequired();
            entity.Ignore(p => p.EndDate);
            entity.Ignore(p => p.IsActive);
            entity.HasOne(p => p.MedicalRecord)
                  .WithMany(mr => mr.Prescriptions)
                  .HasForeignKey(p => p.MedicalRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.Property(v => v.VaccineName).HasMaxLength(200).IsRequired();
            entity.Ignore(v => v.IsExpired);
            entity.Ignore(v => v.IsDueSoon);
            entity.HasOne(v => v.Pet)
                  .WithMany(p => p.Vaccinations)
                  .HasForeignKey(v => v.PetId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(v => v.AdministeredByVet)
                  .WithMany()
                  .HasForeignKey(v => v.AdministeredByVetId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
