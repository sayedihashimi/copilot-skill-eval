using Microsoft.EntityFrameworkCore;
using KeystoneProperties.Models;

namespace KeystoneProperties.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<Inspection> Inspections => Set<Inspection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Property
        modelBuilder.Entity<Property>(e =>
        {
            e.HasMany(p => p.Units).WithOne(u => u.Property).HasForeignKey(u => u.PropertyId);
        });

        // Unit - unique constraint on (PropertyId, UnitNumber)
        modelBuilder.Entity<Unit>(e =>
        {
            e.HasIndex(u => new { u.PropertyId, u.UnitNumber }).IsUnique();
            e.HasMany(u => u.Leases).WithOne(l => l.Unit).HasForeignKey(l => l.UnitId);
            e.HasMany(u => u.MaintenanceRequests).WithOne(m => m.Unit).HasForeignKey(m => m.UnitId);
            e.HasMany(u => u.Inspections).WithOne(i => i.Unit).HasForeignKey(i => i.UnitId);
        });

        // Tenant
        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasIndex(t => t.Email).IsUnique();
            e.HasMany(t => t.Leases).WithOne(l => l.Tenant).HasForeignKey(l => l.TenantId);
            e.HasMany(t => t.MaintenanceRequests).WithOne(m => m.Tenant).HasForeignKey(m => m.TenantId);
        });

        // Lease - self-referencing
        modelBuilder.Entity<Lease>(e =>
        {
            e.HasOne(l => l.RenewalOfLease).WithMany().HasForeignKey(l => l.RenewalOfLeaseId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(l => l.Payments).WithOne(p => p.Lease).HasForeignKey(p => p.LeaseId);
            e.HasMany(l => l.Inspections).WithOne(i => i.Lease).HasForeignKey(i => i.LeaseId);
        });

        // Decimal precision
        modelBuilder.Entity<Unit>(e =>
        {
            e.Property(u => u.MonthlyRent).HasColumnType("decimal(18,2)");
            e.Property(u => u.DepositAmount).HasColumnType("decimal(18,2)");
            e.Property(u => u.Bathrooms).HasColumnType("decimal(3,1)");
        });

        modelBuilder.Entity<Lease>(e =>
        {
            e.Property(l => l.MonthlyRentAmount).HasColumnType("decimal(18,2)");
            e.Property(l => l.DepositAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<MaintenanceRequest>(e =>
        {
            e.Property(m => m.EstimatedCost).HasColumnType("decimal(18,2)");
            e.Property(m => m.ActualCost).HasColumnType("decimal(18,2)");
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
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Property p) p.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Unit u) u.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Tenant t) t.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Lease l) l.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is MaintenanceRequest m) m.UpdatedAt = DateTime.UtcNow;
        }
    }
}
