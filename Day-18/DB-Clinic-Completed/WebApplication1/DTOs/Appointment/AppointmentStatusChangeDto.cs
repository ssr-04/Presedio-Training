using System.ComponentModel.DataAnnotations;

public class AppointmentStatusChangeDto
{
    [Required]
    [StringLength(50)]
    public string NewStatus { get; set; } = string.Empty;
}