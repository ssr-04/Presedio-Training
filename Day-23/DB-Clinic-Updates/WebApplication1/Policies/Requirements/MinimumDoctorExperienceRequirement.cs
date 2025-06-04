using Microsoft.AspNetCore.Authorization;

public class MinimumDoctorExperienceRequirement : IAuthorizationRequirement
{
    public float MinimumYearsExperience { get; }

    public MinimumDoctorExperienceRequirement(float minimumYearsExperience)
    {
        MinimumYearsExperience = minimumYearsExperience;
    }
}