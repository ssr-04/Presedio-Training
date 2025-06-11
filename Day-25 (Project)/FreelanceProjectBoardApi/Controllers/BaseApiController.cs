using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FreelanceProjectBoardApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [EnableRateLimiting("user-policy")]
    // [Authorize] //Maybe 
    public abstract class BaseApiController : ControllerBase
    {
        // Helper to get the authenticated user's ID from JWT claims
        protected Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("Authenticated User ID is missing or invalid.");
            }
            return userId;
        }

        // Helper to get the authenticated user's Type (Role) from JWT claims
        protected string GetUserType()
        {
            var userTypeClaim = User.FindFirst(ClaimTypes.Role);
            if (userTypeClaim == null)
            {
                throw new UnauthorizedAccessException("Authenticated User Type (Role) is missing.");
            }
            return userTypeClaim.Value;
        }
    }
}
