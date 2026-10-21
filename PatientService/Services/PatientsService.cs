using PatientService.Dtos;
using PatientService.Models;
using PatientService.Repositories;

namespace PatientService.Services;

public sealed class PatientsService(IPatientRepository repository, TimeProvider timeProvider) : IPatientsService
{
    public const int MaxPageSize = 100;

    public async Task<PagedResult<PatientDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, MaxPageSize);

        return await repository.GetPageAsync(page, pageSize, cancellationToken);
    }

    public Task<PatientDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => repository.GetByIdAsync(id, cancellationToken);

    public async Task<PatientDto> CreateAsync(PatientRequest request, CancellationToken cancellationToken)
    {
        var patient = Patient.Create(
            request.FirstName,
            request.LastName,
            request.BirthDate,
            request.Gender,
            timeProvider,
            request.Address,
            request.PhoneNumber);

        await repository.AddAsync(patient, cancellationToken);

        return ToDto(patient);
    }

    public async Task<PatientDto?> UpdateAsync(int id, PatientRequest request, CancellationToken cancellationToken)
    {
        var patient = await repository.FindAsync(id, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        patient.Update(
            request.FirstName,
            request.LastName,
            request.BirthDate,
            request.Gender,
            timeProvider,
            request.Address,
            request.PhoneNumber);

        await repository.UpdateAsync(patient, cancellationToken);

        return ToDto(patient);
    }

    private static PatientDto ToDto(Patient patient)
        => new(
            patient.Id,
            patient.FirstName,
            patient.LastName,
            patient.BirthDate,
            patient.Gender,
            patient.Address,
            patient.PhoneNumber);
}
