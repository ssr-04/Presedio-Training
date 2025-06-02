using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController] // Indicates that the class is an API controller
[Route("api/[controller]")] // Defines the base route, e.g., /api/auth
public class AuthController : ControllerBase // Base class for MVC controllers without view support
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger; // Add logging

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">User registration details.</param>
    /// <returns>A JWT token and user ID on successful registration.</returns>
    [HttpPost("register")] // Route: /api/auth/register
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // For username/email already taken
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        _logger.LogInformation("Received registration request for username: {Username}", request.Username);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid registration request for username: {Username}. Errors: {Errors}",
                request.Username, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState); // Returns validation errors
        }

        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response); // Returns 200 OK with AuthResponseDto
        }
        catch (ApplicationException ex) when (ex.Message.Contains("Username already taken") || ex.Message.Contains("Email already taken"))
        {
            _logger.LogWarning("Registration failed: {ErrorMessage}", ex.Message);
            return Conflict(new { error = ex.Message }); // 409 Conflict
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during user registration for username: {Username}", request.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred during registration." });
        }
    }

    /// <summary>
    /// Logs in an existing user.
    /// </summary>
    /// <param name="request">User login credentials.</param>
    /// <returns>A JWT token and user ID on successful login.</returns>
    [HttpPost("login")] // Route: /api/auth/login
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // For invalid credentials
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("Received login request for username: {Username}", request.Username);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid login request for username: {Username}. Errors: {Errors}",
                request.Username, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                _logger.LogWarning("Login failed for username: {Username}. Invalid credentials.", request.Username);
                return Unauthorized(new { error = "Invalid username or password." }); // 401 Unauthorized
            }
            return Ok(response); // Returns 200 OK with AuthResponseDto
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during user login for username: {Username}", request.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred during login." });
        }
    }

    /// <summary>
    /// Logs out the current user (client-side token invalidation).
    /// </summary>
    /// <returns>No content on success.</returns>
    [HttpPost("logout")] // Route: /api/auth/logout
    [Authorize] // Requires a valid JWT token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        // For stateless JWTs, logout is primarily a client-side operation
        // (deleting the token from storage).
        // Server-side, we might just log the event. If refresh tokens were used,
        // this is where they would be revoked.
        _logger.LogInformation("User {UserId} logged out (client-side).", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        return NoContent(); // Returns 204 No Content
    }
}