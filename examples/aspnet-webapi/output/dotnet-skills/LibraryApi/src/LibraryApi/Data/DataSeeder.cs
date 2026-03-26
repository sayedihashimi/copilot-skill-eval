using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(LibraryDbContext context)
    {
        if (await context.Authors.AnyAsync())
            return;

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her six major novels which critique the British landed gentry.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom" },
            new() { Id = 2, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for his works of science fiction and popular science.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States" },
            new() { Id = 3, FirstName = "Toni", LastName = "Morrison", Biography = "American novelist and Nobel Prize laureate known for her powerful exploration of African American identity.", BirthDate = new DateOnly(1931, 2, 18), Country = "United States" },
            new() { Id = 4, FirstName = "Haruki", LastName = "Murakami", Biography = "Japanese writer known for his surreal and melancholic style of fiction.", BirthDate = new DateOnly(1949, 1, 12), Country = "Japan" },
            new() { Id = 5, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli public intellectual, historian, and professor at the Hebrew University of Jerusalem.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel" },
            new() { Id = 6, FirstName = "Margaret", LastName = "Atwood", Biography = "Canadian poet, novelist, literary critic, essayist, and environmental activist.", BirthDate = new DateOnly(1939, 11, 18), Country = "Canada" }
        };
        context.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works created from imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction based on imagined future scientific or technological advances" },
            new() { Id = 3, Name = "History", Description = "Non-fiction works about past events" },
            new() { Id = 4, Name = "Science", Description = "Non-fiction works about the natural world" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of a person's life written by someone else" },
            new() { Id = 6, Name = "Classic Literature", Description = "Works of literature that have stood the test of time" }
        };
        context.Categories.AddRange(categories);

        // Books
        var now = DateTime.UtcNow;
        var books = new List<Book>
        {
            new() { Id = 1, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A classic novel of manners.", PageCount = 432, TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Beloved", ISBN = "978-1400033416", Publisher = "Vintage", PublicationYear = 1987, Description = "A powerful novel about the legacy of slavery.", PageCount = 324, TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "Norwegian Wood", ISBN = "978-0375704024", Publisher = "Vintage", PublicationYear = 1987, Description = "A nostalgic story of loss and sexuality.", PageCount = 296, TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A narrative history of humanity.", PageCount = 464, TotalCopies = 4, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "The Handmaid's Tale", ISBN = "978-0385490818", Publisher = "Anchor", PublicationYear = 1985, Description = "A dystopian novel set in a totalitarian society.", PageCount = 311, TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "I, Robot", ISBN = "978-0553294385", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of nine science fiction short stories.", PageCount = 224, TotalCopies = 1, AvailableCopies = 0, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about the Dashwood sisters.", PageCount = 409, TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "Kafka on the Shore", ISBN = "978-1400079278", Publisher = "Vintage", PublicationYear = 2002, Description = "A metaphysical novel by Murakami.", PageCount = 480, TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "21 Lessons for the 21st Century", ISBN = "978-0525512172", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "A meditation on the big questions of our time.", PageCount = 372, TotalCopies = 3, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "The Blind Assassin", ISBN = "978-0385720953", Publisher = "Anchor", PublicationYear = 2000, Description = "A Booker Prize-winning novel.", PageCount = 521, TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "Song of Solomon", ISBN = "978-1400033423", Publisher = "Vintage", PublicationYear = 1977, Description = "An epic novel spanning four generations.", PageCount = 337, TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now }
        };
        context.Books.AddRange(books);

        // BookAuthor join
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },   // Pride and Prejudice - Austen
            new() { BookId = 2, AuthorId = 2 },   // Foundation - Asimov
            new() { BookId = 3, AuthorId = 3 },   // Beloved - Morrison
            new() { BookId = 4, AuthorId = 4 },   // Norwegian Wood - Murakami
            new() { BookId = 5, AuthorId = 5 },   // Sapiens - Harari
            new() { BookId = 6, AuthorId = 6 },   // Handmaid's Tale - Atwood
            new() { BookId = 7, AuthorId = 2 },   // I, Robot - Asimov
            new() { BookId = 8, AuthorId = 1 },   // Sense and Sensibility - Austen
            new() { BookId = 9, AuthorId = 4 },   // Kafka on the Shore - Murakami
            new() { BookId = 10, AuthorId = 5 },  // 21 Lessons - Harari
            new() { BookId = 11, AuthorId = 6 },  // Blind Assassin - Atwood
            new() { BookId = 12, AuthorId = 3 },  // Song of Solomon - Morrison
        };
        context.BookAuthors.AddRange(bookAuthors);

        // BookCategory join
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 },  // Pride - Fiction
            new() { BookId = 1, CategoryId = 6 },  // Pride - Classic
            new() { BookId = 2, CategoryId = 2 },  // Foundation - Sci-Fi
            new() { BookId = 2, CategoryId = 1 },  // Foundation - Fiction
            new() { BookId = 3, CategoryId = 1 },  // Beloved - Fiction
            new() { BookId = 4, CategoryId = 1 },  // Norwegian Wood - Fiction
            new() { BookId = 5, CategoryId = 3 },  // Sapiens - History
            new() { BookId = 5, CategoryId = 4 },  // Sapiens - Science
            new() { BookId = 6, CategoryId = 1 },  // Handmaid's - Fiction
            new() { BookId = 6, CategoryId = 2 },  // Handmaid's - Sci-Fi
            new() { BookId = 7, CategoryId = 2 },  // I, Robot - Sci-Fi
            new() { BookId = 8, CategoryId = 1 },  // Sense - Fiction
            new() { BookId = 8, CategoryId = 6 },  // Sense - Classic
            new() { BookId = 9, CategoryId = 1 },  // Kafka - Fiction
            new() { BookId = 10, CategoryId = 3 }, // 21 Lessons - History
            new() { BookId = 11, CategoryId = 1 }, // Blind Assassin - Fiction
            new() { BookId = 12, CategoryId = 1 }, // Song of Solomon - Fiction
        };
        context.BookCategories.AddRange(bookCategories);

        // Patrons
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Elm Street", MembershipDate = new DateOnly(2024, 1, 15), MembershipType = MembershipType.Premium, IsActive = true },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Avenue", MembershipDate = new DateOnly(2024, 3, 20), MembershipType = MembershipType.Standard, IsActive = true },
            new() { Id = 3, FirstName = "Carol", LastName = "Davis", Email = "carol.davis@email.com", Phone = "555-0103", Address = "789 Pine Road", MembershipDate = new DateOnly(2024, 6, 1), MembershipType = MembershipType.Student, IsActive = true },
            new() { Id = 4, FirstName = "David", LastName = "Wilson", Email = "david.wilson@email.com", Phone = "555-0104", Address = "321 Maple Drive", MembershipDate = new DateOnly(2024, 2, 10), MembershipType = MembershipType.Standard, IsActive = true },
            new() { Id = 5, FirstName = "Eve", LastName = "Martinez", Email = "eve.martinez@email.com", Phone = "555-0105", Address = "654 Birch Lane", MembershipDate = new DateOnly(2024, 8, 5), MembershipType = MembershipType.Premium, IsActive = true },
            new() { Id = 6, FirstName = "Frank", LastName = "Taylor", Email = "frank.taylor@email.com", Phone = "555-0106", Address = "987 Cedar Court", MembershipDate = new DateOnly(2023, 11, 30), MembershipType = MembershipType.Standard, IsActive = false },
            new() { Id = 7, FirstName = "Grace", LastName = "Anderson", Email = "grace.anderson@email.com", Phone = "555-0107", Address = "147 Walnut Blvd", MembershipDate = new DateOnly(2024, 5, 12), MembershipType = MembershipType.Student, IsActive = true }
        };
        context.Patrons.AddRange(patrons);

        // Loans (match AvailableCopies)
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, CreatedAt = now.AddDays(-10) },
            new() { Id = 2, BookId = 2, PatronId = 2, LoanDate = now.AddDays(-7), DueDate = now.AddDays(7), Status = LoanStatus.Active, CreatedAt = now.AddDays(-7) },
            new() { Id = 3, BookId = 4, PatronId = 3, LoanDate = now.AddDays(-5), DueDate = now.AddDays(2), Status = LoanStatus.Active, CreatedAt = now.AddDays(-5) },
            new() { Id = 4, BookId = 7, PatronId = 1, LoanDate = now.AddDays(-15), DueDate = now.AddDays(6), Status = LoanStatus.Active, CreatedAt = now.AddDays(-15) },
            new() { Id = 5, BookId = 5, PatronId = 4, LoanDate = now.AddDays(-12), DueDate = now.AddDays(2), Status = LoanStatus.Active, CreatedAt = now.AddDays(-12) },
            new() { Id = 6, BookId = 9, PatronId = 5, LoanDate = now.AddDays(-18), DueDate = now.AddDays(3), Status = LoanStatus.Active, CreatedAt = now.AddDays(-18) },
            new() { Id = 7, BookId = 6, PatronId = 2, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue, CreatedAt = now.AddDays(-20) },
            // Returned loans
            new() { Id = 8, BookId = 3, PatronId = 1, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-9), ReturnDate = now.AddDays(-12), Status = LoanStatus.Returned, CreatedAt = now.AddDays(-30) },
            new() { Id = 9, BookId = 1, PatronId = 4, LoanDate = now.AddDays(-45), DueDate = now.AddDays(-31), ReturnDate = now.AddDays(-28), Status = LoanStatus.Returned, CreatedAt = now.AddDays(-45) },
            // Overdue (returned late - for fine generation)
            new() { Id = 10, BookId = 5, PatronId = 3, LoanDate = now.AddDays(-40), DueDate = now.AddDays(-33), ReturnDate = now.AddDays(-25), Status = LoanStatus.Returned, CreatedAt = now.AddDays(-40) }
        };
        context.Loans.AddRange(loans);

        // Reservations
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 7, PatronId = 3, ReservationDate = now.AddDays(-3), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-3) },
            new() { Id = 2, BookId = 7, PatronId = 5, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 2, CreatedAt = now.AddDays(-2) },
            new() { Id = 3, BookId = 2, PatronId = 4, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-1) },
            new() { Id = 4, BookId = 6, PatronId = 1, ReservationDate = now.AddDays(-4), ExpirationDate = now.AddDays(2), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now.AddDays(-4) }
        };
        context.Reservations.AddRange(reservations);

        // Fines
        var fines = new List<Fine>
        {
            // Unpaid fine for patron 3 (returned 8 days late: $0.25 * 8 = $2.00)
            new() { Id = 1, PatronId = 3, LoanId = 10, Amount = 2.00m, Reason = "Overdue return - 8 days late", IssuedDate = now.AddDays(-25), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-25) },
            // Paid fine for patron 4 (returned 3 days late: $0.25 * 3 = $0.75)
            new() { Id = 2, PatronId = 4, LoanId = 9, Amount = 0.75m, Reason = "Overdue return - 3 days late", IssuedDate = now.AddDays(-28), PaidDate = now.AddDays(-26), Status = FineStatus.Paid, CreatedAt = now.AddDays(-28) },
            // Large unpaid fine for patron 2 (to test fine threshold blocking)
            new() { Id = 3, PatronId = 2, LoanId = 8, Amount = 12.50m, Reason = "Damaged book", IssuedDate = now.AddDays(-10), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-10) }
        };
        context.Fines.AddRange(fines);

        await context.SaveChangesAsync();
    }
}
