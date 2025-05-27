using System.ComponentModel.DataAnnotations;

namespace FirstTwitterApp.DTOs.Tweet
{
    public class TweetUpdateDto
    {
        [Required(ErrorMessage = "Tweet content is required.")]
        [StringLength(280, ErrorMessage = "Tweet content cannot exceed 280 characters.")]
        public string Content { get; set; } = string.Empty;
    }
}