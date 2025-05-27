public class AppointmentResponseDto
{
    public string AppointmentNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public string PatientName { get; set; } = string.Empty; // To be populated by service
    public string DoctorName { get; set; } = string.Empty; // To be populated by service
}