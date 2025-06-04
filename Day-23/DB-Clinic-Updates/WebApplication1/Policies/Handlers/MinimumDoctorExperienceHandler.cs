using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

public class MinimumDoctorExperienceHandler : AuthorizationHandler<MinimumDoctorExperienceRequirement>
{
    private readonly IDoctorService _doctorService;
    private readonly IUserRepository _userRepository;

    public MinimumDoctorExperienceHandler(IDoctorService doctorService, IUserRepository userRepository)
    {
        _doctorService = doctorService;
        _userRepository = userRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumDoctorExperienceRequirement requirement)
    {
        // Admin are always allowed
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        // Only doctors can satisfy this requirement
        if (!context.User.IsInRole("Doctor"))
        {
            return;
        }

        var UserIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier); //email
        if (UserIdClaim == null)
        {
            return;
        }

        var user = await _userRepository.GetUserByUsernameAsync(UserIdClaim.Value);
        if (user == null)
        {
            return; //User not found in DB
        }

        //Getting the user ID
        var doctorId = user.DoctorId;
        if (!doctorId.HasValue)
        {
            return;
        }
        var doctor = await _doctorService.GetDoctorByIdAsync(doctorId.Value);

        if (doctor != null && doctor.YearsOfExperience >= requirement.MinimumYearsExperience)
        {
            context.Succeed(requirement); // Doctor meets the experience requirement
            return; 
        }

        return;
    }
}