using FreelanceProjectBoardApi.DTOs.Auth;
using FreelanceProjectBoardApi.DTOs.Users;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models; // For UserType
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceProjectBoardApi.Controllers
{
    [Authorize] // All actions in this controller require authentication
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            // Allows user to get their own profile, or Admin to get any profile
            if (currentUserId != id && currentUserType != UserType.Admin.ToString())
            {
                return StatusCode(403, new { message = "You do not have permission to access this user's profile." });
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return Ok(user);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admins can list all users
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<UserResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserFilter filter, [FromQuery] PaginationParams pagination)
        {
            var pagedResult = await _userService.GetAllUsersAsync(filter, pagination);
            return Ok(pagedResult);
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetUserId();
            if (currentUserId != id)
            {
                return StatusCode(403, new { message = "You can only update your own user profile." });
            }

            var updatedUser = await _userService.UpdateUserAsync(id, updateDto);
            if (updatedUser == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return Ok(updatedUser);
        }

        [HttpPut("{id}/change-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetUserId();
            if (currentUserId != id)
            {
                return StatusCode(403, new { message = "You can only change your own password." });
            }

            var success = await _userService.ChangePasswordAsync(id, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!success)
            {
                return BadRequest(new { message = "Failed to change password. Current password might be incorrect or user not found." });
            }
            return NoContent();
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            // Allows user to delete their own account, or Admin to delete any account
            if (currentUserId != id && currentUserType != UserType.Admin.ToString())
            {
                return StatusCode(403, new { message = "You do not have permission to delete this user's account." });
            }

            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return NoContent();
        }
    }
    
}
