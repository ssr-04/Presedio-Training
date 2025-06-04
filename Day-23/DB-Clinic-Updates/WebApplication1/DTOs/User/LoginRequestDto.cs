
using System.ComponentModel.DataAnnotations;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Username (email) is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}