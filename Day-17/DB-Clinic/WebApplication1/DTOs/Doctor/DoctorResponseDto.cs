public class DoctorResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public float YearsOfExperience { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public List<SpecialityResponseDto> Specialties { get; set; } = new List<SpecialityResponseDto>();
}