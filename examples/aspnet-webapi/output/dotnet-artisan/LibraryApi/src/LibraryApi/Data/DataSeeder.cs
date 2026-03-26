using LibraryApi.Models;

namespace LibraryApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(LibraryDbContext db)
    {
        if (await db.Authors.AnyAsync())
        {
            return; // Already seeded
        }

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "Harper", LastName = "Lee", Biography = "American novelist known for 'To Kill a Mockingbird'.", BirthDate = new DateOnly(1926, 4, 28), Country = "United States" },
            new() { Id = 2, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist, known for '1984' and 'Animal Farm'.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom" },
            new() { Id = 3, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her social commentary and wit.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom" },
            new() { Id = 4, FirstName = "Isaac", LastName = "Asimov", Biography = "American author and professor of biochemistry, prolific science fiction writer.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States" },
            new() { Id = 5, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and professor, author of 'Sapiens'.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel" },
            new() { Id = 6, FirstName = "Agatha", LastName = "Christie", Biography = "English writer known as the 'Queen of Mystery'.", BirthDate = new DateOnly(1890, 9, 15), Country = "United Kingdom" }
        };
        db.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Novels, short stories, and literary fiction" },
            new() { Id = 2, Name = "Science Fiction", Description = "Speculative fiction dealing with futuristic concepts" },
            new() { Id = 3, Name = "History", Description = "Books about historical events and periods" },
            new() { Id = 4, Name = "Science", Description = "Scientific topics and discoveries" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of people's lives" },
            new() { Id = 6, Name = "Mystery", Description = "Crime fiction and detective stories" },
            new() { Id = 7, Name = "Classic Literature", Description = "Enduring literary masterpieces" }
        };
        db.Categories.AddRange(categories);

        // Books
        var books = new List<Book>
        {
            new() { Id = 1, Title = "To Kill a Mockingbird", ISBN = "978-0-06-112008-4", Publisher = "J.B. Lippincott & Co.", PublicationYear = 1960, Description = "A novel about racial injustice in the Deep South.", PageCount = 281, TotalCopies = 3, AvailableCopies = 1 },
            new() { Id = 2, Title = "1984", ISBN = "978-0-452-28423-4", Publisher = "Secker & Warburg", PublicationYear = 1949, Description = "A dystopian novel set in a totalitarian society.", PageCount = 328, TotalCopies = 4, AvailableCopies = 2 },
            new() { Id = 3, Title = "Pride and Prejudice", ISBN = "978-0-14-143951-8", Publisher = "T. Egerton", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 4, Title = "Foundation", ISBN = "978-0-553-29335-7", Publisher = "Gnome Press", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 5, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0-06-231609-7", Publisher = "Harper", PublicationYear = 2011, Description = "A narrative history of humankind.", PageCount = 443, TotalCopies = 3, AvailableCopies = 2 },
            new() { Id = 6, Title = "Animal Farm", ISBN = "978-0-451-52634-2", Publisher = "Secker & Warburg", PublicationYear = 1945, Description = "An allegorical novella about Stalinism.", PageCount = 112, TotalCopies = 3, AvailableCopies = 3 },
            new() { Id = 7, Title = "Murder on the Orient Express", ISBN = "978-0-06-269366-2", Publisher = "Collins Crime Club", PublicationYear = 1934, Description = "A Hercule Poirot mystery novel.", PageCount = 256, TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 8, Title = "I, Robot", ISBN = "978-0-553-29438-5", Publisher = "Gnome Press", PublicationYear = 1950, Description = "A collection of robot science fiction short stories.", PageCount = 253, TotalCopies = 1, AvailableCopies = 0 },
            new() { Id = 9, Title = "Sense and Sensibility", ISBN = "978-0-14-143966-2", Publisher = "Thomas Egerton", PublicationYear = 1811, Description = "A novel about the Dashwood sisters.", PageCount = 409, TotalCopies = 2, AvailableCopies = 2 },
            new() { Id = 10, Title = "The Caves of Steel", ISBN = "978-0-553-29340-1", Publisher = "Doubleday", PublicationYear = 1954, Description = "A science fiction detective novel.", PageCount = 206, TotalCopies = 1, AvailableCopies = 1 },
            new() { Id = 11, Title = "And Then There Were None", ISBN = "978-0-06-207348-8", Publisher = "Collins Crime Club", PublicationYear = 1939, Description = "Ten strangers are lured to an island.", PageCount = 272, TotalCopies = 3, AvailableCopies = 3 },
            new() { Id = 12, Title = "21 Lessons for the 21st Century", ISBN = "978-0-525-51217-2", Publisher = "Spiegel & Grau", PublicationYear = 2018, Description = "Explores current affairs and the future of humanity.", PageCount = 372, TotalCopies = 2, AvailableCopies = 2 }
        };
        db.Books.AddRange(books);

        // Book-Author relationships
        db.BookAuthors.AddRange(
            new BookAuthor { BookId = 1, AuthorId = 1 },
            new BookAuthor { BookId = 2, AuthorId = 2 },
            new BookAuthor { BookId = 3, AuthorId = 3 },
            new BookAuthor { BookId = 4, AuthorId = 4 },
            new BookAuthor { BookId = 5, AuthorId = 5 },
            new BookAuthor { BookId = 6, AuthorId = 2 },
            new BookAuthor { BookId = 7, AuthorId = 6 },
            new BookAuthor { BookId = 8, AuthorId = 4 },
            new BookAuthor { BookId = 9, AuthorId = 3 },
            new BookAuthor { BookId = 10, AuthorId = 4 },
            new BookAuthor { BookId = 11, AuthorId = 6 },
            new BookAuthor { BookId = 12, AuthorId = 5 }
        );

        // Book-Category relationships
        db.BookCategories.AddRange(
            new BookCategory { BookId = 1, CategoryId = 1 },
            new BookCategory { BookId = 1, CategoryId = 7 },
            new BookCategory { BookId = 2, CategoryId = 1 },
            new BookCategory { BookId = 2, CategoryId = 2 },
            new BookCategory { BookId = 2, CategoryId = 7 },
            new BookCategory { BookId = 3, CategoryId = 1 },
            new BookCategory { BookId = 3, CategoryId = 7 },
            new BookCategory { BookId = 4, CategoryId = 2 },
            new BookCategory { BookId = 5, CategoryId = 3 },
            new BookCategory { BookId = 5, CategoryId = 4 },
            new BookCategory { BookId = 6, CategoryId = 1 },
            new BookCategory { BookId = 6, CategoryId = 7 },
            new BookCategory { BookId = 7, CategoryId = 6 },
            new BookCategory { BookId = 7, CategoryId = 1 },
            new BookCategory { BookId = 8, CategoryId = 2 },
            new BookCategory { BookId = 9, CategoryId = 1 },
            new BookCategory { BookId = 9, CategoryId = 7 },
            new BookCategory { BookId = 10, CategoryId = 2 },
            new BookCategory { BookId = 10, CategoryId = 6 },
            new BookCategory { BookId = 11, CategoryId = 6 },
            new BookCategory { BookId = 11, CategoryId = 1 },
            new BookCategory { BookId = 12, CategoryId = 3 },
            new BookCategory { BookId = 12, CategoryId = 4 }
        );

        // Patrons
        var now = DateTime.UtcNow;
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", MembershipDate = DateOnly.FromDateTime(now.AddMonths(-12)), MembershipType = MembershipType.Premium, IsActive = true },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Elm Avenue", MembershipDate = DateOnly.FromDateTime(now.AddMonths(-6)), MembershipType = MembershipType.Standard, IsActive = true },
            new() { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@email.com", Phone = "555-0103", Address = "789 Pine Road", MembershipDate = DateOnly.FromDateTime(now.AddMonths(-3)), MembershipType = MembershipType.Student, IsActive = true },
            new() { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@email.com", Phone = "555-0104", Address = "321 Maple Drive", MembershipDate = DateOnly.FromDateTime(now.AddMonths(-18)), MembershipType = MembershipType.Standard, IsActive = true },
            new() { Id = 5, FirstName = "Eva", LastName = "Davis", Email = "eva.davis@email.com", Phone = "555-0105", Address = "654 Cedar Lane", MembershipDate = DateOnly.FromDateTime(now.AddMonths(-9)), MembershipType = MembershipType.Premium, IsActive = true },
            new() { Id = 6, FirstName = "Frank", LastName = "Wilson", Email = "frank.wilson@email.com", Phone = "555-0106", Address = "987 Birch Court", MembershipDate = DateOnly.FromDateTime(now.AddMonths(-24)), MembershipType = MembershipType.Standard, IsActive = false },
            new() { Id = 7, FirstName = "Grace", LastName = "Taylor", Email = "grace.taylor@email.com", Phone = "555-0107", Address = "147 Walnut Way", MembershipDate = DateOnly.FromDateTime(now.AddMonths(-2)), MembershipType = MembershipType.Student, IsActive = true }
        };
        db.Patrons.AddRange(patrons);

        // Loans - various states
        // Active loans: Books 1(2 out of 3), 2(2 out of 4), 4(1 out of 2), 5(1 out of 3), 7(1 out of 2), 8(1 out of 1)
        var loans = new List<Loan>
        {
            // Active loans
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active },
            new() { Id = 2, BookId = 2, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active },
            new() { Id = 3, BookId = 4, PatronId = 2, LoanDate = now.AddDays(-12), DueDate = now.AddDays(2), Status = LoanStatus.Active },
            new() { Id = 4, BookId = 8, PatronId = 3, LoanDate = now.AddDays(-5), DueDate = now.AddDays(2), Status = LoanStatus.Active },
            new() { Id = 5, BookId = 1, PatronId = 4, LoanDate = now.AddDays(-7), DueDate = now.AddDays(7), Status = LoanStatus.Active },
            // Overdue loans
            new() { Id = 6, BookId = 2, PatronId = 4, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue },
            new() { Id = 7, BookId = 5, PatronId = 2, LoanDate = now.AddDays(-18), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue },
            new() { Id = 8, BookId = 7, PatronId = 5, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-4), Status = LoanStatus.Overdue },
            // Returned loans
            new() { Id = 9, BookId = 3, PatronId = 1, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-9), ReturnDate = now.AddDays(-12), Status = LoanStatus.Returned },
            new() { Id = 10, BookId = 6, PatronId = 5, LoanDate = now.AddDays(-20), DueDate = now.AddDays(1), ReturnDate = now.AddDays(-5), Status = LoanStatus.Returned }
        };
        db.Loans.AddRange(loans);

        // Reservations
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 8, PatronId = 1, ReservationDate = now.AddDays(-3), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 2, BookId = 8, PatronId = 5, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 2 },
            new() { Id = 3, BookId = 2, PatronId = 3, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 1 },
            new() { Id = 4, BookId = 1, PatronId = 7, ReservationDate = now.AddDays(-4), ExpirationDate = now.AddDays(2), Status = ReservationStatus.Ready, QueuePosition = 1 }
        };
        db.Reservations.AddRange(reservations);

        // Fines
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 4, LoanId = 6, Amount = 1.50m, Reason = "Overdue return - 6 day(s) late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid },
            new() { Id = 2, PatronId = 2, LoanId = 7, Amount = 1.00m, Reason = "Overdue return - 4 day(s) late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid },
            new() { Id = 3, PatronId = 1, LoanId = 9, Amount = 0.75m, Reason = "Overdue return - 3 day(s) late", IssuedDate = now.AddDays(-12), Status = FineStatus.Paid, PaidDate = now.AddDays(-10) },
            new() { Id = 4, PatronId = 5, LoanId = 8, Amount = 1.00m, Reason = "Overdue return - 4 day(s) late", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid }
        };
        db.Fines.AddRange(fines);

        await db.SaveChangesAsync();
    }
}

// Needed for EF Core LINQ in static seeder
file static class QueryExtensions
{
    public static Task<bool> AnyAsync<T>(this Microsoft.EntityFrameworkCore.DbSet<T> set) where T : class
        => Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(set);
}
