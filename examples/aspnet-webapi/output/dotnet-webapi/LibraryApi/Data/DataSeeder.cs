using LibraryApi.Models;

namespace LibraryApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(LibraryDbContext db)
    {
        if (db.Authors.Any())
            return;

        var now = DateTime.UtcNow;

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her wit and social commentary. Her works include Pride and Prejudice and Sense and Sensibility.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = now },
            new() { Id = 2, FirstName = "Isaac", LastName = "Asimov", Biography = "American author and professor of biochemistry, known for his works of science fiction and popular science.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = now },
            new() { Id = 3, FirstName = "Gabriel", LastName = "García Márquez", Biography = "Colombian novelist and Nobel Prize laureate, known for magical realism.", BirthDate = new DateOnly(1927, 3, 6), Country = "Colombia", CreatedAt = now },
            new() { Id = 4, FirstName = "Toni", LastName = "Morrison", Biography = "American novelist and Nobel Prize laureate, known for works exploring African-American identity.", BirthDate = new DateOnly(1931, 2, 18), Country = "United States", CreatedAt = now },
            new() { Id = 5, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and professor, author of Sapiens and Homo Deus.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel", CreatedAt = now },
            new() { Id = 6, FirstName = "Mary", LastName = "Shelley", Biography = "English novelist who wrote Frankenstein, considered one of the earliest science fiction novels.", BirthDate = new DateOnly(1797, 8, 30), Country = "United Kingdom", CreatedAt = now }
        };
        db.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Narrative works of imagination, including novels and short stories" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction dealing with futuristic science, technology, and space exploration" },
            new() { Id = 3, Name = "History", Description = "Books about historical events, periods, and figures" },
            new() { Id = 4, Name = "Science", Description = "Books about scientific topics and discoveries" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of a person's life written by another" },
            new() { Id = 6, Name = "Classic Literature", Description = "Timeless works of literary merit" }
        };
        db.Categories.AddRange(categories);

        // Books (12 books, varied copies)
        var books = new List<Book>
        {
            new() { Id = 1, Title = "Pride and Prejudice", ISBN = "978-0-14-143951-8", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A romantic novel that also serves as a social commentary on the British landed gentry.", PageCount = 432, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "Sense and Sensibility", ISBN = "978-0-14-143966-2", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A novel about the Dashwood sisters and their romantic interests.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Foundation", ISBN = "978-0-553-29335-7", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series about the fall of a Galactic Empire.", PageCount = 244, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "I, Robot", ISBN = "978-0-553-29438-5", Publisher = "Bantam Books", PublicationYear = 1950, Description = "A collection of robot stories exploring the Three Laws of Robotics.", PageCount = 224, Language = "English", TotalCopies = 1, AvailableCopies = 0, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "One Hundred Years of Solitude", ISBN = "978-0-06-088328-7", Publisher = "Harper Perennial", PublicationYear = 1967, Description = "The multi-generational story of the Buendía family in the fictional town of Macondo.", PageCount = 417, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "Love in the Time of Cholera", ISBN = "978-0-14-024990-6", Publisher = "Penguin Books", PublicationYear = 1985, Description = "A love story spanning over fifty years.", PageCount = 368, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "Beloved", ISBN = "978-1-4000-3341-6", Publisher = "Vintage", PublicationYear = 1987, Description = "A powerful novel about an escaped enslaved woman haunted by her past.", PageCount = 324, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "Song of Solomon", ISBN = "978-1-4000-3342-3", Publisher = "Vintage", PublicationYear = 1977, Description = "A novel about an African-American man's search for identity.", PageCount = 337, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0-06-231609-7", Publisher = "Harper", PublicationYear = 2015, Description = "A sweeping history of the human species from the Stone Age to the present.", PageCount = 498, Language = "English", TotalCopies = 4, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "Homo Deus: A Brief History of Tomorrow", ISBN = "978-0-06-246431-6", Publisher = "Harper", PublicationYear = 2017, Description = "A look at how humans might evolve in the future.", PageCount = 464, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "Frankenstein", ISBN = "978-0-14-143947-1", Publisher = "Penguin Classics", PublicationYear = 1818, Description = "The story of Victor Frankenstein and his monstrous creation.", PageCount = 280, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "The Last Man", ISBN = "978-0-19-955578-3", Publisher = "Oxford University Press", PublicationYear = 1826, Description = "A post-apocalyptic science fiction novel set at the end of the 21st century.", PageCount = 480, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now }
        };
        db.Books.AddRange(books);

        // BookAuthors
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },  // Pride and Prejudice - Austen
            new() { BookId = 2, AuthorId = 1 },  // Sense and Sensibility - Austen
            new() { BookId = 3, AuthorId = 2 },  // Foundation - Asimov
            new() { BookId = 4, AuthorId = 2 },  // I, Robot - Asimov
            new() { BookId = 5, AuthorId = 3 },  // One Hundred Years - García Márquez
            new() { BookId = 6, AuthorId = 3 },  // Love in the Time - García Márquez
            new() { BookId = 7, AuthorId = 4 },  // Beloved - Morrison
            new() { BookId = 8, AuthorId = 4 },  // Song of Solomon - Morrison
            new() { BookId = 9, AuthorId = 5 },  // Sapiens - Harari
            new() { BookId = 10, AuthorId = 5 }, // Homo Deus - Harari
            new() { BookId = 11, AuthorId = 6 }, // Frankenstein - Shelley
            new() { BookId = 12, AuthorId = 6 }  // The Last Man - Shelley
        };
        db.BookAuthors.AddRange(bookAuthors);

        // BookCategories
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 },  // Pride and Prejudice - Fiction
            new() { BookId = 1, CategoryId = 6 },  // Pride and Prejudice - Classic Literature
            new() { BookId = 2, CategoryId = 1 },  // Sense and Sensibility - Fiction
            new() { BookId = 2, CategoryId = 6 },  // Sense and Sensibility - Classic Literature
            new() { BookId = 3, CategoryId = 2 },  // Foundation - Science Fiction
            new() { BookId = 4, CategoryId = 2 },  // I, Robot - Science Fiction
            new() { BookId = 5, CategoryId = 1 },  // One Hundred Years - Fiction
            new() { BookId = 5, CategoryId = 6 },  // One Hundred Years - Classic Literature
            new() { BookId = 6, CategoryId = 1 },  // Love in the Time - Fiction
            new() { BookId = 7, CategoryId = 1 },  // Beloved - Fiction
            new() { BookId = 7, CategoryId = 6 },  // Beloved - Classic Literature
            new() { BookId = 8, CategoryId = 1 },  // Song of Solomon - Fiction
            new() { BookId = 9, CategoryId = 3 },  // Sapiens - History
            new() { BookId = 9, CategoryId = 4 },  // Sapiens - Science
            new() { BookId = 10, CategoryId = 4 }, // Homo Deus - Science
            new() { BookId = 10, CategoryId = 3 }, // Homo Deus - History
            new() { BookId = 11, CategoryId = 2 }, // Frankenstein - Science Fiction
            new() { BookId = 11, CategoryId = 6 }, // Frankenstein - Classic Literature
            new() { BookId = 12, CategoryId = 2 }, // The Last Man - Science Fiction
        };
        db.BookCategories.AddRange(bookCategories);

        // Patrons (6 patrons: mix of membership types, one inactive)
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street, Springfield", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Williams", Email = "bob.williams@email.com", Phone = "555-0102", Address = "456 Maple Ave, Springfield", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Carol", LastName = "Davis", Email = "carol.davis@email.com", Phone = "555-0103", Address = "789 Pine Road, Springfield", MembershipDate = new DateOnly(2023, 6, 1), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "David", LastName = "Martinez", Email = "david.martinez@email.com", Phone = "555-0104", Address = "321 Elm Street, Springfield", MembershipDate = new DateOnly(2023, 9, 10), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Emily", LastName = "Brown", Email = "emily.brown@email.com", Phone = "555-0105", Address = "654 Cedar Lane, Springfield", MembershipDate = new DateOnly(2024, 1, 5), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Frank", LastName = "Wilson", Email = "frank.wilson@email.com", Phone = "555-0106", Address = "987 Birch Blvd, Springfield", MembershipDate = new DateOnly(2022, 11, 20), MembershipType = MembershipType.Standard, IsActive = false, CreatedAt = now, UpdatedAt = now }
        };
        db.Patrons.AddRange(patrons);

        // Loans (8 loans in various states)
        // Active loans: reduce AvailableCopies accordingly (already reflected in books above)
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },   // Pride and Prejudice - Alice (Premium, 21 days)
            new() { Id = 2, BookId = 3, PatronId = 2, LoanDate = now.AddDays(-10), DueDate = now.AddDays(4), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },    // Foundation - Bob (Standard, 14 days)
            new() { Id = 3, BookId = 4, PatronId = 3, LoanDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },     // I, Robot - Carol (Student, 7 days)
            new() { Id = 4, BookId = 7, PatronId = 1, LoanDate = now.AddDays(-2), DueDate = now.AddDays(19), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now },    // Beloved - Alice

            // Overdue loans
            new() { Id = 5, BookId = 5, PatronId = 4, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now },  // One Hundred Years - David (14 days, 6 days overdue)
            new() { Id = 6, BookId = 9, PatronId = 2, LoanDate = now.AddDays(-18), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue, RenewalCount = 1, CreatedAt = now },  // Sapiens - Bob (14 days, 4 days overdue, 1 renewal)
            new() { Id = 7, BookId = 11, PatronId = 5, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now }, // Frankenstein - Emily

            // Returned loans
            new() { Id = 8, BookId = 6, PatronId = 1, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-9), ReturnDate = now.AddDays(-12), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now } // Love in the Time - Alice (returned on time)
        };
        db.Loans.AddRange(loans);

        // Reservations
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 4, PatronId = 4, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now },     // I, Robot - David (pending)
            new() { Id = 2, BookId = 4, PatronId = 5, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 2, CreatedAt = now },     // I, Robot - Emily (pending, 2nd in queue)
            new() { Id = 3, BookId = 3, PatronId = 1, ReservationDate = now.AddDays(-3), ExpirationDate = now.AddDays(1), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now } // Foundation - Alice (ready to pick up)
        };
        db.Reservations.AddRange(reservations);

        // Fines
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 4, LoanId = 5, Amount = 1.50m, Reason = "Overdue return - 6 day(s) late", IssuedDate = now, Status = FineStatus.Unpaid, CreatedAt = now },
            new() { Id = 2, PatronId = 2, LoanId = 6, Amount = 1.00m, Reason = "Overdue return - 4 day(s) late", IssuedDate = now, Status = FineStatus.Unpaid, CreatedAt = now },
            new() { Id = 3, PatronId = 1, LoanId = 8, Amount = 0.75m, Reason = "Overdue return - 3 day(s) late", IssuedDate = now.AddDays(-12), PaidDate = now.AddDays(-10), Status = FineStatus.Paid, CreatedAt = now }
        };
        db.Fines.AddRange(fines);

        await db.SaveChangesAsync();
    }
}
