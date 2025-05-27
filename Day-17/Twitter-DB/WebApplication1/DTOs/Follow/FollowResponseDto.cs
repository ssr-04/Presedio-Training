using FirstTwitterApp.DTOs.User;

namespace FirstTwitterApp.DTOs.Follow
{
    public class FollowResponseDto
    {
        public int Id { get; set; }
        public int FollowerId { get; set; }
        public UserResponseDto? Follower { get; set; }
        public int FolloweeId { get; set; }
        public UserResponseDto? Followee { get; set; }
        public DateTime FollowedAt { get; set; }
    }
}