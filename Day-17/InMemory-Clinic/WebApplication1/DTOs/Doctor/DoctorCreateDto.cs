using System.ComponentModel.DataAnnotations;

public class DoctorCreateDto
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(20, ErrorMessage = "First name cannot exceed 20 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(20, ErrorMessage = "Last name cannot exceed 20 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Specialty is required.")]
    [StringLength(30, ErrorMessage = "Specialty cannot exceed 30 characters.")]
    public string Specialty { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(75, ErrorMessage = "Email cannot exceed 75 characters.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string PhoneNumber { get; set; } = string.Empty;
}