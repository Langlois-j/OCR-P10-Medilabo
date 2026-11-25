namespace Front.Models;

public sealed record PatientSummary(
    int Id,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Gender,
    string? Address,
    string? PhoneNumber);
