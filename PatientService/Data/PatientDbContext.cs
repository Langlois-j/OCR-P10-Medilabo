using Microsoft.EntityFrameworkCore;
using PatientService.Models;

namespace PatientService.Data;

public sealed class PatientDbContext(DbContextOptions<PatientDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(patient =>
        {
            patient.ToTable("Patients");
            patient.HasKey(p => p.Id);

            patient.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            patient.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            patient.Property(p => p.BirthDate).IsRequired();
            patient.Property(p => p.Gender).IsRequired().HasConversion<string>().HasMaxLength(1);
            patient.Property(p => p.Address).HasMaxLength(200);
            patient.Property(p => p.PhoneNumber).HasMaxLength(20);
        });
    }
}
