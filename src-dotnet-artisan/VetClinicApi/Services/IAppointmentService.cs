using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PaginatedResponse<AppointmentDto>> GetAllAsync(DateTime? dateFrom, DateTime? dateTo, AppointmentStatus? status, int? vetId, int? petId, int page, int pageSize, CancellationToken ct);
    Task<AppointmentDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct);
    Task<IReadOnlyList<AppointmentDto>> GetTodayAsync(CancellationToken ct);
}
