using FreelanceProjectBoardApi.DTOs.ClientProfiles;
using FreelanceProjectBoardApi.DTOs.FreelancerProfiles;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.DTOs.Users
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ClientProfileResponseDto? ClientProfile { get; set; }
        public FreelancerProfileResponseDto? FreelancerProfile { get; set; }

    }
}