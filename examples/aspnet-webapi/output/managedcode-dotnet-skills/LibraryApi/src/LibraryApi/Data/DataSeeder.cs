using LibraryApi.Models;

namespace LibraryApi.Data;

public static class DataSeeder
{
    public static void Seed(LibraryDbContext db)
    {
        if (db.Authors.Any()) return; // Only seed if database is empty

        var now = DateTime.UtcNow;

        // --- Authors ---
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, known for '1984' and 'Animal Farm'.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom", CreatedAt = now },
            new() { Id = 2, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her six major novels including 'Pride and Prejudice'.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = now },
            new() { Id = 3, FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for science fiction and popular science.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = now },
            new() { Id = 4, FirstName = "Toni", LastName = "Morrison", Biography = "American novelist and Nobel Prize winner, known for 'Beloved' and 'Song of Solomon'.", BirthDate = new DateOnly(1931, 2, 18), Country = "United States", CreatedAt = now },
            new() { Id = 5, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and author of 'Sapiens' and 'Homo Deus'.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel", CreatedAt = now },
            new() { Id = 6, FirstName = "Margaret", LastName = "Atwood", Biography = "Canadian poet, novelist, and literary critic. Author of 'The Handmaid\u0027s Tale'.", BirthDate = new DateOnly(1939, 11, 18), Country = "Canada", CreatedAt = now },
        };
        db.Authors.AddRange(authors);

        // --- Categories ---
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Narrative works of imagination" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science and technology" },
            new() { Id = 3, Name = "History", Description = "Non-fiction works about historical events and periods" },
            new() { Id = 4, Name = "Science", Description = "Non-fiction works about scientific topics" },
            new() { Id = 5, Name = "Biography", Description = "Non-fiction accounts of a person's life" },
            new() { Id = 6, Name = "Dystopian", Description = "Fiction set in oppressive or degraded societies" },
        };
        db.Categories.AddRange(categories);

        // --- Books (12+) ---
        var books = new List<Book>
        {
            new() { Id = 1, Title = "1984", ISBN = "978-0451524935", Publisher = "Signet Classics", PublicationYear = 1949, Description = "A dystopian novel set in a totalitarian society.", PageCount = 328, Language = "English", TotalCopies = 3, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "Animal Farm", ISBN = "978-0451526342", Publisher = "Signet Classics", PublicationYear = 1945, Description = "An allegorical novella about a group of farm animals.", PageCount = 112, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about the Dashwood sisters.", PageCount = 409, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "I, Robot", ISBN = "978-0553294385", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of robot short stories.", PageCount = 224, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "Beloved", ISBN = "978-1400033416", Publisher = "Vintage", PublicationYear = 1987, Description = "A novel inspired by the story of Margaret Garner.", PageCount = 324, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "Song of Solomon", ISBN = "978-1400033423", Publisher = "Vintage", PublicationYear = 1977, Description = "A novel following Macon Dead III (Milkman).", PageCount = 337, Language = "English", TotalCopies = 1, AvailableCopies = 0, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A survey of the history of humankind.", PageCount = 464, Language = "English", TotalCopies = 4, AvailableCopies = 4, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0062464316", Publisher = "Harper", PublicationYear = 2017, Description = "Examines possible futures for humanity.", PageCount = 464, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "The Handmaid's Tale", ISBN = "978-0385490818", Publisher = "Anchor Books", PublicationYear = 1985, Description = "A dystopian novel set in a near-future New England.", PageCount = 311, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "Oryx and Crake", ISBN = "978-0385721677", Publisher = "Anchor Books", PublicationYear = 2003, Description = "A speculative fiction novel about a post-apocalyptic world.", PageCount = 374, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
        };
        db.Books.AddRange(books);

        // --- BookAuthors ---
        db.BookAuthors.AddRange(
            new BookAuthor { BookId = 1, AuthorId = 1 },
            new BookAuthor { BookId = 2, AuthorId = 1 },
            new BookAuthor { BookId = 3, AuthorId = 2 },
            new BookAuthor { BookId = 4, AuthorId = 2 },
            new BookAuthor { BookId = 5, AuthorId = 3 },
            new BookAuthor { BookId = 6, AuthorId = 3 },
            new BookAuthor { BookId = 7, AuthorId = 4 },
            new BookAuthor { BookId = 8, AuthorId = 4 },
            new BookAuthor { BookId = 9, AuthorId = 5 },
            new BookAuthor { BookId = 10, AuthorId = 5 },
            new BookAuthor { BookId = 11, AuthorId = 6 },
            new BookAuthor { BookId = 12, AuthorId = 6 }
        );

        // --- BookCategories ---
        db.BookCategories.AddRange(
            new BookCategory { BookId = 1, CategoryId = 1 },
            new BookCategory { BookId = 1, CategoryId = 6 },
            new BookCategory { BookId = 2, CategoryId = 1 },
            new BookCategory { BookId = 3, CategoryId = 1 },
            new BookCategory { BookId = 4, CategoryId = 1 },
            new BookCategory { BookId = 5, CategoryId = 2 },
            new BookCategory { BookId = 6, CategoryId = 2 },
            new BookCategory { BookId = 7, CategoryId = 1 },
            new BookCategory { BookId = 8, CategoryId = 1 },
            new BookCategory { BookId = 9, CategoryId = 3 },
            new BookCategory { BookId = 9, CategoryId = 4 },
            new BookCategory { BookId = 10, CategoryId = 3 },
            new BookCategory { BookId = 10, CategoryId = 4 },
            new BookCategory { BookId = 11, CategoryId = 1 },
            new BookCategory { BookId = 11, CategoryId = 6 },
            new BookCategory { BookId = 12, CategoryId = 2 }
        );

        // --- Patrons (6+) ---
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak St", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Elm St", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Carol", LastName = "Davis", Email = "carol.davis@email.com", Phone = "555-0103", Address = "789 Pine St", MembershipDate = new DateOnly(2023, 6, 1), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "David", LastName = "Wilson", Email = "david.wilson@email.com", Phone = "555-0104", Address = "101 Maple Ave", MembershipDate = new DateOnly(2023, 9, 10), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Eva", LastName = "Martinez", Email = "eva.martinez@email.com", Phone = "555-0105", Address = "202 Cedar Ln", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Frank", LastName = "Brown", Email = "frank.brown@email.com", Phone = "555-0106", Address = "303 Birch Dr", MembershipDate = new DateOnly(2023, 5, 15), MembershipType = MembershipType.Standard, IsActive = false, CreatedAt = now, UpdatedAt = now },
        };
        db.Patrons.AddRange(patrons);

        // --- Loans (8+) ---
        // Active loans: 1 (1984, Alice), 3 (P&P, Bob), 5 (Foundation, Carol), 6 (I Robot, Alice), 8 (Song of Solomon, David)
        // Returned: 9 (Sapiens, Eva) - returned on time, 2 (Animal Farm, Bob) - returned late (overdue fine)
        // Overdue: 11 (Handmaid's Tale, Carol)
        var loans = new List<Loan>
        {
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            new() { Id = 2, BookId = 1, PatronId = 2, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), ReturnDate = now.AddDays(-3), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now },
            new() { Id = 3, BookId = 3, PatronId = 2, LoanDate = now.AddDays(-7), DueDate = now.AddDays(7), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            new() { Id = 4, BookId = 5, PatronId = 3, LoanDate = now.AddDays(-5), DueDate = now.AddDays(2), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            new() { Id = 5, BookId = 6, PatronId = 1, LoanDate = now.AddDays(-15), DueDate = now.AddDays(6), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },
            new() { Id = 6, BookId = 8, PatronId = 4, LoanDate = now.AddDays(-18), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now },
            new() { Id = 7, BookId = 9, PatronId = 5, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-9), ReturnDate = now.AddDays(-10), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now },
            new() { Id = 8, BookId = 11, PatronId = 3, LoanDate = now.AddDays(-12), DueDate = now.AddDays(-5), Status = LoanStatus.Overdue, RenewalCount = 1, CreatedAt = now },
        };
        db.Loans.AddRange(loans);

        // AvailableCopies adjustments already accounted for in book definitions above:
        // Book 1 (1984): TotalCopies=3, ActiveLoans=1(loan1), AvailableCopies should be 1 (loan2 returned) => adjusted above: 1
        // Book 3 (P&P): TotalCopies=2, ActiveLoans=1(loan3), AvailableCopies=1
        // Book 5 (Foundation): TotalCopies=3, ActiveLoans=1(loan4), AvailableCopies=2
        // Book 6 (I Robot): TotalCopies=2, ActiveLoans=1(loan5), AvailableCopies=1
        // Book 8 (Song of Solomon): TotalCopies=1, ActiveLoans=1(loan6, overdue), AvailableCopies=0
        // Book 9 (Sapiens): TotalCopies=4, ActiveLoans=0(loan7 returned), AvailableCopies=3 (was 4, loan7 returned so 3+1-1=3... wait, returned so back to full minus active)
        // Actually loan7 is returned so AvailableCopies should be 4. Let me fix: Sapiens has 4 total, 0 active = 4 available.
        // Book 11 (Handmaid's Tale): TotalCopies=3, ActiveLoans=1(loan8, overdue), AvailableCopies=2

        // --- Reservations (3+) ---
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 8, PatronId = 5, ReservationDate = now.AddDays(-3), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now },
            new() { Id = 2, BookId = 8, PatronId = 1, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 2, CreatedAt = now },
            new() { Id = 3, BookId = 1, PatronId = 3, ReservationDate = now.AddDays(-2), ExpirationDate = now.AddDays(1), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now },
        };
        db.Reservations.AddRange(reservations);

        // --- Fines (3+) ---
        // Loan 2 was returned 3 days late ($0.75), Loan 6 is overdue
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 2, LoanId = 2, Amount = 0.75m, Reason = "Overdue return - 3 day(s) late", IssuedDate = now.AddDays(-3), Status = FineStatus.Unpaid, CreatedAt = now },
            new() { Id = 2, PatronId = 4, LoanId = 6, Amount = 1.00m, Reason = "Overdue return - 4 day(s) late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid, CreatedAt = now },
            new() { Id = 3, PatronId = 5, LoanId = 7, Amount = 0.50m, Reason = "Overdue return - 2 day(s) late", IssuedDate = now.AddDays(-10), PaidDate = now.AddDays(-8), Status = FineStatus.Paid, CreatedAt = now },
        };
        db.Fines.AddRange(fines);

        db.SaveChanges();
    }
}
