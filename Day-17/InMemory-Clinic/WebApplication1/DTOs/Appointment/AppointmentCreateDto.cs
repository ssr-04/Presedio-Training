using System.ComponentModel.DataAnnotations;

public class AppointmentCreateDto
{
    [Required(ErrorMessage = "Patient ID is required.")]
    public Guid PatientId { get; set; }

    [Required(ErrorMessage = "Doctor ID is required.")]
    public Guid DoctorId { get; set; }

    [Required(ErrorMessage = "Appointment date and time is required.")]
    [DataType(DataType.DateTime)]
    public DateTime AppointmentDateTime { get; set; }

    [Required(ErrorMessage = "Reason for appointment is required.")]
    [StringLength(100, ErrorMessage = "Reason cannot exceed 100 characters.")]
    public string Reason { get; set; } = string.Empty;

    // Status can be set to Scheduled by default if not provided
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
}