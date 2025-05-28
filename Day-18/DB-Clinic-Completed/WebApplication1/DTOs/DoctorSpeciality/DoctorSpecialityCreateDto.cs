using System.ComponentModel.DataAnnotations;

public class DoctorSpecialityCreateDto
{
    [Required(ErrorMessage = "Doctor ID is required.")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Speciality ID is required.")]
    public int SpecialityId { get; set; }
}
