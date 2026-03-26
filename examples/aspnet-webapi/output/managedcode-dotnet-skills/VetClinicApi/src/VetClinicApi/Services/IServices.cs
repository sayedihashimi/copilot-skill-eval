using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResult<OwnerDto>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<OwnerDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<OwnerDto> CreateAsync(CreateOwnerDto dto, CancellationToken ct);
    Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<List<PetSummaryDto>> GetPetsAsync(int ownerId, CancellationToken ct);
    Task<List<AppointmentDto>> GetAppointmentsAsync(int ownerId, CancellationToken ct);
}

public interface IPetService
{
    Task<PagedResult<PetDto>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct);
    Task<PetDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PetDto> CreateAsync(CreatePetDto dto, CancellationToken ct);
    Task<PetDto?> UpdateAsync(int id, UpdatePetDto dto, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    Task<List<MedicalRecordDto>> GetMedicalRecordsAsync(int petId, CancellationToken ct);
    Task<List<VaccinationDto>> GetVaccinationsAsync(int petId, CancellationToken ct);
    Task<List<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct);
    Task<List<PrescriptionDto>> GetActivePrescriptionsAsync(int petId, CancellationToken ct);
}

public interface IVeterinarianService
{
    Task<PagedResult<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct);
    Task<VeterinarianDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto, CancellationToken ct);
    Task<VeterinarianDto?> UpdateAsync(int id, UpdateVeterinarianDto dto, CancellationToken ct);
    Task<List<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct);
    Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken ct);
}

public interface IAppointmentService
{
    Task<PagedResult<AppointmentDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int page, int pageSize, CancellationToken ct);
    Task<AppointmentDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto, CancellationToken ct);
    Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto, CancellationToken ct);
    Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto, CancellationToken ct);
    Task<List<AppointmentDto>> GetTodayAsync(CancellationToken ct);
}

public interface IMedicalRecordService
{
    Task<MedicalRecordDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto, CancellationToken ct);
    Task<MedicalRecordDto?> UpdateAsync(int id, UpdateMedicalRecordDto dto, CancellationToken ct);
}

public interface IPrescriptionService
{
    Task<PrescriptionDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto, CancellationToken ct);
}

public interface IVaccinationService
{
    Task<VaccinationDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto, CancellationToken ct);
}
