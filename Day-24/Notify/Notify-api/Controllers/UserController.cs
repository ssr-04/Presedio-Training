using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyService.Services;

[ApiController]
[Route("api/[controller]")] 
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var auth0UserId = User.FindFirst("Auth0UserId")?.Value;
        if (string.IsNullOrEmpty(auth0UserId))
        {
            return Unauthorized("Auth0 User ID claim not found in token.");
        }

        var userDto = await _userService.GetUserByAuth0UserIdAsync(auth0UserId);

        if (userDto == null)
        {
            Console.WriteLine($"Error: Authenticated user with Auth0Id '{auth0UserId}' not found in local database (despite provisioning).");
            return NotFound("User profile not found.");
        }

        return Ok(userDto);
    }
}