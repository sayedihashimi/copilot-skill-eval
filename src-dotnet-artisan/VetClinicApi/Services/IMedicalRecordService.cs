using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct);
    Task<MedicalRecordDto?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct);
}
