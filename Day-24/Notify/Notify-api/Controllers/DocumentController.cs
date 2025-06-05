using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyAPI.DTOs;

namespace NotifyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    [Authorize] 
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }


        [HttpPost("upload")]
        [Authorize(Policy = "RequireHRAdminRole")]
        public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromForm] DocumentUploadRequestDto metadata)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            if (string.IsNullOrEmpty(metadata.Title))
            {
                return BadRequest("Document title is required.");
            }

            // Get the Auth0UserId of the uploader from claims
            var uploaderAuth0UserId = User.FindFirst("Auth0UserId")?.Value;
            if (string.IsNullOrEmpty(uploaderAuth0UserId))
            {
                return Unauthorized("Could not identify uploader's Auth0 ID from token.");
            }

            try
            {
                var uploadedDocument = await _documentService.UploadDocumentAsync(file, metadata, uploaderAuth0UserId);
                return CreatedAtAction(nameof(GetDocumentMetadata), new { id = uploadedDocument.Id }, uploadedDocument);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading document: {ex.Message}");
                return StatusCode(500, "An error occurred during file upload.");
            }
        }

        [HttpGet] 
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var documents = await _documentService.GetAllDocumentMetadataAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all documents: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving documents.");
            }
        }

        [HttpGet("{id}")] 
        public async Task<IActionResult> GetDocumentMetadata(string id)
        {
            try
            {
                var documentDto = await _documentService.GetDocumentMetadataByIdAsync(id);

                if (documentDto == null)
                {
                    return NotFound($"Document with ID '{id}' not found.");
                }

                return Ok(documentDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting document metadata for ID '{id}': {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving document metadata.");
            }
        }


        [HttpGet("{id}/download")] 
        public async Task<IActionResult> DownloadDocument(string id)
        {
            try
            {
                var result = await _documentService.DownloadDocumentAsync(id);

                if (result == null || result.Value.content == null || result.Value.content.Length == 0)
                {
                    return NotFound($"Document with ID '{id}' not found or has no content.");
                }

                var (content, contentType, originalFileName) = result.Value;
                return File(content, contentType, originalFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading document with ID '{id}': {ex.Message}");
                return StatusCode(500, "An error occurred during document download.");
            }
        }

        [HttpDelete("{id}")] 
        [Authorize(Policy = "RequireHRAdminRole")]
        public async Task<IActionResult> DeleteDocument(string id)
        {
            try
            {
                var deleted = await _documentService.DeleteDocumentAsync(id);
                if (!deleted)
                {
                    return NotFound($"Document with ID '{id}' not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting document with ID '{id}': {ex.Message}");
                return StatusCode(500, "An error occurred during document deletion.");
            }
        }
    }
}