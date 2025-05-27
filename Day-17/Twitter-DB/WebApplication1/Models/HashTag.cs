using System.ComponentModel.DataAnnotations;

namespace FirstTwitterApp.Models
{
    public class Hashtag
    {
        [Key]
        public int Id { get; set; } // Primary key

        [Required]
        [StringLength(25, ErrorMessage = "Hashtag text cannot exceed 25 characters.")]
        public string Text { get; set; } = string.Empty; 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false; // Soft delete flag

        // Navigation property for many-to-many
        public ICollection<TweetHashtag>? TweetHashtags { get; set; }
    }
}