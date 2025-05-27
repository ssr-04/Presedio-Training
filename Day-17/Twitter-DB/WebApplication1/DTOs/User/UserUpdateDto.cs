using System.ComponentModel.DataAnnotations;

namespace FirstTwitterApp.DTOs.User
{
    public class UserUpdateDto
    {
        [StringLength(25, ErrorMessage = "Display name cannot exceed 25 characters.")]
        public string? DisplayName { get; set; }

        [StringLength(250, ErrorMessage = "Bio cannot exceed 250 characters.")]
        public string? Bio { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(75, ErrorMessage = "Email cannot exceed 75 characters.")]
        public string? Email { get; set; } // Optional update

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(25, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; } = string.Empty;
    }
}