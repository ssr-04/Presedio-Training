public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
    public string PatientName { get; set; } = string.Empty; // To be filled by service
    public string DoctorName { get; set; } = string.Empty; // To be filled by service
}