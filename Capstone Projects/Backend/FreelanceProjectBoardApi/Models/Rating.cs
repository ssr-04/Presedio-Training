using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.Models
{
    public class Rating : BaseEntity
    {
        [Required]
        public Guid ProjectId { get; set; } // for which project
        public Project Project { get; set; } = default!; // Navigation property

        [Required]
        public Guid RaterId { get; set; } // FK to User who is giving the rating
        public User Rater { get; set; } = default!; // Navigation property

        [Required]
        public Guid RateeId { get; set; } // FK to User who is being rated
        public User Ratee { get; set; } = default!; // Navigation property

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int RatingValue { get; set; } 

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
