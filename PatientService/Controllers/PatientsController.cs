using Microsoft.AspNetCore.Mvc;
using PatientService.Dtos;
using PatientService.Services;

namespace PatientService.Controllers;

[ApiController]
[Route("patients")]
public sealed class PatientsController(IPatientsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PatientDto>>> GetPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await service.GetPageAsync(page, pageSize, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return InvalidRequest(ex);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatientDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var patient = await service.GetByIdAsync(id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost]
    public async Task<ActionResult<PatientDto>> Create(
        PatientRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var patient = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
        }
        catch (ArgumentException ex)
        {
            return InvalidRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PatientDto>> Update(
        int id,
        PatientRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var patient = await service.UpdateAsync(id, request, cancellationToken);
            return patient is null ? NotFound() : Ok(patient);
        }
        catch (ArgumentException ex)
        {
            return InvalidRequest(ex);
        }
    }

    private ObjectResult InvalidRequest(ArgumentException exception)
        => Problem(statusCode: StatusCodes.Status400BadRequest, detail: exception.Message);
}
