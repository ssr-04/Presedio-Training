using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.Proposals;
using FreelanceProjectBoardApi.Models; 
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceProjectBoardApi.Controllers
{
    [Authorize]
    public class ProposalsController : BaseApiController
    {
        private readonly IProposalService _proposalService;
        private readonly IProjectService _projectService; // To check project details for authorization
        private readonly IFileService _fileService;

        public ProposalsController(IProposalService proposalService, IProjectService projectService, IFileService fileService)
        {
            _proposalService = proposalService;
            _projectService = projectService;
            _fileService = fileService;
        }


        [HttpPost]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProposalResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProposal([FromBody] CreateProposalDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var freelancerId = GetUserId();
            var proposal = await _proposalService.CreateProposalAsync(freelancerId, createDto);
            if (proposal == null)
            {
                return Conflict(new { message = "Failed to create proposal. Project not found, project not open for proposals, or proposal already exists from this freelancer." });
            }
            return CreatedAtAction(nameof(GetProposalById), new { id = proposal.Id }, proposal);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProposalResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProposalById(Guid id)
        {
            var proposal = await _proposalService.GetProposalByIdAsync(id);
            if (proposal == null)
            {
                return NotFound($"Proposal with ID {id} not found.");
            }

            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            // Checking authorization: owner of proposal, client of project, or admin
            if (proposal.FreelancerId != currentUserId &&
                proposal.Project?.ClientId != currentUserId && // client ID through nested project DTO
                currentUserType != UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to view this proposal.");
            }

            return Ok(proposal);
        }


        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProposalResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProposalStatus(Guid id, [FromBody] UpdateProposalStatusDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            var proposal = await _proposalService.GetProposalByIdAsync(id);
            if (proposal == null)
            {
                return NotFound($"Proposal with ID {id} not found.");
            }

            // Authorization and logic for status transitions
            bool isClient = currentUserType == UserType.Client.ToString();
            bool isFreelancer = currentUserType == UserType.Freelancer.ToString();
            bool isAdmin = currentUserType == UserType.Admin.ToString();

            if (isClient && proposal.Project?.ClientId == currentUserId)
            {
                if (updateDto.NewStatus == "Accepted" || updateDto.NewStatus == "Rejected")
                {
                    // Client can accept or reject
                }
                else
                {
                    return Forbid("Clients can only accept or reject proposals.");
                }
            }
            else if (isFreelancer && proposal.FreelancerId == currentUserId)
            {
                if (updateDto.NewStatus == "Withdrawn")
                {
                    // Freelancer can withdraw
                }
                else
                {
                    return Forbid("Freelancers can only withdraw their own proposals.");
                }
            }
            else if (!isAdmin)
            {
                return Forbid("You do not have permission to change the status of this proposal.");
            }

            var updatedProposal = await _proposalService.UpdateProposalStatusAsync(id, updateDto);
            if (updatedProposal == null)
            {
                return BadRequest(new { message = "Failed to update proposal status. Invalid transition or status already finalized." });
            }
            return Ok(updatedProposal);
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> DeleteProposal(Guid id)
        {
            var proposal = await _proposalService.GetProposalByIdAsync(id);
            if (proposal == null)
            {
                return NotFound($"Proposal with ID {id} not found.");
            }

            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            // Only the freelancer can delete their own PENDING proposal
            // Admin can also delete any proposal
            if (proposal.FreelancerId != currentUserId && currentUserType != UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to delete this proposal.");
            }

            if (proposal.Status != "Pending" && currentUserType != UserType.Admin.ToString())
            {
                return BadRequest(new { message = "Only pending proposals can be deleted by freelancers." });
            }

            var success = await _proposalService.DeleteProposalAsync(id);
            if (!success)
            {
                return BadRequest(new { message = "Failed to delete proposal. Proposal might not be pending or already finalized." });
            }
            return NoContent();
        }


        [HttpGet("for-project/{projectId}")]
        [Authorize(Roles = "Client,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProposalResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProposalsForProject(Guid projectId)
        {
            var project = await _projectService.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            // Client who owns project or Admin
            if (project.ClientId != currentUserId && currentUserType != UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to view proposals for this project.");
            }

            var proposals = await _proposalService.GetProposalsForProjectAsync(projectId);
            return Ok(proposals);
        }


        [HttpGet("by-freelancer/{freelancerId}")]
        [Authorize(Roles = "Freelancer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProposalResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetProposalsByFreelancer(Guid freelancerId)
        {
            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            // Freelancer themselves or Admin
            if (freelancerId != currentUserId && currentUserType != UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to view proposals made by this freelancer.");
            }

            var proposals = await _proposalService.GetProposalsByFreelancerAsync(freelancerId);
            return Ok(proposals);
        }

        [HttpPost("{proposalId}/attachments")]
        [Authorize(Roles = "Freelancer")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FileResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadProposalAttachment(Guid proposalId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided." });
            }

            var currentUserId = GetUserId();
            var proposal = await _proposalService.GetProposalByIdAsync(proposalId);

            if (proposal == null)
            {
                return NotFound($"Proposal with ID {proposalId} not found.");
            }
            // Only the freelancer who owns the proposal can upload attachments
            if (proposal.FreelancerId != currentUserId)
            {
                return Forbid("You do not have permission to upload attachments to this proposal.");
            }

            // Ensure the proposal is in a status where attachments can be added ( Pending)
            if (proposal.Status != "Pending")
            {
                return BadRequest(new { message = "Attachments can only be uploaded to proposals with 'Pending' status." });
            }

            try
            {
                var uploadedFile = await _proposalService.UploadProposalAttachmentAsync(proposalId, currentUserId, file);
                if (uploadedFile == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload file.");
                }
                // Uses GetProposalAttachmentMetadata to return a link to the metadata endpoint
                return CreatedAtAction(nameof(GetProposalAttachmentMetadata), new { proposalId = proposalId, attachmentId = uploadedFile.Id }, uploadedFile);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during file upload.");
            }
        }

        [HttpGet("{proposalId}/attachments")]
        [Authorize(Roles = "Client,Freelancer,Admin")] // Client, Proposal Owner, or Admin can list
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FileResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProposalAttachmentsMetadata(Guid proposalId)
        {
            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            var proposal = await _proposalService.GetProposalByIdAsync(proposalId);
            if (proposal == null)
            {
                return NotFound($"Proposal with ID {proposalId} not found.");
            }

            // Authorization check
            if (proposal.FreelancerId != currentUserId &&
                proposal.Project?.ClientId != currentUserId &&
                currentUserType != UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to view attachments for this proposal.");
            }

            var attachments = await _proposalService.GetProposalAttachmentsMetadataAsync(proposalId);
            return Ok(attachments);
        }


        [HttpGet("{proposalId}/attachments/{attachmentId}")]
        [Authorize(Roles = "Client,Freelancer,Admin")] // Proposal Owner, Project Client, or Admin can download
        [ProducesResponseType(StatusCodes.Status200OK)] // Returns a file stream
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadProposalAttachment(Guid proposalId, Guid attachmentId)
        {
            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            var proposal = await _proposalService.GetProposalByIdAsync(proposalId);
            if (proposal == null)
            {
                return NotFound($"Proposal with ID {proposalId} not found.");
            }

            var attachmentMetadata = await _proposalService.GetProposalAttachmentMetadataAsync(attachmentId);
            if (attachmentMetadata == null)
            {
                return NotFound($"Attachment with ID {attachmentId} not found or not associated with proposal {proposalId}.");
            }

            if (proposal.FreelancerId != currentUserId &&
                proposal.Project?.ClientId != currentUserId &&
                currentUserType != UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to download this attachment.");
            }

            try
            {
                var fileContent = await _fileService.DownloadFileAsync(attachmentId);

                if (fileContent == null)
                {
                    return NotFound("File content not found.");
                }

                // Return sthe file for download
                return File(fileContent.Value.Stream, fileContent.Value.ContentType, fileContent.Value.FileName);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while downloading the file.");
            }
        }

        // Added for CreatedAtAction and direct metadata retrieval
        [HttpGet("attachments/{attachmentId}")]
        [ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger as it's primarily for internal use
        public async Task<IActionResult> GetProposalAttachmentMetadata(Guid attachmentId)
        {
            var fileMetadata = await _proposalService.GetProposalAttachmentMetadataAsync(attachmentId);
            if (fileMetadata == null)
            {
                return NotFound($"Attachment metadata with ID {attachmentId} not found.");
            }
            return Ok(fileMetadata);
        }


        [HttpDelete("{proposalId}/attachments/{attachmentId}")]
        [Authorize(Roles = "Freelancer")] // Only the freelancer who owns the proposal can remove attachments
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveProposalAttachment(Guid proposalId, Guid attachmentId)
        {
            var currentUserId = GetUserId();
            var proposal = await _proposalService.GetProposalByIdAsync(proposalId);

            if (proposal == null)
            {
                return NotFound($"Proposal with ID {proposalId} not found.");
            }
            if (proposal.FreelancerId != currentUserId)
            {
                return Forbid("You do not have permission to remove attachments from this proposal.");
            }

            var success = await _proposalService.RemoveProposalAttachmentAsync(proposalId, attachmentId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to remove attachment. It might not exist or belong to this proposal." });
            }
            return NoContent(); // 204 No Content for successful deletion
        }
    }
}
