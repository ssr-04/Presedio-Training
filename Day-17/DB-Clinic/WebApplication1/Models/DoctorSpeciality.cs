using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class DoctorSpeciality
{
    [Key] // Explicitly defining SerialNumber as the primary key
    public int SerialNumber { get; set; }

    public int DoctorId { get; set; }
    public int SpecialityId { get; set; }

    // Navigation properties
    [ForeignKey("SpecialityId")]
    public Speciality? Speciality { get; set; }
    [ForeignKey("DoctorId")]
    public Doctor? Doctor { get; set; }
}