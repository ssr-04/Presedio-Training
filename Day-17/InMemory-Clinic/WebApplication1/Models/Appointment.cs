public class Appointment
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; } // Foreign key to Patient
    public Guid DoctorId { get; set; } // Foreign key to Doctor
    public DateTime AppointmentDateTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled; // Default status
}

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    Completed,
    Cancelled
}