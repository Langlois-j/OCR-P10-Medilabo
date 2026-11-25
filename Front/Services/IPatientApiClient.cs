using Front.Models;

namespace Front.Services;

public interface IPatientApiClient
{
    Task<PagedResult<PatientSummary>> GetPatientsAsync(
        int page,
        int pageSize,
        string? token,
        CancellationToken cancellationToken);
}
