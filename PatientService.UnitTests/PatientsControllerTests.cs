using Microsoft.AspNetCore.Mvc;
using PatientService.Controllers;
using PatientService.Dtos;
using PatientService.Models;
using PatientService.Services;

namespace PatientService.UnitTests;

public class PatientsControllerTests
{
    private static readonly CancellationToken _ct = CancellationToken.None;

    private static readonly PatientDto _sampleDto =
        new(7, "Jane", "Doe", new DateOnly(1990, 5, 20), Gender.M, null, null);

    private static readonly PatientRequest _sampleRequest =
        new("Jane", "Doe", new DateOnly(1990, 5, 20), Gender.M, null, null);

    private sealed class FakePatientsService : IPatientsService
    {
        public PatientDto? Dto { get; set; } = _sampleDto;

        public Exception? ExceptionToThrow { get; set; }

        public int LastPage { get; private set; }

        public int LastPageSize { get; private set; }

        public int LastId { get; private set; }

        public CancellationToken LastToken { get; private set; }

        public Task<PagedResult<PatientDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            LastPage = page;
            LastPageSize = pageSize;
            LastToken = cancellationToken;
            ThrowIfConfigured();
            return Task.FromResult(new PagedResult<PatientDto>([_sampleDto], page, pageSize, 1));
        }

        public Task<PatientDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            LastId = id;
            LastToken = cancellationToken;
            return Task.FromResult(Dto);
        }

        public Task<PatientDto> CreateAsync(PatientRequest request, CancellationToken cancellationToken)
        {
            LastToken = cancellationToken;
            ThrowIfConfigured();
            return Task.FromResult(Dto ?? throw new InvalidOperationException("Dto must be set."));
        }

        public Task<PatientDto?> UpdateAsync(int id, PatientRequest request, CancellationToken cancellationToken)
        {
            LastId = id;
            LastToken = cancellationToken;
            ThrowIfConfigured();
            return Task.FromResult(Dto);
        }

        private void ThrowIfConfigured()
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }
        }
    }

    private static (PatientsController Controller, FakePatientsService Service) CreateController()
    {
        var service = new FakePatientsService();
        return (new PatientsController(service), service);
    }

    private static void AssertBadRequest(IActionResult? result)
    {
        var problem = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(400, problem.StatusCode);
    }

    // ----- GetPage -----

    [Fact]
    public async Task GetPageReturnsOkWithPagedResult()
    {
        var (controller, service) = CreateController();

        var result = await controller.GetPage(2, 5, _ct);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var page = Assert.IsType<PagedResult<PatientDto>>(ok.Value);
        Assert.Equal(2, page.Page);
        Assert.Equal(2, service.LastPage);
        Assert.Equal(5, service.LastPageSize);
    }

    [Fact]
    public async Task GetPageWithInvalidPagingReturnsBadRequest()
    {
        var (controller, service) = CreateController();
        service.ExceptionToThrow = new ArgumentOutOfRangeException("page");

        var result = await controller.GetPage(0, 5, _ct);

        AssertBadRequest(result.Result);
    }

    // ----- GetById -----

    [Fact]
    public async Task GetByIdWithExistingPatientReturnsOk()
    {
        var (controller, service) = CreateController();

        var result = await controller.GetById(7, _ct);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(_sampleDto, ok.Value);
        Assert.Equal(7, service.LastId);
    }

    [Fact]
    public async Task GetByIdWithUnknownIdReturnsNotFound()
    {
        var (controller, service) = CreateController();
        service.Dto = null;

        var result = await controller.GetById(42, _ct);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetByIdPassesCancellationTokenToService()
    {
        var (controller, service) = CreateController();
        using var cts = new CancellationTokenSource();

        await controller.GetById(7, cts.Token);

        Assert.Equal(cts.Token, service.LastToken);
    }

    // ----- Create -----

    [Fact]
    public async Task CreateWithValidRequestReturnsCreatedAtGetById()
    {
        var (controller, _) = CreateController();

        var result = await controller.Create(_sampleRequest, _ct);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal("GetById", created.ActionName);
        Assert.Equal(7, created.RouteValues?["id"]);
        Assert.Equal(_sampleDto, created.Value);
    }

    [Fact]
    public async Task CreateWithInvalidRequestReturnsBadRequest()
    {
        var (controller, service) = CreateController();
        service.ExceptionToThrow = new ArgumentException("FirstName is required.");

        var result = await controller.Create(_sampleRequest, _ct);

        AssertBadRequest(result.Result);
    }

    // ----- Update -----

    [Fact]
    public async Task UpdateWithExistingPatientReturnsOk()
    {
        var (controller, service) = CreateController();

        var result = await controller.Update(7, _sampleRequest, _ct);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(_sampleDto, ok.Value);
        Assert.Equal(7, service.LastId);
    }

    [Fact]
    public async Task UpdateWithUnknownIdReturnsNotFound()
    {
        var (controller, service) = CreateController();
        service.Dto = null;

        var result = await controller.Update(42, _sampleRequest, _ct);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateWithInvalidRequestReturnsBadRequest()
    {
        var (controller, service) = CreateController();
        service.ExceptionToThrow = new ArgumentException("LastName is required.");

        var result = await controller.Update(7, _sampleRequest, _ct);

        AssertBadRequest(result.Result);
    }
}
