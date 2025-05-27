using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstTwitterApp.Models
{
    public class Tweet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign key to User who posted the tweet

        [Required]
        [StringLength(750, ErrorMessage = "Tweet content cannot exceed 750 characters.")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false; // Soft delete flag

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; } // The user who posted this tweet
        public ICollection<Like>? Likes { get; set; } // Likes on this tweet
        public ICollection<TweetHashtag>? TweetHashtags { get; set; } // Hashtags used in this tweet
    }
}