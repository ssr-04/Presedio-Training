using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceProjectBoardApi.Controllers
{
    [Authorize] // All file operations require authentication
    public class FilesController : BaseApiController
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Returns a file stream
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            // TODO: Implement more granular authorization here based on file category and associated entity.
            // For example:
            // - Resume/Profile Picture: Publicly viewable or only to selected clients/freelancers.
            // - Project Attachment: Only accessible by project client/assigned freelancer.
            // - Proposal Attachment: Only accessible by proposing freelancer, project client, or assigned freelancer.

            var fileResult = await _fileService.DownloadFileAsync(id);
            if (fileResult == null)
            {
                return NotFound($"File with ID {id} not found or inaccessible.");
            }

            // Returns file stream with correct content type and filename
            return File(fileResult.Value.Stream, fileResult.Value.ContentType, fileResult.Value.FileName);
        }


        [HttpGet("{id}/metadata")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFileMetadata(Guid id)
        {
            var metadata = await _fileService.GetFileMetadataAsync(id);
            if (metadata == null)
            {
                return NotFound($"File metadata for ID {id} not found.");
            }
            return Ok(metadata);
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFile(Guid id)
        {
            var fileMetadata = await _fileService.GetFileMetadataAsync(id);
            if (fileMetadata == null)
            {
                return NotFound($"File with ID {id} not found.");
            }

            var currentUserId = GetUserId();
            var currentUserType = GetUserType();

            // Only uploader or Admin can delete
            if (fileMetadata.UploaderId != currentUserId && currentUserType != Models.UserType.Admin.ToString())
            {
                return Forbid("You do not have permission to delete this file.");
            }

            var success = await _fileService.DeleteFileAsync(id);
            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete file.");
            }
            return NoContent();
        }
    }
}
