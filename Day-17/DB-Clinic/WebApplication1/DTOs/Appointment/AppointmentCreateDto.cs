using System.ComponentModel.DataAnnotations;

public class AppointmentCreateDto
{
    [Required(ErrorMessage = "Appointment Number is required.")]
    [StringLength(15, ErrorMessage = "Appointment Number cannot exceed 15 characters.")]
    public string AppointmentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Patient ID is required.")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Doctor ID is required.")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Appointment date and time is required.")]
    public DateTime AppointmentDateTime { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
    public string Status { get; set; } = string.Empty; // e.g., "Scheduled", "Confirmed", "Completed", "Cancelled"
}