using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Appointment
{
    [Key]
    public string AppointmentNumber { get; set; } = string.Empty; 

    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string Status { get; set; } = string.Empty; // "Scheduled", "Confirmed", "Completed", "Cancelled"
    public bool IsDeleted { get; set; } = false; // Soft delete flag

    // Navigation properties
    [ForeignKey("DoctorId")]
    public Doctor? Doctor { get; set; }
    
    [ForeignKey("PatientId")]
    public Patient? Patient { get; set; }
}