using Front.Services;
using Microsoft.AspNetCore.Mvc;

namespace Front.Controllers;

public sealed class PatientsController(IPatientApiClient patientApi, IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        const int pageSize = 10;

        // TODO(séance 10): lire le jeton dans le cookie d'authentification
        var token = configuration["Dev:Token"];

        var result = await patientApi.GetPatientsAsync(Math.Max(page, 1), pageSize, token, cancellationToken);
        return View(result);
    }
}
