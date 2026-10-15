using PatientService.Models;

namespace PatientService.Dtos;

public sealed record PatientRequest(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    Gender Gender,
    string? Address,
    string? PhoneNumber);
