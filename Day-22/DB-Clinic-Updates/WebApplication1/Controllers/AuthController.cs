

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))] // Returns the token string
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Unauthorized for invalid credentials
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Bad request for invalid input
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var token = await _authService.LoginAsync(request.Username, request.Password);

        if (token == null)
        {
            // user not found, inactive, or invalid password
            return Unauthorized("Invalid credentials or inactive account.");
        }

        return Ok(token); // Return the JWT token
    }
}