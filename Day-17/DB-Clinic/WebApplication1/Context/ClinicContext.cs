using Microsoft.EntityFrameworkCore;
public class ClinicContext : DbContext
{
    public ClinicContext(DbContextOptions options) :base(options)
    {
        
    }

    public DbSet<Patient> Patients { get; set; }

    public DbSet<Doctor> Doctors { get; set; }

    public DbSet<Speciality> Specialities { get; set; }

    public DbSet<DoctorSpeciality> DoctorSpecialities { get; set; } 
    
    public DbSet<Appointment> Appointments { get; set; } 
}

// dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
// dotnet add package Microsoft.EntityFrameworkCore

// dotnet ef migrations add init
// dotnet ef database update
