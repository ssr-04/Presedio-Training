using System.ComponentModel.DataAnnotations;

public class DoctorCreateDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(30, ErrorMessage = "Name cannot exceed 30 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required.")]
    [StringLength(30, ErrorMessage = "Status cannot exceed 30 characters.")]
    public string Status { get; set; } = string.Empty;

    [Range(0.0, 60.0, ErrorMessage = "Years of experience must be between 0 and 60.")]
    public float YearsOfExperience { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(75, ErrorMessage = "Email cannot exceed 75 characters.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "At least one Specialty ID is required.")]
    public List<int> SpecialtyIds { get; set; } = new List<int>();
}