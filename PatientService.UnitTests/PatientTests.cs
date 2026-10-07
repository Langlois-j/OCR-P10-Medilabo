using System.Reflection;
using PatientService.Models;

namespace PatientService.Tests;

public class PatientTests
{
    [Fact]
    public void Create_WithValidRequiredFields_ReturnsPatientWithoutOptionalFields()
    {
        // Arrange
        var DateNaissance = new DateOnly(1966, 12, 31);

        // Act
        var patient = Patient.Create("Test", "TestNone", DateNaissance, Genre.F);

        // Assert
        Assert.Equal("Test", patient.Nom);
        Assert.Equal("TestNone", patient.Prenom);
        Assert.Equal(DateNaissance, patient.DateNaissance);
        Assert.Equal(Genre.F, patient.Genre);
        Assert.Null(patient.Adresse);
        Assert.Null(patient.Telephone);
    }
}
