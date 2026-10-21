using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;
using PatientService.Repositories;

namespace PatientService.IntegrationTests;

public sealed class PatientRepositoryTests : IDisposable
{
    private static readonly CancellationToken _ct = CancellationToken.None;

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<PatientDbContext> _options;

    public PatientRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<PatientDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new PatientDbContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private PatientDbContext NewContext() => new(_options);


    [Fact]
    public async Task GetPageReturnsRequestedPageOrderedById()
    {
        using var context = NewContext();
        var repository = new PatientRepository(context);

        var result = await repository.GetPageAsync(2, 2, _ct);

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(4, result.TotalCount);
        Assert.Equal([3, 4], result.Items.Select(p => p.Id));
    }

    [Fact]
    public async Task GetPageBeyondLastPageReturnsNoItemsButTotalCount()
    {
        using var context = NewContext();
        var repository = new PatientRepository(context);

        var result = await repository.GetPageAsync(3, 2, _ct);

        Assert.Empty(result.Items);
        Assert.Equal(4, result.TotalCount);
    }

    [Fact]
    public async Task GetPageDoesNotTrackEntities()
    {
        using var context = NewContext();
        var repository = new PatientRepository(context);

        await repository.GetPageAsync(1, 10, _ct);

        Assert.Empty(context.ChangeTracker.Entries());
    }


    [Fact]
    public async Task GetByIdWithSeededPatientReturnsProjection()
    {
        using var context = NewContext();
        var repository = new PatientRepository(context);

        var dto = await repository.GetByIdAsync(1, _ct);

        Assert.NotNull(dto);
        Assert.Equal("Test", dto.FirstName);
        Assert.Equal("TestNone", dto.LastName);
        Assert.Equal(new DateOnly(1966, 12, 31), dto.BirthDate);
        Assert.Equal(Gender.F, dto.Gender);
        Assert.Equal("1 Brookside St", dto.Address);
        Assert.Equal("100-222-3333", dto.PhoneNumber);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetByIdWithUnknownIdReturnsNull()
    {
        using var context = NewContext();
        var repository = new PatientRepository(context);

        var dto = await repository.GetByIdAsync(42, _ct);

        Assert.Null(dto);
    }


    [Fact]
    public async Task AddAsyncPersistsPatientAndAssignsId()
    {
        var patient = Patient.Create("Jane", "Doe", new DateOnly(1990, 5, 20), Gender.F, TimeProvider.System);

        using (var context = NewContext())
        {
            await new PatientRepository(context).AddAsync(patient, _ct);
        }

        Assert.Equal(5, patient.Id);

        using var verification = NewContext();
        var dto = await new PatientRepository(verification).GetByIdAsync(5, _ct);
        Assert.NotNull(dto);
        Assert.Equal("Jane", dto.FirstName);
        Assert.Null(dto.Address);
        Assert.Null(dto.PhoneNumber);
    }

    [Fact]
    public async Task FindAsyncWithUnknownIdReturnsNull()
    {
        using var context = NewContext();
        var repository = new PatientRepository(context);

        var patient = await repository.FindAsync(42, _ct);

        Assert.Null(patient);
    }

    [Fact]
    public async Task UpdateAsyncPersistsChangesMadeOnFoundPatient()
    {
        using (var context = NewContext())
        {
            var repository = new PatientRepository(context);
            var patient = await repository.FindAsync(1, _ct);
            Assert.NotNull(patient);

            patient.Update("Jane", "Doe", new DateOnly(1990, 5, 20), Gender.M, TimeProvider.System, null, null);
            await repository.UpdateAsync(patient, _ct);
        }

        using var verification = NewContext();
        var dto = await new PatientRepository(verification).GetByIdAsync(1, _ct);
        Assert.NotNull(dto);
        Assert.Equal("Jane", dto.FirstName);
        Assert.Equal("Doe", dto.LastName);
        Assert.Equal(Gender.M, dto.Gender);
        Assert.Null(dto.Address);
        Assert.Null(dto.PhoneNumber);
    }
}
