using PatientService.Dtos;

namespace PatientService.Services;

public interface IPatientsService
{
    Task<PagedResult<PatientDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<PatientDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<PatientDto> CreateAsync(PatientRequest request, CancellationToken cancellationToken);

    Task<PatientDto?> UpdateAsync(int id, PatientRequest request, CancellationToken cancellationToken);
}
