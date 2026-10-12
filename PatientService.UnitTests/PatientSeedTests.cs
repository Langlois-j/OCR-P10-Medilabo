using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.UnitTests;

public class PatientSeedTests
{
    private static List<IDictionary<string, object?>> GetSeed()
    {
        var options = new DbContextOptionsBuilder<PatientDbContext>()
            .UseSqlServer("Server=.;Database=unused")
            .Options;
        using var context = new PatientDbContext(options);

        return context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(Patient))!
            .GetSeedData()
            .ToList();
    }

    [Fact]
    public void SeedContainsFourPatientsWithStableIds()
    {
        var ids = GetSeed().Select(p => (int)p["Id"]!).Order().ToList();

        Assert.Equal([1, 2, 3, 4], ids);
    }

    [Theory]
    [InlineData(1, "TestNone", Gender.F, "1966-12-31")]
    [InlineData(2, "TestBorderline", Gender.M, "1945-06-24")]
    [InlineData(3, "TestInDanger", Gender.M, "2004-06-18")]
    [InlineData(4, "TestEarlyOnset", Gender.F, "2002-06-28")]
    public void SeedMatchesTestCaseData(int id, string lastName, Gender gender, string birthDate)
    {
        var patient = GetSeed().Single(p => (int)p["Id"]! == id);

        Assert.Equal(lastName, patient["LastName"]);
        Assert.Equal("Test", patient["FirstName"]);
        Assert.Equal(gender, patient["Gender"]);
        Assert.Equal(DateOnly.Parse(birthDate, CultureInfo.InvariantCulture), patient["BirthDate"]);
    }
}
