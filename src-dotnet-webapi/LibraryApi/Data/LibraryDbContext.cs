using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
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
            e.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            e.Property(a => a.LastName).IsRequired().HasMaxLength(100);
            e.Property(a => a.Biography).HasMaxLength(2000);
            e.Property(a => a.Country).HasMaxLength(100);
        });

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Description).HasMaxLength(500);
        });

        // Book
        modelBuilder.Entity<Book>(e =>
        {
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
            e.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            e.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            e.Property(p => p.Email).IsRequired();
            e.HasIndex(p => p.Email).IsUnique();
            e.Property(p => p.Phone).HasMaxLength(20);
            e.Property(p => p.Address).HasMaxLength(500);
            e.Property(p => p.MembershipType).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.IsActive).HasDefaultValue(true);
        });

        // Loan
        modelBuilder.Entity<Loan>(e =>
        {
            e.HasOne(l => l.Book).WithMany(b => b.Loans).HasForeignKey(l => l.BookId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(l => l.Patron).WithMany(p => p.Loans).HasForeignKey(l => l.PatronId).OnDelete(DeleteBehavior.Restrict);
            e.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        });

        // Reservation
        modelBuilder.Entity<Reservation>(e =>
        {
            e.HasOne(r => r.Book).WithMany(b => b.Reservations).HasForeignKey(r => r.BookId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Patron).WithMany(p => p.Reservations).HasForeignKey(r => r.PatronId).OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        });

        // Fine
        modelBuilder.Entity<Fine>(e =>
        {
            e.HasOne(f => f.Patron).WithMany(p => p.Fines).HasForeignKey(f => f.PatronId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.Loan).WithMany(l => l.Fines).HasForeignKey(f => f.LoanId).OnDelete(DeleteBehavior.Restrict);
            e.Property(f => f.Amount).HasColumnType("decimal(10,2)");
            e.Property(f => f.Reason).IsRequired().HasMaxLength(500);
            e.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Authors
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, journalist and critic.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 2, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known primarily for her six major novels.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for science fiction.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 4, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli public intellectual, historian and professor.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 5, FirstName = "Agatha", LastName = "Christie", Biography = "English writer known for her detective novels.", BirthDate = new DateOnly(1890, 9, 15), Country = "United Kingdom", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Author { Id = 6, FirstName = "Carl", LastName = "Sagan", Biography = "American astronomer, planetary scientist, and science communicator.", BirthDate = new DateOnly(1934, 11, 9), Country = "United States", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Fiction", Description = "Literary works of imaginative narration" },
            new Category { Id = 2, Name = "Science Fiction", Description = "Fiction based on imagined future scientific advances" },
            new Category { Id = 3, Name = "History", Description = "Non-fiction works about historical events" },
            new Category { Id = 4, Name = "Science", Description = "Non-fiction works about scientific topics" },
            new Category { Id = 5, Name = "Biography", Description = "Accounts of someone's life written by another" },
            new Category { Id = 6, Name = "Mystery", Description = "Fiction dealing with the solution of a crime" }
        );

        var utcNow = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);

        // Books (12+)
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "1984", ISBN = "978-0-451-52493-5", Publisher = "Secker & Warburg", PublicationYear = 1949, Description = "A dystopian novel set in a totalitarian society.", PageCount = 328, Language = "English", TotalCopies = 5, AvailableCopies = 3, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 2, Title = "Animal Farm", ISBN = "978-0-451-52634-2", Publisher = "Secker & Warburg", PublicationYear = 1945, Description = "An allegorical novella about a group of farm animals.", PageCount = 112, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 3, Title = "Pride and Prejudice", ISBN = "978-0-14-143951-8", Publisher = "T. Egerton", PublicationYear = 1813, Description = "A romantic novel following Elizabeth Bennet.", PageCount = 432, Language = "English", TotalCopies = 4, AvailableCopies = 4, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 4, Title = "Sense and Sensibility", ISBN = "978-0-14-143966-2", Publisher = "T. Egerton", PublicationYear = 1811, Description = "A novel about the Dashwood sisters.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 5, Title = "Foundation", ISBN = "978-0-553-29335-7", Publisher = "Gnome Press", PublicationYear = 1951, Description = "The first novel in the Foundation series.", PageCount = 244, Language = "English", TotalCopies = 3, AvailableCopies = 1, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 6, Title = "I, Robot", ISBN = "978-0-553-29438-5", Publisher = "Gnome Press", PublicationYear = 1950, Description = "A collection of robot short stories.", PageCount = 253, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 7, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0-06-231609-7", Publisher = "Harper", PublicationYear = 2011, Description = "A book about the history of the human species.", PageCount = 443, Language = "English", TotalCopies = 4, AvailableCopies = 2, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 8, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0-06-246431-6", Publisher = "Harper", PublicationYear = 2015, Description = "An exploration of humanity's future.", PageCount = 450, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 9, Title = "Murder on the Orient Express", ISBN = "978-0-00-711931-8", Publisher = "Collins Crime Club", PublicationYear = 1934, Description = "A detective novel featuring Hercule Poirot.", PageCount = 256, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 10, Title = "The Murder of Roger Ackroyd", ISBN = "978-0-00-712717-7", Publisher = "Collins Crime Club", PublicationYear = 1926, Description = "A Poirot mystery novel.", PageCount = 312, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 11, Title = "Cosmos", ISBN = "978-0-345-53943-4", Publisher = "Random House", PublicationYear = 1980, Description = "A popular science book on astronomy.", PageCount = 396, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Book { Id = 12, Title = "The Demon-Haunted World", ISBN = "978-0-345-40946-1", Publisher = "Random House", PublicationYear = 1995, Description = "A book about scientific thinking and skepticism.", PageCount = 457, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = utcNow, UpdatedAt = utcNow }
        );

        // BookAuthors
        modelBuilder.Entity<BookAuthor>().HasData(
            new BookAuthor { BookId = 1, AuthorId = 1 },   // 1984 - Orwell
            new BookAuthor { BookId = 2, AuthorId = 1 },   // Animal Farm - Orwell
            new BookAuthor { BookId = 3, AuthorId = 2 },   // Pride and Prejudice - Austen
            new BookAuthor { BookId = 4, AuthorId = 2 },   // Sense and Sensibility - Austen
            new BookAuthor { BookId = 5, AuthorId = 3 },   // Foundation - Asimov
            new BookAuthor { BookId = 6, AuthorId = 3 },   // I, Robot - Asimov
            new BookAuthor { BookId = 7, AuthorId = 4 },   // Sapiens - Harari
            new BookAuthor { BookId = 8, AuthorId = 4 },   // Homo Deus - Harari
            new BookAuthor { BookId = 9, AuthorId = 5 },   // Orient Express - Christie
            new BookAuthor { BookId = 10, AuthorId = 5 },  // Roger Ackroyd - Christie
            new BookAuthor { BookId = 11, AuthorId = 6 },  // Cosmos - Sagan
            new BookAuthor { BookId = 12, AuthorId = 6 }   // Demon-Haunted - Sagan
        );

        // BookCategories
        modelBuilder.Entity<BookCategory>().HasData(
            new BookCategory { BookId = 1, CategoryId = 1 },   // 1984 - Fiction
            new BookCategory { BookId = 1, CategoryId = 2 },   // 1984 - Science Fiction
            new BookCategory { BookId = 2, CategoryId = 1 },   // Animal Farm - Fiction
            new BookCategory { BookId = 3, CategoryId = 1 },   // Pride and Prejudice - Fiction
            new BookCategory { BookId = 4, CategoryId = 1 },   // Sense and Sensibility - Fiction
            new BookCategory { BookId = 5, CategoryId = 2 },   // Foundation - Science Fiction
            new BookCategory { BookId = 6, CategoryId = 2 },   // I, Robot - Science Fiction
            new BookCategory { BookId = 7, CategoryId = 3 },   // Sapiens - History
            new BookCategory { BookId = 7, CategoryId = 5 },   // Sapiens - Biography
            new BookCategory { BookId = 8, CategoryId = 3 },   // Homo Deus - History
            new BookCategory { BookId = 8, CategoryId = 4 },   // Homo Deus - Science
            new BookCategory { BookId = 9, CategoryId = 1 },   // Orient Express - Fiction
            new BookCategory { BookId = 9, CategoryId = 6 },   // Orient Express - Mystery
            new BookCategory { BookId = 10, CategoryId = 1 },  // Roger Ackroyd - Fiction
            new BookCategory { BookId = 10, CategoryId = 6 },  // Roger Ackroyd - Mystery
            new BookCategory { BookId = 11, CategoryId = 4 },  // Cosmos - Science
            new BookCategory { BookId = 12, CategoryId = 4 }   // Demon-Haunted - Science
        );

        // Patrons (6+, mix of types, one inactive)
        modelBuilder.Entity<Patron>().HasData(
            new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Main St, Springfield", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Patron { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave, Springfield", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Patron { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Rd, Springfield", MembershipDate = new DateOnly(2023, 6, 10), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Patron { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Elm St, Springfield", MembershipDate = new DateOnly(2023, 9, 5), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Patron { Id = 5, FirstName = "Eva", LastName = "Martinez", Email = "eva.martinez@email.com", Phone = "555-0105", Address = "654 Birch Ln, Springfield", MembershipDate = new DateOnly(2024, 1, 1), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = utcNow, UpdatedAt = utcNow },
            new Patron { Id = 6, FirstName = "Frank", LastName = "Davis", Email = "frank.davis@email.com", Phone = "555-0106", Address = "987 Cedar Dr, Springfield", MembershipDate = new DateOnly(2022, 5, 1), MembershipType = MembershipType.Standard, IsActive = false, CreatedAt = utcNow, UpdatedAt = utcNow }
        );

        // Loans (8+, mix of Active, Returned, Overdue) — AvailableCopies must be consistent
        // Active loans: Book 1 (2 active), Book 2 (1 active), Book 5 (2 active), Book 6 (1 active), Book 7 (2 active), Book 9 (1 active), Book 11 (1 active)
        // Total active by book: 1→2, 2→1, 5→2, 6→1, 7→2, 9→1, 11→1 = matches Available = Total - Active
        modelBuilder.Entity<Loan>().HasData(
            // Active loans
            new Loan { Id = 1, BookId = 1, PatronId = 1, LoanDate = new DateTime(2024, 5, 20, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 6, 10, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = utcNow },
            new Loan { Id = 2, BookId = 1, PatronId = 2, LoanDate = new DateTime(2024, 5, 22, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 6, 5, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = utcNow },
            new Loan { Id = 3, BookId = 5, PatronId = 1, LoanDate = new DateTime(2024, 5, 15, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 6, 5, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 1, CreatedAt = utcNow },
            new Loan { Id = 4, BookId = 5, PatronId = 3, LoanDate = new DateTime(2024, 5, 25, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = utcNow },
            new Loan { Id = 5, BookId = 7, PatronId = 2, LoanDate = new DateTime(2024, 5, 10, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 5, 24, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = utcNow },
            new Loan { Id = 6, BookId = 7, PatronId = 4, LoanDate = new DateTime(2024, 5, 18, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = utcNow },
            // Returned loans
            new Loan { Id = 7, BookId = 3, PatronId = 1, LoanDate = new DateTime(2024, 4, 1, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 4, 22, 10, 0, 0, DateTimeKind.Utc), ReturnDate = new DateTime(2024, 4, 20, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = utcNow },
            new Loan { Id = 8, BookId = 9, PatronId = 5, LoanDate = new DateTime(2024, 4, 10, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 5, 1, 10, 0, 0, DateTimeKind.Utc), ReturnDate = new DateTime(2024, 5, 5, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = utcNow },
            // More active
            new Loan { Id = 9, BookId = 2, PatronId = 4, LoanDate = new DateTime(2024, 5, 28, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 6, 11, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = utcNow },
            new Loan { Id = 10, BookId = 6, PatronId = 5, LoanDate = new DateTime(2024, 5, 20, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 6, 10, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = utcNow },
            new Loan { Id = 11, BookId = 9, PatronId = 3, LoanDate = new DateTime(2024, 5, 26, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 6, 2, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = utcNow },
            new Loan { Id = 12, BookId = 11, PatronId = 1, LoanDate = new DateTime(2024, 5, 22, 10, 0, 0, DateTimeKind.Utc), DueDate = new DateTime(2024, 6, 12, 10, 0, 0, DateTimeKind.Utc), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = utcNow }
        );

        // Reservations
        modelBuilder.Entity<Reservation>().HasData(
            new Reservation { Id = 1, BookId = 5, PatronId = 4, ReservationDate = new DateTime(2024, 5, 28, 10, 0, 0, DateTimeKind.Utc), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = utcNow },
            new Reservation { Id = 2, BookId = 5, PatronId = 5, ReservationDate = new DateTime(2024, 5, 29, 10, 0, 0, DateTimeKind.Utc), Status = ReservationStatus.Pending, QueuePosition = 2, CreatedAt = utcNow },
            new Reservation { Id = 3, BookId = 1, PatronId = 3, ReservationDate = new DateTime(2024, 5, 30, 10, 0, 0, DateTimeKind.Utc), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = utcNow }
        );

        // Fines
        modelBuilder.Entity<Fine>().HasData(
            new Fine { Id = 1, PatronId = 2, LoanId = 5, Amount = 2.00m, Reason = "Overdue return - 8 days late", IssuedDate = new DateTime(2024, 5, 24, 10, 0, 0, DateTimeKind.Utc), Status = FineStatus.Unpaid, CreatedAt = utcNow },
            new Fine { Id = 2, PatronId = 5, LoanId = 8, Amount = 1.00m, Reason = "Overdue return - 4 days late", IssuedDate = new DateTime(2024, 5, 5, 10, 0, 0, DateTimeKind.Utc), PaidDate = new DateTime(2024, 5, 10, 10, 0, 0, DateTimeKind.Utc), Status = FineStatus.Paid, CreatedAt = utcNow },
            new Fine { Id = 3, PatronId = 4, LoanId = 9, Amount = 0.50m, Reason = "Damaged book cover", IssuedDate = new DateTime(2024, 5, 30, 10, 0, 0, DateTimeKind.Utc), Status = FineStatus.Unpaid, CreatedAt = utcNow }
        );
    }
}
