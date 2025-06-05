using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
        [Key] // Explicitly define Id as primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-incrementing in DB
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Username { get; set; } = string.Empty; // Email address

        [Required] 
        [StringLength(256)]
        public string Auth0UserId { get; set; } = string.Empty; // Auth0's unique user ID (sub claim)

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty; // E.g., "HR_Admin", "Standard_User"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }


}