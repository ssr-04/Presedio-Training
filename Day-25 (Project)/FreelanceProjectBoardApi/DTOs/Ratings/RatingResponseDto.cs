using FreelanceProjectBoardApi.DTOs.Projects;
using FreelanceProjectBoardApi.DTOs.Users;

namespace FreelanceProjectBoardApi.DTOs.Ratings
{
    public class RatingResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid RaterId { get; set; }
        public Guid RateeId { get; set; }
        public int RatingValue { get; set; }
        public string? Comment { get; set; }
        public ProjectListDto? Project { get; set; } // Basic project info
        public UserResponseDto? Rater { get; set; } // Basic rater info
        public UserResponseDto? Ratee { get; set; } // Basic ratee info
        public DateTime CreatedAt { get; set; }
    }
}
