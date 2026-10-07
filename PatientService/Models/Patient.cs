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

    public static Patient Create(string firstName, string lastName, DateOnly birthDate, Gender gender, string? address = null, string? phoneNumber = null)
        => new(firstName, lastName, birthDate, gender, address, phoneNumber);
}
