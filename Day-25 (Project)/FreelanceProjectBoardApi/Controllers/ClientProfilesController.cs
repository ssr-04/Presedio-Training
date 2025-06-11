using FreelanceProjectBoardApi.DTOs.ClientProfiles;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models; 
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceProjectBoardApi.Controllers
{
    [Authorize]
    public class ClientProfilesController : BaseApiController
    {
        private readonly IClientProfileService _clientProfileService;

        public ClientProfilesController(IClientProfileService clientProfileService)
        {
            _clientProfileService = clientProfileService;
        }

        [HttpPost]
        [Authorize(Roles = "Client")] //Only a user who signed up as client
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ClientProfileResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateClientProfile([FromBody] CreateClientProfileDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var profile = await _clientProfileService.CreateClientProfileAsync(userId, createDto);
            if (profile == null)
            {
                return Conflict(new { message = "Client profile already exists for this user, or user is not a Client type." });
            }
            return CreatedAtAction(nameof(GetClientProfileById), new { id = profile.Id }, profile);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClientProfileById(Guid id)
        {
            var profile = await _clientProfileService.GetClientProfileByIdAsync(id);
            if (profile == null)
            {
                return NotFound($"Client profile with ID {id} not found.");
            }
            return Ok(profile);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyClientProfile()
        {
            var userId = GetUserId();
            var profile = await _clientProfileService.GetClientProfileByUserIdAsync(userId);
            if (profile == null)
            {
                return NotFound("Your client profile not found. Please create one.");
            }
            return Ok(profile);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateClientProfile(Guid id, [FromBody] UpdateClientProfileDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var profile = await _clientProfileService.GetClientProfileByIdAsync(id);
            if (profile == null)
            {
                return NotFound($"Client profile with ID {id} not found.");
            }

            // Ensures the profile belongs to the authenticated user
            if (profile.UserId != userId)
            {
                return StatusCode(403, new { message = "You can only update your own client profile." });
            }

            var updatedProfile = await _clientProfileService.UpdateClientProfileAsync(id, updateDto);
            if (updatedProfile == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update client profile.");
            }
            return Ok(updatedProfile);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Client,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteClientProfile(Guid id)
        {
            var userId = GetUserId();
            var userType = GetUserType();

            var profile = await _clientProfileService.GetClientProfileByIdAsync(id);
            if (profile == null)
            {
                return NotFound($"Client profile with ID {id} not found.");
            }

            // Allow owner or Admin to delete
            if (profile.UserId != userId && userType != UserType.Admin.ToString())
            {
                return StatusCode(403, new { message = "You do not have permission to delete this client profile." });
            }

            var success = await _clientProfileService.DeleteClientProfileAsync(id);
            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete client profile.");
            }
            return NoContent();
        }


        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admins can list all client profiles
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<ClientProfileResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllClientProfiles([FromQuery] ClientFilter filter, [FromQuery] PaginationParams pagination)
        {
            var pagedResult = await _clientProfileService.GetAllClientProfilesAsync(filter, pagination);
            return Ok(pagedResult);
        }
    }
}
