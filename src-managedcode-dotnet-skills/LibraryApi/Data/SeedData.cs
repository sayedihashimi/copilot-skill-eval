using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(LibraryDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Authors.AnyAsync())
            return;

        // Authors
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "Harper", LastName = "Lee", Biography = "American novelist known for To Kill a Mockingbird.", BirthDate = new DateOnly(1926, 4, 28), Country = "United States", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "George", LastName = "Orwell", Biography = "English novelist and essayist known for 1984 and Animal Farm.", BirthDate = new DateOnly(1903, 6, 25), Country = "United Kingdom", CreatedAt = DateTime.UtcNow },
            new() { Id = 3, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for Pride and Prejudice.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = DateTime.UtcNow },
            new() { Id = 4, FirstName = "Mark", LastName = "Twain", Biography = "American author and humorist known for Adventures of Huckleberry Finn.", BirthDate = new DateOnly(1835, 11, 30), Country = "United States", CreatedAt = DateTime.UtcNow },
            new() { Id = 5, FirstName = "Agatha", LastName = "Christie", Biography = "English writer known for detective novels.", BirthDate = new DateOnly(1890, 9, 15), Country = "United Kingdom", CreatedAt = DateTime.UtcNow },
            new() { Id = 6, FirstName = "F. Scott", LastName = "Fitzgerald", Biography = "American novelist known for The Great Gatsby.", BirthDate = new DateOnly(1896, 9, 24), Country = "United States", CreatedAt = DateTime.UtcNow },
        };
        context.Authors.AddRange(authors);

        // Categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary fiction and novels" },
            new() { Id = 2, Name = "Classic", Description = "Timeless classic literature" },
            new() { Id = 3, Name = "Mystery", Description = "Mystery and detective fiction" },
            new() { Id = 4, Name = "Science Fiction", Description = "Science fiction and speculative fiction" },
            new() { Id = 5, Name = "Romance", Description = "Romance novels and love stories" },
        };
        context.Categories.AddRange(categories);

        var now = DateTime.UtcNow;

        // Books
        var books = new List<Book>
        {
            new() { Id = 1, Title = "To Kill a Mockingbird", ISBN = "978-0-06-112008-4", Publisher = "J. B. Lippincott & Co.", PublicationYear = 1960, Description = "A novel about racial injustice in the Deep South.", PageCount = 281, Language = "English", TotalCopies = 5, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "1984", ISBN = "978-0-451-52493-5", Publisher = "Secker & Warburg", PublicationYear = 1949, Description = "A dystopian social science fiction novel.", PageCount = 328, Language = "English", TotalCopies = 4, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Animal Farm", ISBN = "978-0-451-52634-2", Publisher = "Secker & Warburg", PublicationYear = 1945, Description = "An allegorical novella reflecting events leading to the Russian Revolution.", PageCount = 112, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "Pride and Prejudice", ISBN = "978-0-14-143951-8", Publisher = "T. Egerton", PublicationYear = 1813, Description = "A romantic novel of manners.", PageCount = 432, Language = "English", TotalCopies = 4, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "Sense and Sensibility", ISBN = "978-0-14-143966-2", Publisher = "Thomas Egerton", PublicationYear = 1811, Description = "A story of the Dashwood sisters.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "Adventures of Huckleberry Finn", ISBN = "978-0-14-243717-0", Publisher = "Chatto & Windus", PublicationYear = 1884, Description = "A novel about a boy's adventures along the Mississippi River.", PageCount = 366, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "The Adventures of Tom Sawyer", ISBN = "978-0-14-310710-4", Publisher = "American Publishing Company", PublicationYear = 1876, Description = "A novel about a young boy growing up along the Mississippi.", PageCount = 274, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "Murder on the Orient Express", ISBN = "978-0-06-269366-2", Publisher = "Collins Crime Club", PublicationYear = 1934, Description = "A detective novel featuring Hercule Poirot.", PageCount = 256, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "The ABC Murders", ISBN = "978-0-06-269367-9", Publisher = "Collins Crime Club", PublicationYear = 1936, Description = "A Hercule Poirot mystery novel.", PageCount = 256, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "The Great Gatsby", ISBN = "978-0-7432-7356-5", Publisher = "Charles Scribner's Sons", PublicationYear = 1925, Description = "A novel about the American Dream.", PageCount = 180, Language = "English", TotalCopies = 4, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "Tender Is the Night", ISBN = "978-0-684-80154-1", Publisher = "Charles Scribner's Sons", PublicationYear = 1934, Description = "A novel about the rise and fall of Dick Diver.", PageCount = 317, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "Emma", ISBN = "978-0-14-143958-7", Publisher = "John Murray", PublicationYear = 1815, Description = "A comic novel about youthful hubris and romantic misunderstandings.", PageCount = 474, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
        };
        context.Books.AddRange(books);

        // BookAuthors
        var bookAuthors = new List<BookAuthor>
        {
            new() { BookId = 1, AuthorId = 1 },   // To Kill a Mockingbird - Harper Lee
            new() { BookId = 2, AuthorId = 2 },   // 1984 - Orwell
            new() { BookId = 3, AuthorId = 2 },   // Animal Farm - Orwell
            new() { BookId = 4, AuthorId = 3 },   // Pride and Prejudice - Austen
            new() { BookId = 5, AuthorId = 3 },   // Sense and Sensibility - Austen
            new() { BookId = 6, AuthorId = 4 },   // Huckleberry Finn - Twain
            new() { BookId = 7, AuthorId = 4 },   // Tom Sawyer - Twain
            new() { BookId = 8, AuthorId = 5 },   // Orient Express - Christie
            new() { BookId = 9, AuthorId = 5 },   // ABC Murders - Christie
            new() { BookId = 10, AuthorId = 6 },  // Great Gatsby - Fitzgerald
            new() { BookId = 11, AuthorId = 6 },  // Tender Is the Night - Fitzgerald
            new() { BookId = 12, AuthorId = 3 },  // Emma - Austen
        };
        context.BookAuthors.AddRange(bookAuthors);

        // BookCategories
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 }, new() { BookId = 1, CategoryId = 2 },
            new() { BookId = 2, CategoryId = 1 }, new() { BookId = 2, CategoryId = 4 },
            new() { BookId = 3, CategoryId = 1 }, new() { BookId = 3, CategoryId = 2 },
            new() { BookId = 4, CategoryId = 1 }, new() { BookId = 4, CategoryId = 2 }, new() { BookId = 4, CategoryId = 5 },
            new() { BookId = 5, CategoryId = 1 }, new() { BookId = 5, CategoryId = 5 },
            new() { BookId = 6, CategoryId = 1 }, new() { BookId = 6, CategoryId = 2 },
            new() { BookId = 7, CategoryId = 1 }, new() { BookId = 7, CategoryId = 2 },
            new() { BookId = 8, CategoryId = 1 }, new() { BookId = 8, CategoryId = 3 },
            new() { BookId = 9, CategoryId = 1 }, new() { BookId = 9, CategoryId = 3 },
            new() { BookId = 10, CategoryId = 1 }, new() { BookId = 10, CategoryId = 2 },
            new() { BookId = 11, CategoryId = 1 }, new() { BookId = 11, CategoryId = 5 },
            new() { BookId = 12, CategoryId = 1 }, new() { BookId = 12, CategoryId = 2 }, new() { BookId = 12, CategoryId = 5 },
        };
        context.BookCategories.AddRange(bookCategories);

        // Patrons
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Main St, Springfield", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Oak Ave, Springfield", MembershipDate = new DateOnly(2023, 3, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Carol", LastName = "Davis", Email = "carol.davis@email.com", Phone = "555-0103", Address = "789 Pine Rd, Springfield", MembershipDate = new DateOnly(2023, 6, 10), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "David", LastName = "Wilson", Email = "david.wilson@email.com", Phone = "555-0104", Address = "321 Elm St, Springfield", MembershipDate = new DateOnly(2023, 9, 5), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Eva", LastName = "Martinez", Email = "eva.martinez@email.com", Phone = "555-0105", Address = "654 Maple Dr, Springfield", MembershipDate = new DateOnly(2024, 1, 12), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Frank", LastName = "Brown", Email = "frank.brown@email.com", Phone = "555-0106", Address = "987 Cedar Ln, Springfield", MembershipDate = new DateOnly(2024, 4, 1), MembershipType = MembershipType.Student, IsActive = false, CreatedAt = now, UpdatedAt = now },
        };
        context.Patrons.AddRange(patrons);

        // Loans (8 loans: mix of active, returned, and overdue)
        var loans = new List<Loan>
        {
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-10), DueDate = now.AddDays(11), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-10) },
            new() { Id = 2, BookId = 2, PatronId = 1, LoanDate = now.AddDays(-20), DueDate = now.AddDays(1), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-20) },
            new() { Id = 3, BookId = 4, PatronId = 2, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-14), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-30) },
            new() { Id = 4, BookId = 5, PatronId = 3, LoanDate = now.AddDays(-10), DueDate = now.AddDays(-3), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-10) },
            new() { Id = 5, BookId = 8, PatronId = 4, LoanDate = now.AddDays(-5), DueDate = now.AddDays(9), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-5) },
            new() { Id = 6, BookId = 10, PatronId = 5, LoanDate = now.AddDays(-15), DueDate = now.AddDays(6), Status = LoanStatus.Active, RenewalCount = 1, CreatedAt = now.AddDays(-15) },
            new() { Id = 7, BookId = 10, PatronId = 2, LoanDate = now.AddDays(-40), DueDate = now.AddDays(-26), ReturnDate = now.AddDays(-25), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-40) },
            new() { Id = 8, BookId = 9, PatronId = 5, LoanDate = now.AddDays(-8), DueDate = now.AddDays(13), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-8) },
        };
        context.Loans.AddRange(loans);

        // Reservations (3)
        var reservations = new List<Reservation>
        {
            new() { Id = 1, BookId = 2, PatronId = 3, ReservationDate = now.AddDays(-2), ExpirationDate = now.AddDays(12), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-2) },
            new() { Id = 2, BookId = 5, PatronId = 4, ReservationDate = now.AddDays(-1), ExpirationDate = now.AddDays(13), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-1) },
            new() { Id = 3, BookId = 1, PatronId = 2, ReservationDate = now.AddDays(-3), ExpirationDate = now.AddDays(11), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-3) },
        };
        context.Reservations.AddRange(reservations);

        // Fines (3)
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 2, LoanId = 3, Amount = 0.50m, Reason = "Overdue return - 2 days late", IssuedDate = now.AddDays(-14), PaidDate = now.AddDays(-12), Status = FineStatus.Paid, CreatedAt = now.AddDays(-14) },
            new() { Id = 2, PatronId = 3, LoanId = 4, Amount = 0.75m, Reason = "Overdue - 3 days late (ongoing)", IssuedDate = now.AddDays(-3), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-3) },
            new() { Id = 3, PatronId = 4, LoanId = 5, Amount = 1.00m, Reason = "Damaged book cover", IssuedDate = now.AddDays(-2), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-2) },
        };
        context.Fines.AddRange(fines);

        await context.SaveChangesAsync();
    }
}
