namespace LibraryApi.DTOs;

// --- Author DTOs ---

public record AuthorResponse(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt,
    List<AuthorBookResponse>? Books = null
);

public record AuthorBookResponse(int Id, string Title, string ISBN);

public record CreateAuthorRequest(
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country
);

public record UpdateAuthorRequest(
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country
);

// --- Category DTOs ---

public record CategoryResponse(
    int Id,
    string Name,
    string? Description,
    int? BookCount = null
);

public record CreateCategoryRequest(string Name, string? Description);

public record UpdateCategoryRequest(string Name, string? Description);

// --- Book DTOs ---

public record BookResponse(
    int Id,
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string? Description,
    int? PageCount,
    string Language,
    int TotalCopies,
    int AvailableCopies,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<BookAuthorResponse>? Authors = null,
    List<BookCategoryResponse>? Categories = null
);

public record BookAuthorResponse(int Id, string FirstName, string LastName);

public record BookCategoryResponse(int Id, string Name);

public record CreateBookRequest(
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string? Description,
    int? PageCount,
    string? Language,
    int TotalCopies,
    List<int> AuthorIds,
    List<int> CategoryIds
);

public record UpdateBookRequest(
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string? Description,
    int? PageCount,
    string? Language,
    int TotalCopies,
    List<int> AuthorIds,
    List<int> CategoryIds
);

// --- Patron DTOs ---

public record PatronResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    DateOnly MembershipDate,
    string MembershipType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int? ActiveLoansCount = null,
    decimal? TotalUnpaidFines = null
);

public record CreatePatronRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    string MembershipType
);

public record UpdatePatronRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    string MembershipType
);

// --- Loan DTOs ---

public record LoanResponse(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnDate,
    string Status,
    int RenewalCount,
    DateTime CreatedAt
);

public record CreateLoanRequest(int BookId, int PatronId);

// --- Reservation DTOs ---

public record ReservationResponse(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime ReservationDate,
    DateTime? ExpirationDate,
    string Status,
    int QueuePosition,
    DateTime CreatedAt
);

public record CreateReservationRequest(int BookId, int PatronId);

// --- Fine DTOs ---

public record FineResponse(
    int Id,
    int PatronId,
    string PatronName,
    int LoanId,
    string BookTitle,
    decimal Amount,
    string Reason,
    DateTime IssuedDate,
    DateTime? PaidDate,
    string Status,
    DateTime CreatedAt
);
