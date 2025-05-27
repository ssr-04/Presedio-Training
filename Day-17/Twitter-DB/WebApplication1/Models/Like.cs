using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstTwitterApp.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; } // Primary key

        [Required]
        public int UserId { get; set; } // Foreign key to User who liked the tweet

        [Required]
        public int TweetId { get; set; } // Foreign key to Tweet that was liked

        public DateTime LikedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }
        [ForeignKey("TweetId")]
        public Tweet? Tweet { get; set; }
    }
}