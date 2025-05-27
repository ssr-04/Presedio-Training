using FirstTwitterApp.DTOs.User; 
using FirstTwitterApp.DTOs.Hashtag; 

namespace FirstTwitterApp.DTOs.Tweet
{
    public class TweetResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public UserResponseDto? User { get; set; } // Including user details
        public int LikeCount { get; set; } // Number of likes
        public List<HashtagResponseDto> Hashtags { get; set; } = new List<HashtagResponseDto>(); // Associated hashtags
    }
}