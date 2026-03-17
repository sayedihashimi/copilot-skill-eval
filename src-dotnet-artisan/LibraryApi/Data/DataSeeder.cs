using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public sealed class DataSeeder(LibraryDbContext db, ILogger<DataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Authors.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded — skipping");
            return;
        }

        logger.LogInformation("Seeding database...");

        // ── Authors ──
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist known for his sharp criticism of political oppression.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom" },
            new() { Id = 2, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her social commentary and wit.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom" },
            new() { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, famous for science fiction.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States" },
            new() { Id = 4, FirstName = "Ursula", LastName = "Le Guin", Biography = "American author known for science fiction and fantasy works.", BirthDate = new DateOnly(1929, 10, 21), Country = "United States" },
            new() { Id = 5, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and professor, author of popular science bestsellers.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel" },
            new() { Id = 6, FirstName = "Mary", LastName = "Shelley", Biography = "English novelist who wrote Frankenstein, considered the first science fiction novel.", BirthDate = new DateOnly(1797, 8, 30), Country = "United Kingdom" },
        };
        db.Authors.AddRange(authors);

        // ── Categories ──
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Narrative works of imaginative storytelling" },
            new() { Id = 2, Name = "Science Fiction", Description = "Speculative fiction dealing with futuristic concepts" },
            new() { Id = 3, Name = "History", Description = "Non-fiction works about past events" },
            new() { Id = 4, Name = "Science", Description = "Non-fiction works about the natural world" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of a person's life" },
            new() { Id = 6, Name = "Classic Literature", Description = "Enduring works of literary significance" },
        };
        db.Categories.AddRange(categories);

        // ── Books ──
        var books = new List<Book>
        {
            new() { Id = 1, Title = "1984", ISBN = "978-0451524935", Publisher = "Signet Classics", PublicationYear = 1949, Description = "A dystopian novel about totalitarian surveillance.", PageCount = 328, TotalCopies = 5, AvailableCopies = 3 },
            new() { Id = 2, Title = "Animal Farm", ISBN = "978-0451526342", Publisher = "Signet Classics", PublicationYear = 1945, Description = "An allegorical novella about a group of farm animals.", PageCount = 141, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 3, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel about manners and matrimony.", PageCount = 432, TotalCopies = 4, AvailableCopies = 3 },
            new() { Id = 4, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in the Foundation series about the fall of a galactic empire.", PageCount = 244, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 5, Title = "The Left Hand of Darkness", ISBN = "978-0441478125", Publisher = "Ace Books", PublicationYear = 1969, Description = "A science fiction novel exploring gender and society.", PageCount = 304, TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 6, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A sweeping narrative of human history.", PageCount = 464, TotalCopies = 4, AvailableCopies = 2 },
            new() { Id = 7, Title = "I, Robot", ISBN = "978-0553382563", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of robot short stories.", PageCount = 224, TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 8, Title = "Frankenstein", ISBN = "978-0486282114", Publisher = "Dover Publications", PublicationYear = 1818, Description = "A Gothic novel about a scientist who creates life.", PageCount = 166, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 9, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about two sisters and their romantic entanglements.", PageCount = 409, TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 10, Title = "21 Lessons for the 21st Century", ISBN = "978-0525512172", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "Explores present-day issues and their future implications.", PageCount = 372, TotalCopies = 3, AvailableCopies = 3 },
            new() { Id = 11, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0062464316", Publisher = "Harper", PublicationYear = 2017, Description = "A look at humanity's future.", PageCount = 464, TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 12, Title = "The Dispossessed", ISBN = "978-0061054884", Publisher = "Harper Voyager", PublicationYear = 1974, Description = "An ambiguous utopia exploring anarchism and capitalism.", PageCount = 387, TotalCopies = 1, AvailableCopies = 0 },
        };
        db.Books.AddRange(books);

        // ── BookAuthors ──
        db.BookAuthors.AddRange(
            new BookAuthor { BookId = 1, AuthorId = 1 },
            new BookAuthor { BookId = 2, AuthorId = 1 },
            new BookAuthor { BookId = 3, AuthorId = 2 },
            new BookAuthor { BookId = 4, AuthorId = 3 },
            new BookAuthor { BookId = 5, AuthorId = 4 },
            new BookAuthor { BookId = 6, AuthorId = 5 },
            new BookAuthor { BookId = 7, AuthorId = 3 },
            new BookAuthor { BookId = 8, AuthorId = 6 },
            new BookAuthor { BookId = 9, AuthorId = 2 },
            new BookAuthor { BookId = 10, AuthorId = 5 },
            new BookAuthor { BookId = 11, AuthorId = 5 },
            new BookAuthor { BookId = 12, AuthorId = 4 }
        );

        // ── BookCategories ──
        db.BookCategories.AddRange(
            new BookCategory { BookId = 1, CategoryId = 1 },
            new BookCategory { BookId = 1, CategoryId = 2 },
            new BookCategory { BookId = 1, CategoryId = 6 },
            new BookCategory { BookId = 2, CategoryId = 1 },
            new BookCategory { BookId = 2, CategoryId = 6 },
            new BookCategory { BookId = 3, CategoryId = 1 },
            new BookCategory { BookId = 3, CategoryId = 6 },
            new BookCategory { BookId = 4, CategoryId = 2 },
            new BookCategory { BookId = 5, CategoryId = 2 },
            new BookCategory { BookId = 6, CategoryId = 3 },
            new BookCategory { BookId = 6, CategoryId = 4 },
            new BookCategory { BookId = 7, CategoryId = 2 },
            new BookCategory { BookId = 8, CategoryId = 1 },
            new BookCategory { BookId = 8, CategoryId = 2 },
            new BookCategory { BookId = 8, CategoryId = 6 },
            new BookCategory { BookId = 9, CategoryId = 1 },
            new BookCategory { BookId = 9, CategoryId = 6 },
            new BookCategory { BookId = 10, CategoryId = 3 },
            new BookCategory { BookId = 10, CategoryId = 4 },
            new BookCategory { BookId = 11, CategoryId = 3 },
            new BookCategory { BookId = 11, CategoryId = 4 },
            new BookCategory { BookId = 12, CategoryId = 2 }
        );

        // ── Patrons ──
        var now = DateTime.UtcNow;
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Maple St", MembershipType = MembershipType.Premium, MembershipDate = new DateOnly(2023, 1, 15) },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave", MembershipType = MembershipType.Standard, MembershipDate = new DateOnly(2023, 3, 20) },
            new() { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", MembershipType = MembershipType.Student, MembershipDate = new DateOnly(2024, 9, 1) },
            new() { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "789 Pine Rd", MembershipType = MembershipType.Standard, MembershipDate = new DateOnly(2022, 6, 10) },
            new() { Id = 5, FirstName = "Eve", LastName = "Davis", Email = "eve.davis@email.com", Phone = "555-0105", MembershipType = MembershipType.Premium, MembershipDate = new DateOnly(2023, 11, 1) },
            new() { Id = 6, FirstName = "Frank", LastName = "Miller", Email = "frank.miller@email.com", Phone = "555-0106", Address = "321 Elm St", MembershipType = MembershipType.Standard, IsActive = false, MembershipDate = new DateOnly(2021, 4, 5) },
        };
        db.Patrons.AddRange(patrons);

        // ── Loans ──
        // Active loans: Books 1, 4, 5, 6, 11, 12 each have 1 fewer available copy
        // Book 1: 5 total, 3 available → 2 active loans
        // Book 4: 3 total, 2 available → 1 active loan
        // Book 5: 2 total, 1 available → 1 active loan
        // Book 6: 4 total, 2 available → 2 active loans (1 active + 1 overdue counts as active for available)
        // Book 11: 2 total, 1 available → 1 active loan
        // Book 12: 1 total, 0 available → 1 active loan
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active },
            new() { Id = 2, BookId = 4, PatronId = 2, LoanDate = now.AddDays(-3), DueDate = now.AddDays(11), Status = LoanStatus.Active },
            new() { Id = 3, BookId = 5, PatronId = 3, LoanDate = now.AddDays(-2), DueDate = now.AddDays(5), Status = LoanStatus.Active },
            new() { Id = 4, BookId = 1, PatronId = 4, LoanDate = now.AddDays(-10), DueDate = now.AddDays(4), Status = LoanStatus.Active },
            new() { Id = 5, BookId = 12, PatronId = 1, LoanDate = now.AddDays(-7), DueDate = now.AddDays(14), Status = LoanStatus.Active },
            new() { Id = 6, BookId = 11, PatronId = 5, LoanDate = now.AddDays(-4), DueDate = now.AddDays(17), Status = LoanStatus.Active },
            // Overdue loan (counts against available copies too)
            new() { Id = 7, BookId = 6, PatronId = 2, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue },
            // Active loan on book 6
            new() { Id = 8, BookId = 6, PatronId = 5, LoanDate = now.AddDays(-3), DueDate = now.AddDays(18), Status = LoanStatus.Active },
            // Returned loans
            new() { Id = 9, BookId = 3, PatronId = 1, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-18), Status = LoanStatus.Returned },
            new() { Id = 10, BookId = 8, PatronId = 4, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-11), ReturnDate = now.AddDays(-8), Status = LoanStatus.Returned, RenewalCount = 1 },
        };
        db.Loans.AddRange(loans);

        // ── Reservations ──
        var reservations = new List<Reservation>
        {
            // Pending reservation on book 12 (no copies available)
            new() { Id = 1, BookId = 12, PatronId = 3, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 1 },
            // Another pending on book 12
            new() { Id = 2, BookId = 12, PatronId = 4, ReservationDate = now.AddHours(-12), Status = ReservationStatus.Pending, QueuePosition = 2 },
            // Ready reservation (copy returned, patron notified)
            new() { Id = 3, BookId = 8, PatronId = 2, ReservationDate = now.AddDays(-3), ExpirationDate = now.AddDays(2), Status = ReservationStatus.Ready, QueuePosition = 1 },
        };
        db.Reservations.AddRange(reservations);

        // ── Fines ──
        var fines = new List<Fine>
        {
            // Unpaid fine from overdue loan #7
            new() { Id = 1, PatronId = 2, LoanId = 7, Amount = 1.50m, Reason = "Overdue: 6 days late on 'Sapiens'", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid },
            // Paid fine from returned-late loan #10
            new() { Id = 2, PatronId = 4, LoanId = 10, Amount = 0.75m, Reason = "Overdue: 3 days late on 'Frankenstein'", IssuedDate = now.AddDays(-8), PaidDate = now.AddDays(-7), Status = FineStatus.Paid },
            // Unpaid fine — small amount
            new() { Id = 3, PatronId = 4, LoanId = 9, Amount = 0.50m, Reason = "Damaged book cover on 'Pride and Prejudice'", IssuedDate = now.AddDays(-15), Status = FineStatus.Unpaid },
        };
        db.Fines.AddRange(fines);

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Database seeded with {Authors} authors, {Books} books, {Patrons} patrons, {Loans} loans", authors.Count, books.Count, patrons.Count, loans.Count);
    }
}
