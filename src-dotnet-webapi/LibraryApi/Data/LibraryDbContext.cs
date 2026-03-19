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
            e.Property(b => b.ISBN).IsRequired();
            e.HasIndex(b => b.ISBN).IsUnique();
            e.Property(b => b.Publisher).HasMaxLength(200);
            e.Property(b => b.Description).HasMaxLength(2000);
            e.Property(b => b.Language).HasMaxLength(100).HasDefaultValue("English");
        });

        // BookAuthor (many-to-many)
        modelBuilder.Entity<BookAuthor>(e =>
        {
            e.HasKey(ba => new { ba.BookId, ba.AuthorId });
            e.HasOne(ba => ba.Book).WithMany(b => b.BookAuthors).HasForeignKey(ba => ba.BookId);
            e.HasOne(ba => ba.Author).WithMany(a => a.BookAuthors).HasForeignKey(ba => ba.AuthorId);
        });

        // BookCategory (many-to-many)
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
            e.Property(p => p.Email).IsRequired();
            e.HasIndex(p => p.Email).IsUnique();
            e.Property(p => p.MembershipType).HasConversion<string>();
            e.Property(p => p.IsActive).HasDefaultValue(true);
        });

        // Loan
        modelBuilder.Entity<Loan>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Book).WithMany(b => b.Loans).HasForeignKey(l => l.BookId);
            e.HasOne(l => l.Patron).WithMany(p => p.Loans).HasForeignKey(l => l.PatronId);
            e.Property(l => l.Status).HasConversion<string>();
        });

        // Reservation
        modelBuilder.Entity<Reservation>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Book).WithMany(b => b.Reservations).HasForeignKey(r => r.BookId);
            e.HasOne(r => r.Patron).WithMany(p => p.Reservations).HasForeignKey(r => r.PatronId);
            e.Property(r => r.Status).HasConversion<string>();
        });

        // Fine
        modelBuilder.Entity<Fine>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasOne(f => f.Patron).WithMany(p => p.Fines).HasForeignKey(f => f.PatronId);
            e.HasOne(f => f.Loan).WithMany(l => l.Fines).HasForeignKey(f => f.LoanId);
            e.Property(f => f.Amount).HasPrecision(10, 2);
            e.Property(f => f.Status).HasConversion<string>();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        // Authors
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her six major novels.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = now },
            new Author { Id = 2, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, journalist and critic.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom", CreatedAt = now },
            new Author { Id = 3, FirstName = "Gabriel", LastName = "Garcia Marquez", Biography = "Colombian novelist and Nobel Prize winner.", BirthDate = new DateOnly(1927, 3, 6), Country = "Colombia", CreatedAt = now },
            new Author { Id = 4, FirstName = "Toni", LastName = "Morrison", Biography = "American novelist and Nobel Prize in Literature laureate.", BirthDate = new DateOnly(1931, 2, 18), Country = "United States", CreatedAt = now },
            new Author { Id = 5, FirstName = "Haruki", LastName = "Murakami", Biography = "Japanese writer known for surreal and dreamlike fiction.", BirthDate = new DateOnly(1949, 1, 12), Country = "Japan", CreatedAt = now },
            new Author { Id = 6, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, prolific author of science fiction.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = now }
        );

        // Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Fiction", Description = "Literary and general fiction works" },
            new Category { Id = 2, Name = "Science Fiction", Description = "Speculative fiction dealing with futuristic concepts" },
            new Category { Id = 3, Name = "History", Description = "Non-fiction works about historical events and periods" },
            new Category { Id = 4, Name = "Science", Description = "Books covering scientific topics and discoveries" },
            new Category { Id = 5, Name = "Biography", Description = "Accounts of a person's life written by someone else" },
            new Category { Id = 6, Name = "Classic Literature", Description = "Timeless works of literary significance" }
        );

        // Books (12+)
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Pride and Prejudice", ISBN = "978-0-14-028329-7", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 2, Title = "1984", ISBN = "978-0-45-152493-5", Publisher = "Signet Classic", PublicationYear = 1949, Description = "A dystopian social science fiction novel.", PageCount = 328, Language = "English", TotalCopies = 4, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 3, Title = "One Hundred Years of Solitude", ISBN = "978-0-06-088328-7", Publisher = "Harper Perennial", PublicationYear = 1967, Description = "The multi-generational story of the Buendia family.", PageCount = 417, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 4, Title = "Beloved", ISBN = "978-1-40-003341-6", Publisher = "Vintage", PublicationYear = 1987, Description = "A novel inspired by the story of Margaret Garner.", PageCount = 324, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 5, Title = "Norwegian Wood", ISBN = "978-0-37-571894-0", Publisher = "Vintage International", PublicationYear = 1987, Description = "A nostalgic story of loss and sexuality.", PageCount = 296, Language = "English", TotalCopies = 1, AvailableCopies = 0, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 6, Title = "Foundation", ISBN = "978-0-55-338257-3", Publisher = "Bantam Spectra", PublicationYear = 1951, Description = "The first novel in Isaac Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 7, Title = "Animal Farm", ISBN = "978-0-45-152634-2", Publisher = "Signet Classic", PublicationYear = 1945, Description = "An allegorical novella about Soviet totalitarianism.", PageCount = 112, Language = "English", TotalCopies = 5, AvailableCopies = 4, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 8, Title = "Kafka on the Shore", ISBN = "978-1-40-000290-9", Publisher = "Vintage International", PublicationYear = 2002, Description = "A metaphysical novel by Haruki Murakami.", PageCount = 467, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 9, Title = "I, Robot", ISBN = "978-0-55-338256-6", Publisher = "Bantam Spectra", PublicationYear = 1950, Description = "A collection of nine science fiction short stories.", PageCount = 224, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 10, Title = "Song of Solomon", ISBN = "978-1-40-003342-3", Publisher = "Vintage", PublicationYear = 1977, Description = "A novel by Toni Morrison about African-American identity.", PageCount = 337, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 11, Title = "Sense and Sensibility", ISBN = "978-0-14-143966-4", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "Jane Austen's first published novel.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new Book { Id = 12, Title = "Love in the Time of Cholera", ISBN = "978-0-14-024990-3", Publisher = "Penguin Books", PublicationYear = 1985, Description = "A love story spanning over fifty years.", PageCount = 348, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now }
        );

        // BookAuthors
        modelBuilder.Entity<BookAuthor>().HasData(
            new BookAuthor { BookId = 1, AuthorId = 1 },   // Pride and Prejudice - Austen
            new BookAuthor { BookId = 2, AuthorId = 2 },   // 1984 - Orwell
            new BookAuthor { BookId = 3, AuthorId = 3 },   // One Hundred Years - Marquez
            new BookAuthor { BookId = 4, AuthorId = 4 },   // Beloved - Morrison
            new BookAuthor { BookId = 5, AuthorId = 5 },   // Norwegian Wood - Murakami
            new BookAuthor { BookId = 6, AuthorId = 6 },   // Foundation - Asimov
            new BookAuthor { BookId = 7, AuthorId = 2 },   // Animal Farm - Orwell
            new BookAuthor { BookId = 8, AuthorId = 5 },   // Kafka on the Shore - Murakami
            new BookAuthor { BookId = 9, AuthorId = 6 },   // I, Robot - Asimov
            new BookAuthor { BookId = 10, AuthorId = 4 },  // Song of Solomon - Morrison
            new BookAuthor { BookId = 11, AuthorId = 1 },  // Sense and Sensibility - Austen
            new BookAuthor { BookId = 12, AuthorId = 3 }   // Love in the Time of Cholera - Marquez
        );

        // BookCategories
        modelBuilder.Entity<BookCategory>().HasData(
            new BookCategory { BookId = 1, CategoryId = 1 },   // Pride - Fiction
            new BookCategory { BookId = 1, CategoryId = 6 },   // Pride - Classic
            new BookCategory { BookId = 2, CategoryId = 2 },   // 1984 - Sci-Fi
            new BookCategory { BookId = 2, CategoryId = 1 },   // 1984 - Fiction
            new BookCategory { BookId = 3, CategoryId = 1 },   // 100 Years - Fiction
            new BookCategory { BookId = 3, CategoryId = 6 },   // 100 Years - Classic
            new BookCategory { BookId = 4, CategoryId = 1 },   // Beloved - Fiction
            new BookCategory { BookId = 5, CategoryId = 1 },   // Norwegian Wood - Fiction
            new BookCategory { BookId = 6, CategoryId = 2 },   // Foundation - Sci-Fi
            new BookCategory { BookId = 7, CategoryId = 1 },   // Animal Farm - Fiction
            new BookCategory { BookId = 7, CategoryId = 6 },   // Animal Farm - Classic
            new BookCategory { BookId = 8, CategoryId = 1 },   // Kafka - Fiction
            new BookCategory { BookId = 9, CategoryId = 2 },   // I, Robot - Sci-Fi
            new BookCategory { BookId = 10, CategoryId = 1 },  // Song of Solomon - Fiction
            new BookCategory { BookId = 11, CategoryId = 1 },  // Sense - Fiction
            new BookCategory { BookId = 11, CategoryId = 6 },  // Sense - Classic
            new BookCategory { BookId = 12, CategoryId = 1 }   // Love - Fiction
        );

        // Patrons (6+)
        modelBuilder.Entity<Patron>().HasData(
            new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Main St", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Patron { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Patron { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Rd", MembershipDate = new DateOnly(2023, 6, 1), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Patron { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Elm St", MembershipDate = new DateOnly(2023, 9, 10), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Patron { Id = 5, FirstName = "Eva", LastName = "Martinez", Email = "eva.martinez@email.com", Phone = "555-0105", Address = "654 Birch Ln", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Patron { Id = 6, FirstName = "Frank", LastName = "Davis", Email = "frank.davis@email.com", Phone = "555-0106", Address = "987 Cedar Dr", MembershipDate = new DateOnly(2022, 5, 20), MembershipType = MembershipType.Standard, IsActive = false, CreatedAt = now, UpdatedAt = now }
        );

        // Loans (8+, various states)
        // Active loans: patron 1 has book 1, patron 2 has book 2, patron 3 has book 5
        // Overdue: patron 2 has book 3 (overdue), patron 4 has book 9 (overdue)
        // Returned: patron 1 returned book 7, patron 5 returned book 2, patron 4 returned book 6
        modelBuilder.Entity<Loan>().HasData(
            new Loan { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-10) },
            new Loan { Id = 2, BookId = 2, PatronId = 2, LoanDate = now.AddDays(-7), DueDate = now.AddDays(7), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-7) },
            new Loan { Id = 3, BookId = 5, PatronId = 3, LoanDate = now.AddDays(-5), DueDate = now.AddDays(2), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-5) },
            new Loan { Id = 4, BookId = 3, PatronId = 2, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-20) },
            new Loan { Id = 5, BookId = 9, PatronId = 4, LoanDate = now.AddDays(-18), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-18) },
            new Loan { Id = 6, BookId = 7, PatronId = 1, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-9), ReturnDate = now.AddDays(-12), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-30) },
            new Loan { Id = 7, BookId = 2, PatronId = 5, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-4), ReturnDate = now.AddDays(-5), Status = LoanStatus.Returned, RenewalCount = 1, CreatedAt = now.AddDays(-25) },
            new Loan { Id = 8, BookId = 6, PatronId = 4, LoanDate = now.AddDays(-15), DueDate = now.AddDays(-1), ReturnDate = now.AddDays(-2), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-15) }
        );

        // Reservations
        modelBuilder.Entity<Reservation>().HasData(
            new Reservation { Id = 1, BookId = 5, PatronId = 1, ReservationDate = now.AddDays(-3), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-3) },
            new Reservation { Id = 2, BookId = 5, PatronId = 4, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 2, CreatedAt = now.AddDays(-2) },
            new Reservation { Id = 3, BookId = 2, PatronId = 3, ReservationDate = now.AddDays(-1), ExpirationDate = now.AddDays(2), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now.AddDays(-1) }
        );

        // Fines
        modelBuilder.Entity<Fine>().HasData(
            new Fine { Id = 1, PatronId = 2, LoanId = 4, Amount = 1.50m, Reason = "Overdue return", IssuedDate = now, Status = FineStatus.Unpaid, CreatedAt = now },
            new Fine { Id = 2, PatronId = 4, LoanId = 5, Amount = 1.00m, Reason = "Overdue return", IssuedDate = now, Status = FineStatus.Unpaid, CreatedAt = now },
            new Fine { Id = 3, PatronId = 5, LoanId = 7, Amount = 0.25m, Reason = "Overdue return - 1 day late", IssuedDate = now.AddDays(-5), PaidDate = now.AddDays(-4), Status = FineStatus.Paid, CreatedAt = now.AddDays(-5) }
        );
    }
}
