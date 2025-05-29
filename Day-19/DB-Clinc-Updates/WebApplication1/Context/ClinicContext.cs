using Microsoft.EntityFrameworkCore;
public class ClinicContext : DbContext
{
    public ClinicContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Speciality> Specialities { get; set; }
    public DbSet<DoctorSpeciality> DoctorSpecialities { get; set; }
    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Configure Patient Entity
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.Id); //Primary key
            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(25);
            entity.Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(75);
            entity.Property(p => p.Phone)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(p => p.Age)
                .IsRequired();
            entity.Property(p => p.IsDeleted)
                .HasDefaultValue(false);
        });

        // Configure Doctor Entity
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.Id); // Primary Key
            entity.Property(d => d.Name)
                  .IsRequired()
                  .HasMaxLength(25);
            entity.Property(d => d.Status)
                  .HasMaxLength(50)
                  .HasDefaultValue("Active"); // e.g., "Active", "On Leave"
            entity.Property(d => d.Email)
                  .IsRequired()
                  .HasMaxLength(75);
            entity.Property(d => d.Phone)
                  .IsRequired()
                  .HasMaxLength(20);
            entity.Property(d => d.IsDeleted)
                  .HasDefaultValue(false);
        });

        // Configure Speciality Entity
        modelBuilder.Entity<Speciality>(entity =>
        {
            entity.HasKey(s => s.Id); // Primary Key
            entity.Property(s => s.Name)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(s => s.Status)
                  .HasMaxLength(50);
            entity.Property(s => s.IsDeleted)
                  .HasDefaultValue(false);
        });

        // Configure DoctorSpeciality (Many-to-Many join table)
        modelBuilder.Entity<DoctorSpeciality>(entity =>
        {
            entity.HasKey(ds => ds.SerialNumber); // Primary Key

            // Many-to-One relationship with Doctor
            entity.HasOne(ds => ds.Doctor)
                  .WithMany(d => d.DoctorSpecialities)
                  .HasForeignKey(ds => ds.DoctorId)
                  .HasConstraintName("FK_Doctor_DoctorSpeciality")
                  .OnDelete(DeleteBehavior.Cascade); // If a doctor is deleted, their DoctorSpeciality entries are deleted

            // Many-to-One relationship with Speciality
            entity.HasOne(ds => ds.Speciality)
                  .WithMany(s => s.DoctorSpecialities)
                  .HasForeignKey(ds => ds.SpecialityId)
                  .HasConstraintName("FK_Speciality_DoctorSpeciality")
                  .OnDelete(DeleteBehavior.Cascade); // If a speciality is deleted, its DoctorSpeciality entries are deleted

        });

        // Configure Appointment Entity
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.AppointmentNumber); // Primary Key (using string)
            entity.Property(a => a.AppointmentNumber)
                  .HasMaxLength(50); // Set max length for AppointmentNumber

            entity.Property(a => a.Status)
                  .HasMaxLength(50)
                  .IsRequired(); // Status is required
            entity.Property(a => a.IsDeleted)
                  .HasDefaultValue(false);

            // Foreign Key relationship with Patient
            entity.HasOne(a => a.Patient)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PatientId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Foreign Key relationship with Doctor
            entity.HasOne(a => a.Doctor)
                  .WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
        });
    }
}

// dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
// dotnet add package Microsoft.EntityFrameworkCore

// dotnet ef migrations add init
// dotnet ef database update
