using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResult<OwnerSummaryDto>> GetAllAsync(string? search, PaginationParams pagination);
    Task<OwnerResponseDto?> GetByIdAsync(int id);
    Task<OwnerResponseDto> CreateAsync(OwnerCreateDto dto);
    Task<OwnerResponseDto?> UpdateAsync(int id, OwnerUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<PetResponseDto>> GetPetsAsync(int ownerId);
    Task<PagedResult<AppointmentResponseDto>> GetAppointmentsAsync(int ownerId, PaginationParams pagination);
}

public interface IPetService
{
    Task<PagedResult<PetResponseDto>> GetAllAsync(string? search, string? species, bool includeInactive, PaginationParams pagination);
    Task<PetResponseDto?> GetByIdAsync(int id);
    Task<PetResponseDto> CreateAsync(PetCreateDto dto);
    Task<PetResponseDto?> UpdateAsync(int id, PetUpdateDto dto);
    Task<bool> SoftDeleteAsync(int id);
    Task<List<MedicalRecordResponseDto>> GetMedicalRecordsAsync(int petId);
    Task<List<VaccinationResponseDto>> GetVaccinationsAsync(int petId);
    Task<List<VaccinationResponseDto>> GetUpcomingVaccinationsAsync(int petId);
    Task<List<PrescriptionResponseDto>> GetActivePrescriptionsAsync(int petId);
}

public interface IVeterinarianService
{
    Task<PagedResult<VeterinarianResponseDto>> GetAllAsync(string? specialization, bool? isAvailable, PaginationParams pagination);
    Task<VeterinarianResponseDto?> GetByIdAsync(int id);
    Task<VeterinarianResponseDto> CreateAsync(VeterinarianCreateDto dto);
    Task<VeterinarianResponseDto?> UpdateAsync(int id, VeterinarianUpdateDto dto);
    Task<List<AppointmentResponseDto>> GetScheduleAsync(int vetId, DateOnly date);
    Task<PagedResult<AppointmentResponseDto>> GetAppointmentsAsync(int vetId, string? status, PaginationParams pagination);
}

public interface IAppointmentService
{
    Task<PagedResult<AppointmentResponseDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, PaginationParams pagination);
    Task<AppointmentResponseDto?> GetByIdAsync(int id);
    Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto dto);
    Task<AppointmentResponseDto?> UpdateAsync(int id, AppointmentUpdateDto dto);
    Task<AppointmentResponseDto?> UpdateStatusAsync(int id, AppointmentStatusUpdateDto dto);
    Task<List<AppointmentResponseDto>> GetTodayAsync();
}

public interface IMedicalRecordService
{
    Task<MedicalRecordResponseDto?> GetByIdAsync(int id);
    Task<MedicalRecordResponseDto> CreateAsync(MedicalRecordCreateDto dto);
    Task<MedicalRecordResponseDto?> UpdateAsync(int id, MedicalRecordUpdateDto dto);
}

public interface IPrescriptionService
{
    Task<PrescriptionResponseDto?> GetByIdAsync(int id);
    Task<PrescriptionResponseDto> CreateAsync(PrescriptionCreateDto dto);
}

public interface IVaccinationService
{
    Task<VaccinationResponseDto?> GetByIdAsync(int id);
    Task<VaccinationResponseDto> CreateAsync(VaccinationCreateDto dto);
}
