using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstTwitterApp.Models
{
    public class Follow
    {
        [Key]
        public int Id { get; set; } // Primary key

        [Required]
        public int FollowerId { get; set; } // User who is following

        [Required]
        public int FolloweeId { get; set; } // User who is being followed

        public DateTime FollowedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("FollowerId")]
        public User? Follower { get; set; }

        [ForeignKey("FolloweeId")]
        public User? Followee { get; set; }
    }
}