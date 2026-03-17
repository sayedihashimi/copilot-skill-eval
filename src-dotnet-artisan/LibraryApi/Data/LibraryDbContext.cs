using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
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
        modelBuilder.Entity<Author>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            e.Property(a => a.LastName).IsRequired().HasMaxLength(100);
            e.Property(a => a.Biography).HasMaxLength(2000);
            e.Property(a => a.Country).HasMaxLength(100);
        });

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Description).HasMaxLength(500);
        });

        // Book
        modelBuilder.Entity<Book>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Title).IsRequired().HasMaxLength(300);
            e.Property(b => b.ISBN).IsRequired().HasMaxLength(20);
            e.HasIndex(b => b.ISBN).IsUnique();
            e.Property(b => b.Publisher).HasMaxLength(200);
            e.Property(b => b.Description).HasMaxLength(2000);
            e.Property(b => b.Language).HasMaxLength(50).HasDefaultValue("English");
        });

        // BookAuthor join
        modelBuilder.Entity<BookAuthor>(e =>
        {
            e.HasKey(ba => new { ba.BookId, ba.AuthorId });
            e.HasOne(ba => ba.Book).WithMany(b => b.BookAuthors).HasForeignKey(ba => ba.BookId);
            e.HasOne(ba => ba.Author).WithMany(a => a.BookAuthors).HasForeignKey(ba => ba.AuthorId);
        });

        // BookCategory join
        modelBuilder.Entity<BookCategory>(e =>
        {
            e.HasKey(bc => new { bc.BookId, bc.CategoryId });
            e.HasOne(bc => bc.Book).WithMany(b => b.BookCategories).HasForeignKey(bc => bc.BookId);
            e.HasOne(bc => bc.Category).WithMany(c => c.BookCategories).HasForeignKey(bc => bc.CategoryId);
        });

        // Patron
        modelBuilder.Entity<Patron>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            e.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            e.Property(p => p.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(p => p.Email).IsUnique();
            e.Property(p => p.Phone).HasMaxLength(20);
            e.Property(p => p.Address).HasMaxLength(500);
            e.Property(p => p.MembershipType).HasConversion<string>().HasMaxLength(20);
        });

        // Loan
        modelBuilder.Entity<Loan>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Book).WithMany(b => b.Loans).HasForeignKey(l => l.BookId);
            e.HasOne(l => l.Patron).WithMany(p => p.Loans).HasForeignKey(l => l.PatronId);
            e.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        });

        // Reservation
        modelBuilder.Entity<Reservation>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Book).WithMany(b => b.Reservations).HasForeignKey(r => r.BookId);
            e.HasOne(r => r.Patron).WithMany(p => p.Reservations).HasForeignKey(r => r.PatronId);
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        });

        // Fine
        modelBuilder.Entity<Fine>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasOne(f => f.Patron).WithMany(p => p.Fines).HasForeignKey(f => f.PatronId);
            e.HasOne(f => f.Loan).WithMany(l => l.Fines).HasForeignKey(f => f.LoanId);
            e.Property(f => f.Amount).HasColumnType("decimal(10,2)");
            e.Property(f => f.Reason).IsRequired().HasMaxLength(500);
            e.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
