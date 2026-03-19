using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(LibraryDbContext db)
    {
        if (await db.Authors.AnyAsync())
            return;

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, best known for Animal Farm and Nineteen Eighty-Four.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom" },
            new() { Id = 2, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her social commentary and romantic fiction.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom" },
            new() { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, prolific science fiction author.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States" },
            new() { Id = 4, FirstName = "Toni", LastName = "Morrison", Biography = "American novelist and Nobel Prize in Literature laureate.", BirthDate = new DateOnly(1931, 2, 18), Country = "United States" },
            new() { Id = 5, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and professor, author of Sapiens and Homo Deus.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel" },
            new() { Id = 6, FirstName = "Mary", LastName = "Shelley", Biography = "English novelist who wrote Frankenstein, considered the first science fiction novel.", BirthDate = new DateOnly(1797, 8, 30), Country = "United Kingdom" },
        };

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works of the imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction based on scientific discoveries or advanced technology" },
            new() { Id = 3, Name = "History", Description = "Books about historical events and periods" },
            new() { Id = 4, Name = "Science", Description = "Books about scientific topics and discoveries" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of a person's life written by someone else" },
            new() { Id = 6, Name = "Classic Literature", Description = "Enduring works of literary fiction" },
        };

        // Books
        var books = new List<Book>
        {
            new() { Id = 1, Title = "Nineteen Eighty-Four", ISBN = "978-0451524935", Publisher = "Signet Classics", PublicationYear = 1949, Description = "A dystopian novel set in a totalitarian society.", PageCount = 328, Language = "English", TotalCopies = 4, AvailableCopies = 2 },
            new() { Id = 2, Title = "Animal Farm", ISBN = "978-0451526342", Publisher = "Signet Classics", PublicationYear = 1945, Description = "A satirical allegory of Soviet totalitarianism.", PageCount = 112, Language = "English", TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 3, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, Language = "English", TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 4, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A story of two sisters and their romantic entanglements.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 5, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 3, AvailableCopies = 1 },
            new() { Id = 6, Title = "I, Robot", ISBN = "978-0553382563", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of robot short stories.", PageCount = 224, Language = "English", TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 7, Title = "Beloved", ISBN = "978-1400033416", Publisher = "Vintage", PublicationYear = 1987, Description = "A novel inspired by the story of Margaret Garner.", PageCount = 324, Language = "English", TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 8, Title = "Song of Solomon", ISBN = "978-1400033423", Publisher = "Vintage", PublicationYear = 1977, Description = "A coming-of-age story with African American themes.", PageCount = 337, Language = "English", TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 9, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A survey of the history of humankind.", PageCount = 464, Language = "English", TotalCopies = 5, AvailableCopies = 3 },
            new() { Id = 10, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0062464316", Publisher = "Harper", PublicationYear = 2017, Description = "A look at the future of humankind.", PageCount = 464, Language = "English", TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 11, Title = "Frankenstein", ISBN = "978-0486282114", Publisher = "Dover Publications", PublicationYear = 1818, Description = "The classic tale of a scientist who creates a living creature.", PageCount = 166, Language = "English", TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 12, Title = "21 Lessons for the 21st Century", ISBN = "978-0525512172", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "Explores the big questions of our time.", PageCount = 372, Language = "English", TotalCopies = 2, AvailableCopies = 2 },
        };

        // Book-Author relationships
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },
            new() { BookId = 2, AuthorId = 1 },
            new() { BookId = 3, AuthorId = 2 },
            new() { BookId = 4, AuthorId = 2 },
            new() { BookId = 5, AuthorId = 3 },
            new() { BookId = 6, AuthorId = 3 },
            new() { BookId = 7, AuthorId = 4 },
            new() { BookId = 8, AuthorId = 4 },
            new() { BookId = 9, AuthorId = 5 },
            new() { BookId = 10, AuthorId = 5 },
            new() { BookId = 11, AuthorId = 6 },
            new() { BookId = 12, AuthorId = 5 },
        };

        // Book-Category relationships
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 }, new() { BookId = 1, CategoryId = 2 }, new() { BookId = 1, CategoryId = 6 },
            new() { BookId = 2, CategoryId = 1 }, new() { BookId = 2, CategoryId = 6 },
            new() { BookId = 3, CategoryId = 1 }, new() { BookId = 3, CategoryId = 6 },
            new() { BookId = 4, CategoryId = 1 }, new() { BookId = 4, CategoryId = 6 },
            new() { BookId = 5, CategoryId = 2 }, new() { BookId = 5, CategoryId = 1 },
            new() { BookId = 6, CategoryId = 2 },
            new() { BookId = 7, CategoryId = 1 },
            new() { BookId = 8, CategoryId = 1 },
            new() { BookId = 9, CategoryId = 3 }, new() { BookId = 9, CategoryId = 4 },
            new() { BookId = 10, CategoryId = 3 }, new() { BookId = 10, CategoryId = 4 },
            new() { BookId = 11, CategoryId = 1 }, new() { BookId = 11, CategoryId = 2 }, new() { BookId = 11, CategoryId = 6 },
            new() { BookId = 12, CategoryId = 3 },
        };

        // Patrons
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Maple St", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true },
            new() { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Rd", MembershipDate = new DateOnly(2023, 6, 1), MembershipType = MembershipType.Student, IsActive = true },
            new() { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Elm Dr", MembershipDate = new DateOnly(2023, 9, 10), MembershipType = MembershipType.Standard, IsActive = true },
            new() { Id = 5, FirstName = "Eva", LastName = "Martinez", Email = "eva.martinez@email.com", Phone = "555-0105", Address = "654 Cedar Ln", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Premium, IsActive = true },
            new() { Id = 6, FirstName = "Frank", LastName = "Davis", Email = "frank.davis@email.com", Phone = "555-0106", Address = "987 Birch Ct", MembershipDate = new DateOnly(2024, 2, 28), MembershipType = MembershipType.Student, IsActive = false },
        };

        var now = DateTime.UtcNow;

        // Loans - mix of active, returned, and overdue
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active },
            new() { Id = 2, BookId = 5, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active },
            new() { Id = 3, BookId = 6, PatronId = 2, LoanDate = now.AddDays(-7), DueDate = now.AddDays(7), Status = LoanStatus.Active },
            new() { Id = 4, BookId = 9, PatronId = 3, LoanDate = now.AddDays(-5), DueDate = now.AddDays(2), Status = LoanStatus.Active },

            // Returned loans
            new() { Id = 5, BookId = 3, PatronId = 2, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-18), Status = LoanStatus.Returned },
            new() { Id = 6, BookId = 7, PatronId = 4, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-11), ReturnDate = now.AddDays(-12), Status = LoanStatus.Returned },

            // Overdue loans
            new() { Id = 7, BookId = 1, PatronId = 4, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue },
            new() { Id = 8, BookId = 9, PatronId = 5, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue },
        };

        // Reservations
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 5, PatronId = 3, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 2, BookId = 1, PatronId = 2, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 3, BookId = 9, PatronId = 4, ReservationDate = now.AddDays(-3), Status = ReservationStatus.Pending, QueuePosition = 1 },
        };

        // Fines
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 4, LoanId = 7, Amount = 1.50m, Reason = "Overdue return", IssuedDate = now.AddDays(-5), Status = FineStatus.Unpaid },
            new() { Id = 2, PatronId = 5, LoanId = 8, Amount = 1.00m, Reason = "Overdue return", IssuedDate = now.AddDays(-3), Status = FineStatus.Unpaid },
            new() { Id = 3, PatronId = 2, LoanId = 5, Amount = 0.50m, Reason = "Overdue return", IssuedDate = now.AddDays(-18), PaidDate = now.AddDays(-15), Status = FineStatus.Paid },
        };

        db.Authors.AddRange(authors);
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        db.Books.AddRange(books);
        await db.SaveChangesAsync();

        db.BookAuthors.AddRange(bookAuthors);
        db.BookCategories.AddRange(bookCategories);
        db.Patrons.AddRange(patrons);
        await db.SaveChangesAsync();

        db.Loans.AddRange(loans);
        db.Reservations.AddRange(reservations);
        await db.SaveChangesAsync();

        db.Fines.AddRange(fines);
        await db.SaveChangesAsync();
    }
}
