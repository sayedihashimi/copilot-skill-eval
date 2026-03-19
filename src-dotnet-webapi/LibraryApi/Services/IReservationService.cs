using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(string? status, int page, int pageSize, CancellationToken ct);
    Task<ReservationResponse?> GetReservationByIdAsync(int id, CancellationToken ct);
    Task<(ReservationResponse? Reservation, string? Error)> CreateReservationAsync(CreateReservationRequest request, CancellationToken ct);
    Task<(ReservationResponse? Reservation, string? Error, bool NotFound)> CancelReservationAsync(int id, CancellationToken ct);
    Task<(LoanResponse? Loan, string? Error, bool NotFound)> FulfillReservationAsync(int id, CancellationToken ct);
}
