using System.ComponentModel.DataAnnotations;

public class AppointmentRescheduleDto
{
    [Required(ErrorMessage = "New timing is required.")]
    [RegularExpression(@"^\d{2}-\d{2}-\d{4} \d{2}:\d{2}$", ErrorMessage = "New timing must be in 'dd-MM-yyyy hh:mm' format.")]
    public string NewTimingString { get; set; } = string.Empty;
}
