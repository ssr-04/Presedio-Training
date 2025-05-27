using System.ComponentModel.DataAnnotations;

public class AppointmentUpdateDto
{
    [Required(ErrorMessage = "Id is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Patient ID is required.")]
    public Guid PatientId { get; set; }

    [Required(ErrorMessage = "Doctor ID is required.")]
    public Guid DoctorId { get; set; }

    [Required(ErrorMessage = "Appointment date and time is required.")]
    public DateTime AppointmentDateTime { get; set; }

    [Required(ErrorMessage = "Reason for appointment is required.")]
    [StringLength(100, ErrorMessage = "Reason cannot exceed 100 characters.")]
    public string Reason { get; set; } = string.Empty;

    [Required(ErrorMessage = "Appointment status is required.")]
    public AppointmentStatus Status { get; set; }
}