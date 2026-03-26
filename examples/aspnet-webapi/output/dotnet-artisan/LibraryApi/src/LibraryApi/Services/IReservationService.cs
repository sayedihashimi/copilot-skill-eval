using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(ReservationStatus? status, int page, int pageSize);
    Task<ReservationResponse?> GetReservationByIdAsync(int id);
    Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request);
    Task<ReservationResponse> CancelReservationAsync(int id);
    Task<LoanResponse> FulfillReservationAsync(int id);
}
