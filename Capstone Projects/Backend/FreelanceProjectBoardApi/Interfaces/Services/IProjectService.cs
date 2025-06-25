using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.Projects;
using FreelanceProjectBoardApi.Helpers;

namespace FreelanceProjectBoardApi.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponseDto?> CreateProjectAsync(Guid clientId, CreateProjectDto createDto);
        Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id);
        Task<ProjectResponseDto?> UpdateProjectAsync(Guid id, UpdateProjectDto updateDto);
        Task<bool> DeleteProjectAsync(Guid id);
        Task<PageResult<ProjectListDto>> GetAllProjectsAsync(ProjectFilter filter, PaginationParams pagination);
        Task<ProjectResponseDto?> AssignFreelancerToProjectAsync(Guid projectId, Guid freelancerId); // For client to assign
        Task<ProjectResponseDto?> MarkProjectAsCompletedAsync(Guid projectId); // For client to mark as complete
        Task<ProjectResponseDto?> CancelProjectAsync(Guid projectId); // For client to cancel

        Task<FileResponseDto?> UploadProjectAttachmentAsync(Guid projectId, Guid uploaderId, IFormFile file);
        Task<bool> RemoveProjectAttachmentAsync(Guid projectId, Guid attachmentId);
        Task<IEnumerable<FileResponseDto>> GetProjectAttachmentsMetadataAsync(Guid projectId); // Lists all attachments for a project
        Task<FileResponseDto?> GetProjectAttachmentMetadataAsync(Guid attachmentId); // Get single attachment metadata

        Task<IEnumerable<ProjectResponseDto>> GetMyProjectsAsync(Guid userId);

    }
}
