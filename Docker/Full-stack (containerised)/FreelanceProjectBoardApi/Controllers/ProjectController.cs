using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.Projects;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models; 
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceProjectBoardApi.Controllers
{
    [Authorize]
    public class ProjectsController : BaseApiController
    {
        private readonly IProjectService _projectService;
        private readonly IFileService _fileService;

        public ProjectsController(IProjectService projectService, IFileService fileService)
        {
            _projectService = projectService;
            _fileService = fileService;
        }


        [HttpPost]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProjectResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clientId = GetUserId();
            var project = await _projectService.CreateProjectAsync(clientId, createDto);
            if (project == null)
            {
                return BadRequest(new { message = "Failed to create project. Ensure client profile exists." });
            }
            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }
            return Ok(project);
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<ProjectListDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProjects([FromQuery] ProjectFilter filter, [FromQuery] PaginationParams pagination)
        {
            var pagedResult = await _projectService.GetAllProjectsAsync(filter, pagination);
            return Ok(pagedResult);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetUserId();
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }
            if (project.ClientId != currentUserId)
            {
                return StatusCode(403, new { message = "You do not have permission to update this project." });
            }
            if (project.Status != "Open")
            {
                return BadRequest(new { message = "Only projects with 'Open' status can be updated." });
            }

            var updatedProject = await _projectService.UpdateProjectAsync(id, updateDto);
            if (updatedProject == null)
            {
                return BadRequest("Failed to update project. Check if project is open or inputs are valid.");
            }
            return Ok(updatedProject);
        }

        [HttpPut("{id}/assign-freelancer/{freelancerId}")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignFreelancer(Guid id, Guid freelancerId)
        {
            var currentUserId = GetUserId();
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }
            if (project.ClientId != currentUserId)
            {
                return StatusCode(403, new { message = "You do not have permission to assign a freelancer to this project." });
            }
            if (project.Status != "Open")
            {
                return BadRequest(new { message = "Only projects with 'Open' status can be assigned a freelancer." });
            }

            var updatedProject = await _projectService.AssignFreelancerToProjectAsync(id, freelancerId);
            if (updatedProject == null)
            {
                return BadRequest(new { message = "Failed to assign freelancer. Freelancer might not exist or project status is invalid." });
            }
            return Ok(updatedProject);
        }

        [HttpPut("{id}/mark-completed")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkProjectCompleted(Guid id)
        {
            var currentUserId = GetUserId();
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }
            if (project.ClientId != currentUserId)
            {
                return StatusCode(403, new { message = "You do not have permission to mark this project as completed." });
            }
            if (project.Status != "Assigned")
            {
                return BadRequest(new { message = "Only projects with 'Assigned' status can be marked as completed." });
            }

            var updatedProject = await _projectService.MarkProjectAsCompletedAsync(id);
            if (updatedProject == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to mark project as completed.");
            }
            return Ok(updatedProject);
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelProject(Guid id)
        {
            var currentUserId = GetUserId();
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }
            if (project.ClientId != currentUserId)
            {
                return StatusCode(403, new { message = "You do not have permission to cancel this project." });
            }
            if (project.Status != "Open" && project.Status != "Assigned")
            {
                return BadRequest(new { message = "Only projects with 'Open' or 'Assigned' status can be cancelled." });
            }

            var updatedProject = await _projectService.CancelProjectAsync(id);
            if (updatedProject == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to cancel project.");
            }
            return Ok(updatedProject);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Client,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            if (project.ClientId != currentUserId && currentUserType != UserType.Admin.ToString())
            {
                return StatusCode(403, new { message = "You do not have permission to delete this project." });
            }

            var success = await _projectService.DeleteProjectAsync(id);
            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete project.");
            }
            return NoContent();
        }

        [HttpPost("{projectId}/attachments")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FileResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadProjectAttachment(Guid projectId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided." });
            }

            var currentUserId = GetUserId();
            var project = await _projectService.GetProjectByIdAsync(projectId);

            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }
            // Only the project owner can upload attachments
            if (project.ClientId != currentUserId)
            {
                return StatusCode(403, new { message = "You do not have permission to upload attachments to this project." });
            }

            // Ensures the project is in a status where attachments can be added)
            if (project.Status != "Open" && project.Status != "Assigned" && project.Status != "InProgress")
            {
                return BadRequest(new { message = "Attachments can only be uploaded to projects with 'Open', 'Assigned', or 'In Progress' status." });
            }

            try
            {
                var uploadedFile = await _projectService.UploadProjectAttachmentAsync(projectId, currentUserId, file);
                if (uploadedFile == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload file.");
                }

                // Uses GetProjectAttachmentMetadata to return a link to the metadata endpoint
                return CreatedAtAction(nameof(GetProjectAttachmentMetadata), new { projectId = projectId, attachmentId = uploadedFile.Id }, uploadedFile);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during file upload.");
            }
        }

        [HttpGet("{projectId}/attachments")]
        [Authorize(Roles = "Client,Freelancer,Admin")] // Client, Assigned Freelancer, or Admin can list
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FileResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProjectAttachmentsMetadata(Guid projectId)
        {
            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            var project = await _projectService.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            // // Authorization check: Project client, assigned freelancer, or admin can list attachments
            // if (project.ClientId != currentUserId &&
            //     project.AssignedFreelancerId != currentUserId &&
            //     currentUserType != UserType.Admin.ToString())
            // {
            //     return StatusCode(403, new { message = "You do not have permission to view attachments for this project." });
            // }

            var attachments = await _projectService.GetProjectAttachmentsMetadataAsync(projectId);
            return Ok(attachments);
        }


        [HttpGet("{projectId}/attachments/{attachmentId}")]
        [Authorize(Roles = "Client,Freelancer,Admin")] // Client, Assigned Freelancer, or Admin can download
        [ProducesResponseType(StatusCodes.Status200OK)] // Returns a file stream
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadProjectAttachment(Guid projectId, Guid attachmentId)
        {
            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            var project = await _projectService.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            // Get attachment metadata to verify it belongs to the project and get file details
            var attachmentMetadata = await _projectService.GetProjectAttachmentMetadataAsync(attachmentId);
            if (attachmentMetadata == null)
            {
                return NotFound($"Attachment with ID {attachmentId} not found.");
            }

            // Authorization check: Project client, assigned freelancer, or admin can download
            if (project.ClientId != currentUserId &&
                project.AssignedFreelancerId != currentUserId &&
                currentUserType != UserType.Admin.ToString())
            {
                return StatusCode(403, new { message = "You do not have permission to download this attachment." });
            }

            try
            {
                var fileContent = await _fileService.DownloadFileAsync(attachmentId);

                if (fileContent == null)
                {
                    return NotFound("File content not found.");
                }

                // Return the file for download
                return File(fileContent.Value.Stream, fileContent.Value.ContentType, fileContent.Value.FileName);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while downloading the file.");
            }
        }

        // Added for CreatedAtAction and direct metadata retrieval
        [HttpGet("attachments/{attachmentId}")]
        [ApiExplorerSettings(IgnoreApi = true)] // Hides from Swagger as it's primarily for internal use
        public async Task<IActionResult> GetProjectAttachmentMetadata(Guid attachmentId)
        {
            var fileMetadata = await _projectService.GetProjectAttachmentMetadataAsync(attachmentId);
            if (fileMetadata == null)
            {
                return NotFound($"Attachment metadata with ID {attachmentId} not found.");
            }

            return Ok(fileMetadata);
        }


        [HttpDelete("{projectId}/attachments/{attachmentId}")]
        [Authorize(Roles = "Client")] // Only the client who owns the project can remove attachments
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveProjectAttachment(Guid projectId, Guid attachmentId)
        {
            var currentUserId = GetUserId();
            var project = await _projectService.GetProjectByIdAsync(projectId);

            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }
            // Only the project owner can remove attachments
            if (project.ClientId != currentUserId)
            {
                return StatusCode(403, new { message = "You do not have permission to remove attachments from this project." });
            }

            var success = await _projectService.RemoveProjectAttachmentAsync(projectId, attachmentId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to remove attachment. It might not exist or belong to this project." });
            }
            return NoContent(); // 204 No Content for successful deletion
        }

        [HttpGet("MyProjects")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProjectResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyProjects()
        {
            
            var currentUserId = GetUserId();
            var projects = await _projectService.GetMyProjectsAsync(currentUserId);
            
            return Ok(projects);
        }
    }
}
