using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MedicalRecordDetailResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct = default);
    Task<MedicalRecordDetailResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct = default);
}
