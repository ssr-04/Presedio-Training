using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.DTOs.Ratings
{
    public class CreateRatingDto
    {
        [Required]
        public Guid ProjectId { get; set; } // The project the rating is for

        // RaterId will be derived from authenticated user

        [Required]
        public Guid RateeId { get; set; } // The user being rated (freelancer or client)

        [Required, Range(1, 5)]
        public int RatingValue { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
