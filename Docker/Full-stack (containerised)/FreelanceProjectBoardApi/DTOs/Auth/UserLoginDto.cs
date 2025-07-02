using System.ComponentModel.DataAnnotations;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.DTOs.Auth
{
    public class UserLoginDto
    {
        [Required, EmailAddress(ErrorMessage ="Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
    }
}