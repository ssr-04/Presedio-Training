using System.ComponentModel.DataAnnotations;

namespace FirstTwitterApp.DTOs.User
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Username or Email is required.")]
        public string Identifier { get; set; } = string.Empty; // Can be username or email

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}