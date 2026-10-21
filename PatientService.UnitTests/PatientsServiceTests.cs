using PatientService.Dtos;
using PatientService.Models;
using PatientService.Repositories;
using PatientService.Services;

namespace PatientService.UnitTests;

public class PatientsServiceTests
{
    private static readonly CancellationToken _ct = CancellationToken.None;

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private static readonly TimeProvider _clock =
        new FixedTimeProvider(new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero));

    private sealed class FakePatientRepository : IPatientRepository
    {
        private readonly List<Patient> _patients = [];
        private int _nextId = 1;

        public int Count => _patients.Count;

        public int UpdateCalls { get; private set; }

        public Task<PagedResult<PatientDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            var items = _patients
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToDto)
                .ToList();

            return Task.FromResult(new PagedResult<PatientDto>(items, page, pageSize, _patients.Count));
        }

        public Task<PatientDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var patient = _patients.Find(p => p.Id == id);
            return Task.FromResult(patient is null ? null : ToDto(patient));
        }

        public Task<Patient?> FindAsync(int id, CancellationToken cancellationToken)
            => Task.FromResult(_patients.Find(p => p.Id == id));

        public Task AddAsync(Patient patient, CancellationToken cancellationToken)
        {
            typeof(Patient).GetProperty(nameof(Patient.Id))!.SetValue(patient, _nextId++);
            _patients.Add(patient);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Patient patient, CancellationToken cancellationToken)
        {
            UpdateCalls++;
            return Task.CompletedTask;
        }

        private static PatientDto ToDto(Patient p)
            => new(p.Id, p.FirstName, p.LastName, p.BirthDate, p.Gender, p.Address, p.PhoneNumber);
    }

    private static (PatientsService Service, FakePatientRepository Repository) CreateService()
    {
        var repository = new FakePatientRepository();
        return (new PatientsService(repository, _clock), repository);
    }

    private static Patient NewPatient(string firstName)
        => Patient.Create(firstName, "lastName", new DateOnly(1966, 12, 31), Gender.F, _clock);

    private static PatientRequest NewRequest(string firstName = "Jane", string lastName = "Doe")
        => new(firstName, lastName, new DateOnly(1990, 5, 20), Gender.M, "1 Main St", "100-222-3333");

    // ----- Liste paginée -----

    [Fact]
    public async Task GetPageReturnsRequestedPage()
    {
        var (service, repository) = CreateService();
        for (var i = 1; i <= 3; i++)
        {
            await repository.AddAsync(NewPatient($"First{i}"), _ct);
        }

        var result = await service.GetPageAsync(2, 2, _ct);

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        var item = Assert.Single(result.Items);
        Assert.Equal("First3", item.FirstName);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetPageWithInvalidPagingThrowsArgumentOutOfRangeException(int page, int pageSize)
    {
        var (service, _) = CreateService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetPageAsync(page, pageSize, _ct));
    }

    // ----- Détail -----

    [Fact]
    public async Task GetByIdWithExistingPatientReturnsDto()
    {
        var (service, repository) = CreateService();
        await repository.AddAsync(NewPatient("Jane"), _ct);

        var dto = await service.GetByIdAsync(1, _ct);

        Assert.NotNull(dto);
        Assert.Equal(1, dto.Id);
        Assert.Equal("Jane", dto.FirstName);
    }

    [Fact]
    public async Task GetByIdWithUnknownIdReturnsNull()
    {
        var (service, _) = CreateService();

        var dto = await service.GetByIdAsync(42, _ct);

        Assert.Null(dto);
    }

    // ----- Création -----

    [Fact]
    public async Task CreateWithValidRequestPersistsPatientAndReturnsDto()
    {
        var (service, repository) = CreateService();

        var dto = await service.CreateAsync(NewRequest(firstName: "  Jane  "), _ct);

        Assert.Equal(1, repository.Count);
        Assert.Equal(1, dto.Id);
        Assert.Equal("Jane", dto.FirstName);
        Assert.Equal("Doe", dto.LastName);
        Assert.Equal("1 Main St", dto.Address);
        Assert.Equal("100-222-3333", dto.PhoneNumber);
    }

    [Fact]
    public async Task CreateWithInvalidRequestThrowsAndPersistsNothing()
    {
        var (service, repository) = CreateService();

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.CreateAsync(NewRequest(firstName: " "), _ct));

        Assert.Equal(0, repository.Count);
    }

    // ----- Mise à jour -----

    [Fact]
    public async Task UpdateWithExistingPatientReplacesFieldsAndReturnsDto()
    {
        var (service, repository) = CreateService();
        await repository.AddAsync(NewPatient("Old"), _ct);

        var dto = await service.UpdateAsync(1, NewRequest(firstName: "Jane"), _ct);

        Assert.NotNull(dto);
        Assert.Equal(1, dto.Id);
        Assert.Equal("Jane", dto.FirstName);
        Assert.Equal(Gender.M, dto.Gender);
        Assert.Equal(1, repository.UpdateCalls);
    }

    [Fact]
    public async Task UpdateWithUnknownIdReturnsNullAndPersistsNothing()
    {
        var (service, repository) = CreateService();

        var dto = await service.UpdateAsync(42, NewRequest(), _ct);

        Assert.Null(dto);
        Assert.Equal(0, repository.UpdateCalls);
    }

    [Fact]
    public async Task UpdateWithInvalidRequestThrowsAndPersistsNothing()
    {
        var (service, repository) = CreateService();
        await repository.AddAsync(NewPatient("Old"), _ct);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.UpdateAsync(1, NewRequest(lastName: " "), _ct));

        Assert.Equal(0, repository.UpdateCalls);
    }
}
