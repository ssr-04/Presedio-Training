using FreelanceProjectBoardApi.DTOs.FreelancerProfiles;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceProjectBoardApi.Controllers
{
    [Authorize]
    public class FreelancerProfilesController : BaseApiController
    {
        private readonly IFreelancerProfileService _freelancerProfileService;

        public FreelancerProfilesController(IFreelancerProfileService freelancerProfileService)
        {
            _freelancerProfileService = freelancerProfileService;
        }

        [HttpPost]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FreelancerProfileResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateFreelancerProfile([FromBody] CreateFreelancerProfileDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var profile = await _freelancerProfileService.CreateFreelancerProfileAsync(userId, createDto);
            if (profile == null)
            {
                return Conflict(new { message = "Freelancer profile already exists for this user, or user is not a Freelancer/Both type." });
            }
            return CreatedAtAction(nameof(GetFreelancerProfileById), new { id = profile.Id }, profile);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FreelancerProfileResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFreelancerProfileById(Guid id)
        {
            var profile = await _freelancerProfileService.GetFreelancerProfileByIdAsync(id);
            if (profile == null)
            {
                return NotFound($"Freelancer profile with ID {id} not found.");
            }
            return Ok(profile);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FreelancerProfileResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyFreelancerProfile()
        {
            var userId = GetUserId();
            var profile = await _freelancerProfileService.GetFreelancerProfileByUserIdAsync(userId);
            if (profile == null)
            {
                return NotFound("Your freelancer profile not found. Please create one.");
            }
            return Ok(profile);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FreelancerProfileResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateFreelancerProfile(Guid id, [FromBody] UpdateFreelancerProfileDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var profile = await _freelancerProfileService.GetFreelancerProfileByIdAsync(id);
            if (profile == null)
            {
                return NotFound($"Freelancer profile with ID {id} not found.");
            }

            if (profile.UserId != userId)
            {
                return Forbid("You can only update your own freelancer profile.");
            }

            var updatedProfile = await _freelancerProfileService.UpdateFreelancerProfileAsync(id, updateDto);
            if (updatedProfile == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update freelancer profile.");
            }
            return Ok(updatedProfile);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Freelancer,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFreelancerProfile(Guid id)
        {
            var userId = GetUserId();
            var userType = GetUserType();

            var profile = await _freelancerProfileService.GetFreelancerProfileByIdAsync(id);
            if (profile == null)
            {
                return NotFound($"Freelancer profile with ID {id} not found.");
            }

            if (profile.UserId != userId && userType != UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to delete this freelancer profile.");
            }

            var success = await _freelancerProfileService.DeleteFreelancerProfileAsync(id);
            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete freelancer profile.");
            }
            return NoContent();
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<FreelancerProfileResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllFreelancerProfiles([FromQuery] FreelancerFilter filter, [FromQuery] PaginationParams pagination)
        {
            var pagedResult = await _freelancerProfileService.GetAllFreelancerProfilesAsync(filter, pagination);
            return Ok(pagedResult);
        }


        [HttpPost("{id}/resume")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FreelancerProfileResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadResume(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var userId = GetUserId();
            var profile = await _freelancerProfileService.GetFreelancerProfileByIdAsync(id);
            if (profile == null) return NotFound($"Freelancer profile with ID {id} not found.");
            if (profile.UserId != userId) return Forbid("You can only upload a resume for your own profile.");

            var updatedProfile = await _freelancerProfileService.UploadResumeAsync(id, file);
            if (updatedProfile == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload resume.");
            }
            return Ok(updatedProfile);
        }

        [HttpDelete("{id}/resume")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveResume(Guid id)
        {
            var userId = GetUserId();
            var profile = await _freelancerProfileService.GetFreelancerProfileByIdAsync(id);
            if (profile == null) return NotFound($"Freelancer profile with ID {id} not found.");
            if (profile.UserId != userId) return Forbid("You can only remove a resume from your own profile.");

            var success = await _freelancerProfileService.RemoveResumeAsync(id);
            if (!success)
            {
                return NotFound("Resume not found or failed to remove.");
            }
            return NoContent();
        }

        [HttpPost("{id}/profile-picture")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FreelancerProfileResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadProfilePicture(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var userId = GetUserId();
            var profile = await _freelancerProfileService.GetFreelancerProfileByIdAsync(id);
            if (profile == null) return NotFound($"Freelancer profile with ID {id} not found.");
            if (profile.UserId != userId) return Forbid("You can only upload a profile picture for your own profile.");

            var updatedProfile = await _freelancerProfileService.UploadProfilePictureAsync(id, file);
            if (updatedProfile == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload profile picture.");
            }
            return Ok(updatedProfile);
        }

        [HttpDelete("{id}/profile-picture")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveProfilePicture(Guid id)
        {
            var userId = GetUserId();
            var profile = await _freelancerProfileService.GetFreelancerProfileByIdAsync(id);
            if (profile == null) return NotFound($"Freelancer profile with ID {id} not found.");
            if (profile.UserId != userId) return Forbid("You can only remove a profile picture from your own profile.");

            var success = await _freelancerProfileService.RemoveProfilePictureAsync(id);
            if (!success)
            {
                return NotFound("Profile picture not found or failed to remove.");
            }
            return NoContent();
        }
    }
}
