using System.Text.Json.Serialization;
using PatientService.Models;

namespace PatientService.Dtos;

public sealed record PatientRequest(
    string FirstName,
    string LastName,
    [property: JsonRequired] DateOnly BirthDate,
    [property: JsonRequired] Gender Gender,
    string? Address,
    string? PhoneNumber);
