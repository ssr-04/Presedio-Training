using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstTwitterApp.Models
{
    public class TweetHashtag
    {
        [Key]
        public int Id { get; set; } // Primary key

        [Required]
        public int TweetId { get; set; } // Foreign key to Tweet

        [Required]
        public int HashtagId { get; set; } // Foreign key to Hashtag

        // Navigation properties
        [ForeignKey("TweetId")]
        public Tweet? Tweet { get; set; }
        [ForeignKey("HashtagId")]
        public Hashtag? Hashtag { get; set; }
    }
}