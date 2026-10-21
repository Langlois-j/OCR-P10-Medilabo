using PatientService.Dtos;
using PatientService.Models;

namespace PatientService.Repositories;

public interface IPatientRepository
{
    Task<PagedResult<PatientDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<PatientDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<Patient?> FindAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Patient patient, CancellationToken cancellationToken);

    Task UpdateAsync(Patient patient, CancellationToken cancellationToken);
}
