using PatientService.Models;

namespace PatientService.UnitTests;

public class PatientTests
{
    [Fact]
    public void CreateWithValidRequiredFieldsReturnsPatientWithoutOptionalFields()
    {
        // Arrange
        var birthDate = new DateOnly(1966, 12, 31);

        // Act
        var patient = Patient.Create("Test", "TestNone", birthDate, Gender.F);

        // Assert
        Assert.Equal("Test", patient.FirstName);
        Assert.Equal("TestNone", patient.LastName);
        Assert.Equal(birthDate, patient.BirthDate);
        Assert.Equal(Gender.F, patient.Gender);
        Assert.Null(patient.Address);
        Assert.Null(patient.PhoneNumber);
    }
}
