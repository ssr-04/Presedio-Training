using System.ComponentModel.DataAnnotations;

namespace FirstTwitterApp.DTOs.Hashtag
{
    public class HashtagCreateDto
    {
        [Required(ErrorMessage = "Hashtag text is required.")]
        [StringLength(50, ErrorMessage = "Hashtag text cannot exceed 25 characters.")]
        public string Text { get; set; } = string.Empty;
    }
}