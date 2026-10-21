using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Dtos;
using PatientService.Models;

namespace PatientService.Repositories;

public sealed class PatientRepository(PatientDbContext context) : IPatientRepository
{
    public async Task<PagedResult<PatientDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await context.Patients
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var items = await context.Patients
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatientDto(
                p.Id,
                p.FirstName,
                p.LastName,
                p.BirthDate,
                p.Gender,
                p.Address,
                p.PhoneNumber))
            .ToListAsync(cancellationToken);

        return new PagedResult<PatientDto>(items, page, pageSize, totalCount);
    }

    public Task<PatientDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => context.Patients
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatientDto(
                p.Id,
                p.FirstName,
                p.LastName,
                p.BirthDate,
                p.Gender,
                p.Address,
                p.PhoneNumber))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Patient?> FindAsync(int id, CancellationToken cancellationToken)
        => await context.Patients.FindAsync([id], cancellationToken);

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken)
    {
        context.Patients.Add(patient);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken)
        => await context.SaveChangesAsync(cancellationToken);
}
