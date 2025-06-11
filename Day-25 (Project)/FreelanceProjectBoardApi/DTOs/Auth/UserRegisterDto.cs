using System.ComponentModel.DataAnnotations;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.DTOs.Auth
{
    public class UserRegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string UserType { get; set; } = string.Empty;
    }
}