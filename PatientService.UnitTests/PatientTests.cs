using PatientService.Models;


namespace PatientService.UnitTests;

public class PatientTests
{

    private const string _defaultFirstName = "firstName";
    private const string _defaultLastName = "lastName";
    private static readonly DateOnly _defaultBirthDate = new(1966, 12, 31);
    private const Gender _defaultGender = Gender.F;
    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private static readonly TimeProvider _clock =
        new FixedTimeProvider(new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero));

    private static Patient CreateValidPatient(
        string? firstName = _defaultFirstName,
        string? lastName = _defaultLastName,
        DateOnly? birthDate = null,
        Gender? gender = null,
        string? address = null,
        string? phoneNumber = null)
    {
        return Patient.Create(
            firstName!,
            lastName!,
            birthDate ?? _defaultBirthDate,
            gender ?? _defaultGender,
            _clock,        
            address,
            phoneNumber);
    }

    [Fact]
    public void CreateWithValidRequiredFieldsReturnsPatientWithoutOptionalFields()
    {
        // Act
        var patient = CreateValidPatient();

        // Assert
        Assert.Equal(_defaultFirstName, patient.FirstName);
        Assert.Equal(_defaultLastName, patient.LastName);
        Assert.Equal(_defaultBirthDate, patient.BirthDate);
        Assert.Equal(_defaultGender, patient.Gender);
        Assert.Null(patient.Address);
        Assert.Null(patient.PhoneNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateWithBlankFirstNameThrowsArgumentException(string? invalidValue)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => CreateValidPatient(firstName: invalidValue));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateWithBlankLastNameThrowsArgumentException(string? invalidValue)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => CreateValidPatient(lastName: invalidValue));
    }

    [Fact]
    public void CreateWithBirthDateTomorrowThrowsArgumentOutOfRangeException()
    {
        // 2026-10-08 est le lendemain de l'horloge figée
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateValidPatient(birthDate: new DateOnly(2026, 10, 8)));
    }

    [Fact]
    public void CreateWithBirthDateTodayKeepsBirthDate()
    {
        var today = new DateOnly(2026, 10, 7);

        var patient = CreateValidPatient(birthDate: today);

        Assert.Equal(today, patient.BirthDate);
    }

    [Fact]
    public void CreateWithUndefinedGenderThrowsArgumentOutOfRangeException()
        => Assert.Throws<ArgumentOutOfRangeException>(() => CreateValidPatient(gender: (Gender)99));

    [Fact]
    public void CreateWithAddressAndPhoneNumberKeepsOptionalFields()
    {
        // Arrange
        const string expectedAddress = "123 Rue de la Paix, 75000 Paris";
        const string expectedPhoneNumber = "0123456789";

        // Act
        var patient = CreateValidPatient(
            address: expectedAddress,
            phoneNumber: expectedPhoneNumber
        );

        // Assert
        Assert.Equal(expectedAddress, patient.Address);
        Assert.Equal(expectedPhoneNumber, patient.PhoneNumber);
    }

    [Fact]
    public void CreateTrimsNamesSurroundingWhitespace()
    {
        // Arrange

        const string expectedFirstName = "Test";
        const string expectedLastName = "User";
        const string rawFirstName = "  "+ expectedFirstName + "   ";
        const string rawLastName = "  "+ expectedLastName + "  ";

        // Act
        var patient = CreateValidPatient(
            firstName: rawFirstName,
            lastName: rawLastName
        );

        // Assert
        Assert.Equal(expectedFirstName, patient.FirstName);
        Assert.Equal(expectedLastName, patient.LastName);
    }
}
