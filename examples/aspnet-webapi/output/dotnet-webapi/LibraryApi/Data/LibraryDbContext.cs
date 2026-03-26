using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options)
    : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<BookCategory> BookCategories => Set<BookCategory>();
    public DbSet<Patron> Patrons => Set<Patron>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Fine> Fines => Set<Fine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Author
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.LastName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Biography).HasMaxLength(2000);
            entity.Property(a => a.Country).HasMaxLength(100);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Name).IsUnique();
            entity.Property(c => c.Description).HasMaxLength(500);
        });

        // Book
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(300);
            entity.Property(b => b.ISBN).IsRequired();
            entity.HasIndex(b => b.ISBN).IsUnique();
            entity.Property(b => b.Publisher).HasMaxLength(200);
            entity.Property(b => b.Description).HasMaxLength(2000);
            entity.Property(b => b.Language).HasDefaultValue("English");
        });

        // BookAuthor (many-to-many join)
        modelBuilder.Entity<BookAuthor>(entity =>
        {
            entity.HasKey(ba => new { ba.BookId, ba.AuthorId });
            entity.HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BookCategory (many-to-many join)
        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.HasKey(bc => new { bc.BookId, bc.CategoryId });
            entity.HasOne(bc => bc.Book)
                .WithMany(b => b.BookCategories)
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(bc => bc.Category)
                .WithMany(c => c.BookCategories)
                .HasForeignKey(bc => bc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Patron
        modelBuilder.Entity<Patron>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Email).IsRequired();
            entity.HasIndex(p => p.Email).IsUnique();
            entity.Property(p => p.MembershipType).HasConversion<string>();
        });

        // Loan
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Status).HasConversion<string>();
            entity.HasOne(l => l.Book)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(l => l.Patron)
                .WithMany(p => p.Loans)
                .HasForeignKey(l => l.PatronId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Reservation
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Status).HasConversion<string>();
            entity.HasOne(r => r.Book)
                .WithMany(b => b.Reservations)
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Patron)
                .WithMany(p => p.Reservations)
                .HasForeignKey(r => r.PatronId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Fine
        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Amount).HasColumnType("decimal(10,2)");
            entity.Property(f => f.Status).HasConversion<string>();
            entity.Property(f => f.Reason).IsRequired();
            entity.HasOne(f => f.Patron)
                .WithMany(p => p.Fines)
                .HasForeignKey(f => f.PatronId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(f => f.Loan)
                .WithMany(l => l.Fines)
                .HasForeignKey(f => f.LoanId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
