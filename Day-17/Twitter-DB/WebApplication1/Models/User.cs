using System.ComponentModel.DataAnnotations;

namespace FirstTwitterApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; } // Using int for simplicity and Identity column

        [Required]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 25 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(75, ErrorMessage = "Email cannot exceed 75 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255, ErrorMessage = "Password hash cannot exceed 255 characters.")]
        public string PasswordHash { get; set; } = string.Empty; // Storing hashed passwords

        [StringLength(20, ErrorMessage = "Display name cannot exceed 20 characters.")]
        public string? DisplayName { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false; // Soft delete flag

        // Navigation properties
        public ICollection<Tweet>? Tweets { get; set; } // Tweets posted by this user
        public ICollection<Like>? Likes { get; set; } // Likes made by this user
        public ICollection<Follow>? Following { get; set; } // Users this user is following 
        public ICollection<Follow>? Followers { get; set; } // Users who are following this user
    }
}