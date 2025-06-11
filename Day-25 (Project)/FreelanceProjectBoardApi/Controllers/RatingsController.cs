using FreelanceProjectBoardApi.DTOs.Ratings;
using FreelanceProjectBoardApi.Models; // For UserType
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceProjectBoardApi.Controllers
{
    [Authorize]
    public class RatingsController : BaseApiController
    {
        private readonly IRatingService _ratingService;
        private readonly IProjectService _projectService; // For context and authorization

        public RatingsController(IRatingService ratingService, IProjectService projectService)
        {
            _ratingService = ratingService;
            _projectService = projectService;
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RatingResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var raterId = GetUserId();

            // logic for who can rate whom for which project is handled in the service.
            var rating = await _ratingService.CreateRatingAsync(raterId, createDto);
            if (rating == null)
            {
                return BadRequest(new { message = "Failed to create rating. Invalid project status, rater/ratee roles, or rating already exists." });
            }
            return CreatedAtAction(nameof(GetRatingById), new { id = rating.Id }, rating);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RatingResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRatingById(Guid id)
        {
            var rating = await _ratingService.GetRatingByIdAsync(id);
            if (rating == null)
            {
                return NotFound($"Rating with ID {id} not found.");
            }
            return Ok(rating);
        }


        [HttpGet("received-by/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RatingResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRatingsReceivedByUserId(Guid userId)
        {
            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            if (userId != currentUserId && currentUserType != UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to view ratings received by this user.");
            }

            var ratings = await _ratingService.GetRatingsReceivedByUserIdAsync(userId);
            return Ok(ratings);
        }


        [HttpGet("given-by/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RatingResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRatingsGivenByUserId(Guid userId)
        {
            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            if (userId != currentUserId && currentUserType != UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to view ratings given by this user.");
            }

            var ratings = await _ratingService.GetRatingsGivenByUserIdAsync(userId);
            return Ok(ratings);
        }

        [HttpGet("average-for/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(double))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAverageRatingForUser(Guid userId)
        {
            var averageRating = await _ratingService.GetAverageRatingForUserAsync(userId);
            // If the user has no ratings, this might return 0.0.
            return Ok(averageRating);
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRating(Guid id)
        {
            var rating = await _ratingService.GetRatingByIdAsync(id);
            if (rating == null)
            {
                return NotFound($"Rating with ID {id} not found.");
            }

            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            if (rating.RaterId != currentUserId && currentUserType != UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to delete this rating.");
            }

            var success = await _ratingService.DeleteRatingAsync(id);
            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete rating.");
            }
            return NoContent();
        }
    }
}
