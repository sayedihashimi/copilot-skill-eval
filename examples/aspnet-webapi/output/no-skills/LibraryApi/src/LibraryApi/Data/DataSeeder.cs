using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class DataSeeder
{
    public static void Seed(LibraryDbContext context)
    {
        if (context.Authors.Any()) return; // Already seeded

        var now = DateTime.UtcNow;

        // --- Authors ---
        var authors = new List<Author>
        {
            new() { Id = 1, FirstName = "Jane", LastName = "Austen", Biography = "English novelist known for her six major novels.", BirthDate = new DateOnly(1775, 12, 16), Country = "United Kingdom", CreatedAt = now },
            new() { Id = 2, FirstName = "Isaac", LastName = "Asimov", Biography = "American author and professor of biochemistry, famous for science fiction.", BirthDate = new DateOnly(1920, 1, 2), Country = "United States", CreatedAt = now },
            new() { Id = 3, FirstName = "Gabriel", LastName = "García Márquez", Biography = "Colombian novelist and Nobel Prize laureate.", BirthDate = new DateOnly(1927, 3, 6), Country = "Colombia", CreatedAt = now },
            new() { Id = 4, FirstName = "Yuval Noah", LastName = "Harari", Biography = "Israeli historian and professor, author of Sapiens.", BirthDate = new DateOnly(1976, 2, 24), Country = "Israel", CreatedAt = now },
            new() { Id = 5, FirstName = "Agatha", LastName = "Christie", Biography = "English writer known for detective novels.", BirthDate = new DateOnly(1890, 9, 15), Country = "United Kingdom", CreatedAt = now },
            new() { Id = 6, FirstName = "Stephen", LastName = "Hawking", Biography = "English theoretical physicist and cosmologist.", BirthDate = new DateOnly(1942, 1, 8), Country = "United Kingdom", CreatedAt = now },
        };
        context.Authors.AddRange(authors);

        // --- Categories ---
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiction", Description = "Literary works of imaginative narration" },
            new() { Id = 2, Name = "Science Fiction", Description = "Fiction based on imagined future scientific advances" },
            new() { Id = 3, Name = "History", Description = "Non-fiction works about historical events" },
            new() { Id = 4, Name = "Science", Description = "Works about the natural and physical world" },
            new() { Id = 5, Name = "Biography", Description = "Accounts of someone's life written by another" },
            new() { Id = 6, Name = "Mystery", Description = "Fiction involving a puzzling crime or situation" },
        };
        context.Categories.AddRange(categories);

        // --- Books ---
        var books = new List<Book>
        {
            new() { Id = 1, Title = "Pride and Prejudice", ISBN = "978-0141439518", Publisher = "Penguin Classics", PublicationYear = 1813, Description = "A classic romance novel.", PageCount = 432, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "Sense and Sensibility", ISBN = "978-0141439662", Publisher = "Penguin Classics", PublicationYear = 1811, Description = "A story of two sisters.", PageCount = 409, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "Foundation", ISBN = "978-0553293357", Publisher = "Bantam Books", PublicationYear = 1951, Description = "The first novel in Asimov's Foundation series.", PageCount = 244, Language = "English", TotalCopies = 4, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "I, Robot", ISBN = "978-0553294385", Publisher = "Bantam Books", PublicationYear = 1950, Description = "Collection of robot short stories.", PageCount = 224, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "One Hundred Years of Solitude", ISBN = "978-0060883287", Publisher = "Harper Perennial", PublicationYear = 1967, Description = "The multi-generational story of the Buendía family.", PageCount = 417, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "Love in the Time of Cholera", ISBN = "978-0307389732", Publisher = "Vintage", PublicationYear = 1985, Description = "A love story spanning fifty years.", PageCount = 348, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0062316097", Publisher = "Harper", PublicationYear = 2015, Description = "A narrative history of humankind.", PageCount = 464, Language = "English", TotalCopies = 5, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "Homo Deus", ISBN = "978-0062464347", Publisher = "Harper", PublicationYear = 2017, Description = "A look at humanity's future.", PageCount = 450, Language = "English", TotalCopies = 3, AvailableCopies = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "Murder on the Orient Express", ISBN = "978-0062693662", Publisher = "William Morrow", PublicationYear = 1934, Description = "A Hercule Poirot mystery.", PageCount = 274, Language = "English", TotalCopies = 2, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, Title = "And Then There Were None", ISBN = "978-0062073488", Publisher = "William Morrow", PublicationYear = 1939, Description = "Ten strangers lured to an island.", PageCount = 272, Language = "English", TotalCopies = 2, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, Title = "A Brief History of Time", ISBN = "978-0553380163", Publisher = "Bantam", PublicationYear = 1988, Description = "Landmark exploration of cosmology.", PageCount = 212, Language = "English", TotalCopies = 3, AvailableCopies = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, Title = "The Universe in a Nutshell", ISBN = "978-0553802023", Publisher = "Bantam", PublicationYear = 2001, Description = "Sequel to A Brief History of Time.", PageCount = 216, Language = "English", TotalCopies = 1, AvailableCopies = 1, CreatedAt = now, UpdatedAt = now },
        };
        context.Books.AddRange(books);

        // --- BookAuthors ---
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
            new() { BookId = 12, AuthorId = 6 },
        };
        context.BookAuthors.AddRange(bookAuthors);

        // --- BookCategories ---
        var bookCategories = new List<BookCategory>
        {
            new() { BookId = 1, CategoryId = 1 },
            new() { BookId = 2, CategoryId = 1 },
            new() { BookId = 3, CategoryId = 2 },
            new() { BookId = 4, CategoryId = 2 },
            new() { BookId = 5, CategoryId = 1 },
            new() { BookId = 6, CategoryId = 1 },
            new() { BookId = 7, CategoryId = 3 },
            new() { BookId = 7, CategoryId = 4 },
            new() { BookId = 8, CategoryId = 3 },
            new() { BookId = 8, CategoryId = 4 },
            new() { BookId = 9, CategoryId = 6 },
            new() { BookId = 9, CategoryId = 1 },
            new() { BookId = 10, CategoryId = 6 },
            new() { BookId = 10, CategoryId = 1 },
            new() { BookId = 11, CategoryId = 4 },
            new() { BookId = 12, CategoryId = 4 },
        };
        context.BookCategories.AddRange(bookCategories);

        // --- Patrons ---
        var patrons = new List<Patron>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", MembershipDate = new DateOnly(2023, 1, 15), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@email.com", Phone = "555-0102", Address = "456 Maple Avenue", MembershipDate = new DateOnly(2023, 3, 10), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Carol", LastName = "Davis", Email = "carol.davis@email.com", Phone = "555-0103", Address = "789 Pine Road", MembershipDate = new DateOnly(2023, 6, 1), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "David", LastName = "Wilson", Email = "david.wilson@email.com", Phone = "555-0104", Address = "321 Elm Street", MembershipDate = new DateOnly(2024, 1, 20), MembershipType = MembershipType.Standard, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Emma", LastName = "Brown", Email = "emma.brown@email.com", Phone = "555-0105", Address = "654 Birch Lane", MembershipDate = new DateOnly(2024, 4, 5), MembershipType = MembershipType.Premium, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, FirstName = "Frank", LastName = "Taylor", Email = "frank.taylor@email.com", Phone = "555-0106", Address = "987 Cedar Drive", MembershipDate = new DateOnly(2024, 6, 15), MembershipType = MembershipType.Standard, IsActive = false, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, FirstName = "Grace", LastName = "Martinez", Email = "grace.martinez@email.com", Phone = "555-0107", Address = "111 Walnut Way", MembershipDate = new DateOnly(2024, 8, 1), MembershipType = MembershipType.Student, IsActive = true, CreatedAt = now, UpdatedAt = now },
        };
        context.Patrons.AddRange(patrons);

        // --- Loans ---
        // Active loans: Patron 1 has book 1, Patron 2 has book 4, Patron 1 has book 7, Patron 3 has book 9
        // Returned loans: Patron 2 returned book 5, Patron 4 returned book 3
        // Overdue loans: Patron 5 has book 7 (overdue), Patron 2 has book 11 (overdue)
        var loans = new List<Loan>
        {
            // Active
            new() { Id = 1, BookId = 1, PatronId = 1, LoanDate = now.AddDays(-5), DueDate = now.AddDays(16), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-5) },
            new() { Id = 2, BookId = 4, PatronId = 2, LoanDate = now.AddDays(-10), DueDate = now.AddDays(4), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-10) },
            new() { Id = 3, BookId = 7, PatronId = 1, LoanDate = now.AddDays(-3), DueDate = now.AddDays(18), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-3) },
            new() { Id = 4, BookId = 9, PatronId = 3, LoanDate = now.AddDays(-2), DueDate = now.AddDays(5), Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now.AddDays(-2) },
            // Returned
            new() { Id = 5, BookId = 5, PatronId = 2, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-16), ReturnDate = now.AddDays(-17), Status = LoanStatus.Returned, RenewalCount = 0, CreatedAt = now.AddDays(-30) },
            new() { Id = 6, BookId = 3, PatronId = 4, LoanDate = now.AddDays(-25), DueDate = now.AddDays(-11), ReturnDate = now.AddDays(-12), Status = LoanStatus.Returned, RenewalCount = 1, CreatedAt = now.AddDays(-25) },
            // Overdue
            new() { Id = 7, BookId = 7, PatronId = 5, LoanDate = now.AddDays(-30), DueDate = now.AddDays(-9), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-30) },
            new() { Id = 8, BookId = 11, PatronId = 2, LoanDate = now.AddDays(-20), DueDate = now.AddDays(-6), Status = LoanStatus.Overdue, RenewalCount = 0, CreatedAt = now.AddDays(-20) },
        };
        context.Loans.AddRange(loans);

        // --- Reservations ---
        var reservations = new List<Reservation>
        {
            // Pending reservation for book 7 (both copies out on loan)
            new() { Id = 1, BookId = 7, PatronId = 4, ReservationDate = now.AddDays(-1), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-1) },
            // Ready reservation for book 9 (copy returned, patron notified)
            new() { Id = 2, BookId = 9, PatronId = 2, ReservationDate = now.AddDays(-5), ExpirationDate = now.AddDays(2), Status = ReservationStatus.Ready, QueuePosition = 1, CreatedAt = now.AddDays(-5) },
            // Pending reservation for book 4
            new() { Id = 3, BookId = 4, PatronId = 3, ReservationDate = now.AddDays(-2), Status = ReservationStatus.Pending, QueuePosition = 1, CreatedAt = now.AddDays(-2) },
        };
        context.Reservations.AddRange(reservations);

        // --- Fines ---
        // Overdue fine on loan 7 (9 days overdue * $0.25 = $2.25)
        // Paid fine on loan 5
        // Unpaid fine on patron 2 for loan 8 (6 days overdue * $0.25 = $1.50)
        var fines = new List<Fine>
        {
            new() { Id = 1, PatronId = 5, LoanId = 7, Amount = 2.25m, Reason = "Overdue return", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-1) },
            new() { Id = 2, PatronId = 2, LoanId = 5, Amount = 0.50m, Reason = "Overdue return", IssuedDate = now.AddDays(-15), PaidDate = now.AddDays(-14), Status = FineStatus.Paid, CreatedAt = now.AddDays(-15) },
            new() { Id = 3, PatronId = 2, LoanId = 8, Amount = 1.50m, Reason = "Overdue return", IssuedDate = now.AddDays(-1), Status = FineStatus.Unpaid, CreatedAt = now.AddDays(-1) },
        };
        context.Fines.AddRange(fines);

        context.SaveChanges();
    }
}
