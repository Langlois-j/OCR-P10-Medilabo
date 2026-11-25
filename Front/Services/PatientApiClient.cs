using System.Net.Http.Headers;
using Front.Models;

namespace Front.Services;

public sealed class PatientApiClient(IHttpClientFactory httpClientFactory) : IPatientApiClient
{
    public async Task<PagedResult<PatientSummary>> GetPatientsAsync(
        int page,
        int pageSize,
        string? token,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Gateway");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"patients?page={page}&pageSize={pageSize}");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PagedResult<PatientSummary>>(cancellationToken)
            ?? throw new InvalidOperationException("Empty response from the gateway.");
    }
}
