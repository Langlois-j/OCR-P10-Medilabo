using PatientService.Models;

namespace PatientService.Data;

internal static class PatientSeed
{
    internal static readonly object[] Patients =
    [
        new { Id = 1, FirstName = "Test", LastName = "TestNone", BirthDate = new DateOnly(1966, 12, 31), Gender = Gender.F, Address = "1 Brookside St", PhoneNumber = "100-222-3333" },
        new { Id = 2, FirstName = "Test", LastName = "TestBorderline", BirthDate = new DateOnly(1945, 6, 24), Gender = Gender.M, Address = "2 High St", PhoneNumber = "200-333-4444" },
        new { Id = 3, FirstName = "Test", LastName = "TestInDanger", BirthDate = new DateOnly(2004, 6, 18), Gender = Gender.M, Address = "3 Club Road", PhoneNumber = "300-444-5555" },
        new { Id = 4, FirstName = "Test", LastName = "TestEarlyOnset", BirthDate = new DateOnly(2002, 6, 28), Gender = Gender.F, Address = "4 Valley Dr", PhoneNumber = "400-555-6666" },
    ];
}
