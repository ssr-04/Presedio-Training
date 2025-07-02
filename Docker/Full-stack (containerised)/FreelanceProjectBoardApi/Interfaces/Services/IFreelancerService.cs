using FreelanceProjectBoardApi.DTOs.FreelancerProfiles;
using FreelanceProjectBoardApi.Helpers;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace FreelanceProjectBoardApi.Services.Interfaces
{
    public interface IFreelancerProfileService
    {
        Task<FreelancerProfileResponseDto?> CreateFreelancerProfileAsync(Guid userId, CreateFreelancerProfileDto createDto);
        Task<FreelancerProfileResponseDto?> GetFreelancerProfileByIdAsync(Guid id);
        Task<FreelancerProfileResponseDto?> GetFreelancerProfileByUserIdAsync(Guid userId);
        Task<FreelancerProfileResponseDto?> UpdateFreelancerProfileAsync(Guid id, UpdateFreelancerProfileDto updateDto);
        Task<bool> DeleteFreelancerProfileAsync(Guid id);
        Task<PageResult<FreelancerProfileResponseDto>> GetAllFreelancerProfilesAsync(FreelancerFilter filter, PaginationParams pagination);
        Task<FreelancerProfileResponseDto?> UploadResumeAsync(Guid freelancerProfileId, IFormFile file);
        Task<FreelancerProfileResponseDto?> UploadProfilePictureAsync(Guid freelancerProfileId, IFormFile file);
        Task<bool> RemoveResumeAsync(Guid freelancerProfileId);
        Task<bool> RemoveProfilePictureAsync(Guid freelancerProfileId);
    }
}
