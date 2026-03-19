using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public static class DataSeeder
{
    public static async Task SeedAsync(LibraryDbContext db)
    {
        if (await db.Authors.AnyAsync()) return; // Already seeded

        var now = DateTime.UtcNow;

        // --- Authors ---
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist known for his dystopian works.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom", CreatedAt = now },
            new() { Id = 2, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her social commentary and wit.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = now },
            new() { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, prolific science fiction author.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = now },
            new() { Id = 4, FirstName = "Toni", LastName = "Morrison", Biography = "American novelist and Nobel Prize winner in Literature.", BirthDate = new DateOnly(1931, 2, 18), Country = "United States", CreatedAt = now },
            new() { Id = 5, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and author of Sapiens and Homo Deus.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel", CreatedAt = now },
            new() { Id = 6, FirstName = "Mary", LastName = "Shelley", Biography = "English novelist who wrote Frankenstein, a foundational work of science fiction.", BirthDate = new DateOnly(1797, 8, 30), Country = "United Kingdom", CreatedAt = now }
        };
        db.Authors.AddRange(authors);

        // --- Categories ---
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Novels and stories based on imagined events." },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction based on futuristic science and technology." },
            new() { Id = 3, Name = "History", Description = "Books about historical events and periods." },
            new() { Id = 4, Name = "Science", Description = "Books about scientific subjects and discoveries." },
            new() { Id = 5, Name = "Biography", Description = "Books about the lives of real people." },
            new() { Id = 6, Name = "Classic Literature", Description = "Enduring works of literary merit." }
        };
        db.Categories.AddRange(categories);

        // --- Books ---
        // Book 1: 1984 by Orwell — 3 copies, 1 checked out = 2 available
        // Book 2: Animal Farm by Orwell — 2 copies, 1 checked out = 1 available
        // Book 3: Pride and Prejudice by Austen — 4 copies, 0 checked out = 4 available
        // Book 4: Foundation by Asimov — 2 copies, 1 checked out = 1 available
        // Book 5: I, Robot by Asimov — 1 copy, 1 checked out = 0 available
        // Book 6: Beloved by Morrison — 3 copies, 1 checked out = 2 available
        // Book 7: Song of Solomon by Morrison — 2 copies, 0 checked out = 2 available
        // Book 8: Sapiens by Harari — 5 copies, 2 checked out = 3 available
        // Book 9: Homo Deus by Harari — 2 copies, 0 checked out = 2 available
        // Book 10: Frankenstein by Shelley — 3 copies, 1 checked out = 2 available
        // Book 11: Sense and Sensibility by Austen — 1 copy, 0 checked out = 1 available
        // Book 12: 21 Lessons by Harari — 2 copies, 1 checked out = 1 available

        var books = new List<Book>
        {
            new() { Id = 1, Title = "1984", ISBN = "9780451524935", Publisher = "Signet Classics", PublicationYear = 1949, Description = "A dystopian novel set in a totalitarian society.", PageCount = 328, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "Animal Farm", ISBN = "9780451526342", Publisher = "Signet Classics", PublicationYear = 1945, Description = "An allegorical novella about a farm rebellion.", PageCount = 112, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Pride and Prejudice", ISBN = "9780141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, Language = "English", TotalCopies = 4, AvailableCopies = 4, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "Foundation", ISBN = "9780553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "I, Robot", ISBN = "9780553294385", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of short stories about robots.", PageCount = 224, Language = "English", TotalCopies = 1, AvailableCopies = 0, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "Beloved", ISBN = "9781400033416", Publisher = "Vintage", PublicationYear = 1987, Description = "A powerful novel about the legacy of slavery.", PageCount = 324, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "Song of Solomon", ISBN = "9781400033423", Publisher = "Vintage", PublicationYear = 1977, Description = "A novel about African-American identity and heritage.", PageCount = 337, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "Sapiens: A Brief History of Humankind", ISBN = "9780062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A narrative of humanity's creation and evolution.", PageCount = 443, Language = "English", TotalCopies = 5, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "9780062464316", Publisher = "Harper", PublicationYear = 2017, Description = "A look at humanity's future.", PageCount = 448, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "Frankenstein", ISBN = "9780141439471", Publisher = "Penguin Classics", PublicationYear = 1818, Description = "A foundational work of science fiction about creation and responsibility.", PageCount = 280, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "Sense and Sensibility", ISBN = "9780141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about two sisters and their romantic experiences.", PageCount = 409, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "21 Lessons for the 21st Century", ISBN = "9780525512196", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "A meditation on the biggest questions of our time.", PageCount = 372, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now }
        };
        db.Books.AddRange(books);

        // --- BookAuthors ---
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },  // 1984 - Orwell
            new() { BookId = 2, AuthorId = 1 },  // Animal Farm - Orwell
            new() { BookId = 3, AuthorId = 2 },  // Pride and Prejudice - Austen
            new() { BookId = 4, AuthorId = 3 },  // Foundation - Asimov
            new() { BookId = 5, AuthorId = 3 },  // I, Robot - Asimov
            new() { BookId = 6, AuthorId = 4 },  // Beloved - Morrison
            new() { BookId = 7, AuthorId = 4 },  // Song of Solomon - Morrison
            new() { BookId = 8, AuthorId = 5 },  // Sapiens - Harari
            new() { BookId = 9, AuthorId = 5 },  // Homo Deus - Harari
            new() { BookId = 10, AuthorId = 6 }, // Frankenstein - Shelley
            new() { BookId = 11, AuthorId = 2 }, // Sense and Sensibility - Austen
            new() { BookId = 12, AuthorId = 5 }  // 21 Lessons - Harari
        };
        db.BookAuthors.AddRange(bookAuthors);

        // --- BookCategories ---
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 },  // 1984 - Fiction
            new() { BookId = 1, CategoryId = 6 },  // 1984 - Classic Literature
            new() { BookId = 2, CategoryId = 1 },  // Animal Farm - Fiction
            new() { BookId = 2, CategoryId = 6 },  // Animal Farm - Classic Literature
            new() { BookId = 3, CategoryId = 1 },  // P&P - Fiction
            new() { BookId = 3, CategoryId = 6 },  // P&P - Classic Literature
            new() { BookId = 4, CategoryId = 2 },  // Foundation - Sci-Fi
            new() { BookId = 5, CategoryId = 2 },  // I, Robot - Sci-Fi
            new() { BookId = 6, CategoryId = 1 },  // Beloved - Fiction
            new() { BookId = 7, CategoryId = 1 },  // Song of Solomon - Fiction
            new() { BookId = 8, CategoryId = 3 },  // Sapiens - History
            new() { BookId = 8, CategoryId = 4 },  // Sapiens - Science
            new() { BookId = 9, CategoryId = 3 },  // Homo Deus - History
            new() { BookId = 9, CategoryId = 4 },  // Homo Deus - Science
            new() { BookId = 10, CategoryId = 2 }, // Frankenstein - Sci-Fi
            new() { BookId = 10, CategoryId = 6 }, // Frankenstein - Classic Literature
            new() { BookId = 11, CategoryId = 1 }, // S&S - Fiction
            new() { BookId = 11, CategoryId = 6 }, // S&S - Classic Literature
            new() { BookId = 12, CategoryId = 3 }, // 21 Lessons - History
            new() { BookId = 12, CategoryId = 4 }  // 21 Lessons - Science
        };
        db.BookCategories.AddRange(bookCategories);

        // --- Patrons ---
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Maple St", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Rd", MembershipDate = new DateOnly(2023, 6, 10), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Elm St", MembershipDate = new DateOnly(2023, 9, 1), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Eva", LastName = "Martinez", Email = "eva.martinez@email.com", Phone = "555-0105", Address = "654 Birch Ln", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Frank", LastName = "Davis", Email = "frank.davis@email.com", Phone = "555-0106", Address = "987 Cedar Dr", MembershipDate = new DateOnly(2022, 11, 20), MembershipType = MembershipType.Standard, IsActive = false, CreatedAt = now, UpdatedAt = now }
        };
        db.Patrons.AddRange(patrons);

        await db.SaveChangesAsync();

        // --- Loans ---
        // Active loans: patron 1 -> book 1, patron 2 -> book 2, patron 3 -> book 5, patron 1 -> book 8, patron 4 -> book 12
        // Returned loans: patron 2 -> book 3 (returned on time), patron 5 -> book 6 (returned late)
        // Overdue loan: patron 4 -> book 4 (still active but past due), patron 2 -> book 8 (overdue)

        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-5) },
            new() { Id = 2, BookId = 2, PatronId = 2, LoanDate = now.AddDays(-10), DueDate = now.AddDays(4), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-10) },
            new() { Id = 3, BookId = 5, PatronId = 3, LoanDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-3) },
            new() { Id = 4, BookId = 8, PatronId = 1, LoanDate = now.AddDays(-7), DueDate = now.AddDays(14), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-7) },
            new() { Id = 5, BookId = 12, PatronId = 4, LoanDate = now.AddDays(-8), DueDate = now.AddDays(6), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-8) },
            // Returned loans
            new() { Id = 6, BookId = 3, PatronId = 2, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-18), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-30) },
            new() { Id = 7, BookId = 6, PatronId = 5, LoanDate = now.AddDays(-40), DueDate = now.AddDays(-19), ReturnDate = now.AddDays(-15), Status = LoanStatus.Returned, RenewalCount = 1, CreatedAt = now.AddDays(-40) },
            // Overdue loans (active but past due)
            new() { Id = 8, BookId = 4, PatronId = 4, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-20) },
            new() { Id = 9, BookId = 8, PatronId = 2, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-11), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-25) },
            new() { Id = 10, BookId = 10, PatronId = 3, LoanDate = now.AddDays(-12), DueDate = now.AddDays(-5), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-12) }
        };
        db.Loans.AddRange(loans);

        // --- Reservations ---
        var reservations = new List<Reservation>
        {
            // Pending reservation for I, Robot (all copies checked out)
            new() { Id = 1, BookId = 5, PatronId = 1, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-2) },
            new() { Id = 2, BookId = 5, PatronId = 4, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 2, CreatedAt = now.AddDays(-1) },
            // Ready reservation for Foundation (a copy was returned and patron notified)
            new() { Id = 3, BookId = 4, PatronId = 5, ReservationDate = now.AddDays(-5), ExpirationDate = now.AddDays(2), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now.AddDays(-5) }
        };
        db.Reservations.AddRange(reservations);

        // --- Fines ---
        var fines = new List<Fine>
        {
            // Unpaid fine for patron 5 (returned book 6 late: 4 days * $0.25 = $1.00)
            new() { Id = 1, PatronId = 5, LoanId = 7, Amount = 1.00m, Reason = "Overdue return - 4 day(s) late", IssuedDate = now.AddDays(-15), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-15) },
            // Paid fine for patron 2 (old fine)
            new() { Id = 2, PatronId = 2, LoanId = 6, Amount = 0.50m, Reason = "Overdue return - 2 day(s) late", IssuedDate = now.AddDays(-20), PaidDate = now.AddDays(-18), Status = FineStatus.Paid, CreatedAt = now.AddDays(-20) },
            // Unpaid fine for patron 4 (overdue book 4: 6 days * $0.25 = $1.50)
            new() { Id = 3, PatronId = 4, LoanId = 8, Amount = 1.50m, Reason = "Overdue return - 6 day(s) late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-1) },
            // Waived fine
            new() { Id = 4, PatronId = 3, LoanId = 10, Amount = 1.25m, Reason = "Overdue return - 5 day(s) late", IssuedDate = now.AddDays(-2), Status = FineStatus.Waived, CreatedAt = now.AddDays(-2) }
        };
        db.Fines.AddRange(fines);

        await db.SaveChangesAsync();
    }
}
