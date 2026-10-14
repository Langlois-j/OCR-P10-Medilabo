using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PatientService.Migrations
{
    /// <inheritdoc />
    public partial class SeedPatients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "Address", "BirthDate", "FirstName", "Gender", "LastName", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "1 Brookside St", new DateOnly(1966, 12, 31), "Test", "F", "TestNone", "100-222-3333" },
                    { 2, "2 High St", new DateOnly(1945, 6, 24), "Test", "M", "TestBorderline", "200-333-4444" },
                    { 3, "3 Club Road", new DateOnly(2004, 6, 18), "Test", "M", "TestInDanger", "300-444-5555" },
                    { 4, "4 Valley Dr", new DateOnly(2002, 6, 28), "Test", "F", "TestEarlyOnset", "400-555-6666" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
