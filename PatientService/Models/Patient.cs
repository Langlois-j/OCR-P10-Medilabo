namespace PatientService.Models;

public sealed class Patient
{
    private Patient(string firstName, string lastName, DateOnly birthDate, Gender gender, string? address, string? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Gender = gender;
        Address = address;
        PhoneNumber = phoneNumber;
    }

    public int Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public Gender Gender { get; private set; }
    public string? Address { get; private set; }
    public string? PhoneNumber { get; private set; }

    public static Patient Create(
    string firstName,
    string lastName,
    DateOnly birthDate,
    Gender gender,
    TimeProvider timeProvider,
    string? address = null,
    string? phoneNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        if (!Enum.IsDefined(gender))
        {
            throw new ArgumentOutOfRangeException(nameof(gender));
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(birthDate, today);

        return new Patient(firstName.Trim(), lastName.Trim(), birthDate, gender, address, phoneNumber);
    }
}
