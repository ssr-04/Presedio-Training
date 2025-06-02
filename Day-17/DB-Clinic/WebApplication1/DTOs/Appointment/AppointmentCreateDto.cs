using System.ComponentModel.DataAnnotations;

public class AppointmentCreateDto
{

    [Required(ErrorMessage = "Patient ID is required.")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Doctor ID is required.")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Appointment date and time is required.")]
    [RegularExpression(@"^\d{2}-\d{2}-\d{4} \d{2}:\d{2}$", ErrorMessage = "Appointment time must be in 'dd-MM-yyyy hh:mm' format.")]
    public string AppointmentDateTimeString { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required.")]
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
    public string Status { get; set; } = "Scheduled"; // e.g., "Scheduled", "Confirmed", "Completed", "Cancelled"
}