
using System.ComponentModel.DataAnnotations;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    // In a real app, add more complexity requirements (e.g., regex for special chars, numbers)
    public string Password { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email { get; set; }
}

public class LoginRequestDto
{
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = null!;
}

public class AuthResponseDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Token { get; set; } = null!; // The JWT token
}