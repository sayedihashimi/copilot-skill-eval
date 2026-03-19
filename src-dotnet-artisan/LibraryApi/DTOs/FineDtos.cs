using LibraryApi.Models;

namespace LibraryApi.DTOs;

public record FineResponse(
    int Id,
    int PatronId,
    string PatronName,
    int LoanId,
    decimal Amount,
    string Reason,
    DateTime IssuedDate,
    DateTime? PaidDate,
    FineStatus Status);
