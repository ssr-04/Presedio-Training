public class DoctorSpecialityResponseDto
{
    public int SerialNumber { get; set; } 

    public int DoctorId { get; set; }
    public string? DoctorName { get; set; } // Need to include

    public int SpecialityId { get; set; }
    public string? SpecialityName { get; set; } // Need to include
}
