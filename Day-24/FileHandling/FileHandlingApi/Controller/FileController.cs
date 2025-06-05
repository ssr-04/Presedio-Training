using FileHandlingApi.Models;
using FileHandlingApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileHandlingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FileMetadata>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequestSizeLimit(20 * 1024 * 1024)] // Limiting to 20 MB per request
        public async Task<IActionResult> UploadFiles([FromForm] IEnumerable<IFormFile> files)
        {
            if (files == null || !files.Any())
            {
                return BadRequest("No files selected for upload.");
            }

            var uploadedMetadata = await _fileService.UploadFilesAsync(files);

            if (!uploadedMetadata.Any())
            {
                return BadRequest("File upload failed or no files were processed.");
            }

            return Ok(uploadedMetadata);
        }

        
        [HttpGet("download/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadFile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("File ID cannot be empty.");
            }

            var fileResult = await _fileService.DownloadFileAsync(id);

            if (fileResult == null)
            {
                return NotFound($"File with ID '{id}' not found.");
            }

            var (fileContent, contentType, originalFileName) = fileResult.Value;

            // Using the original file name for browser download
            return File(fileContent, contentType, originalFileName);
        }

    
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FileMetadata>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllFiles()
        {
            var allMetadata = await _fileService.GetAllFileMetadataAsync();
            return Ok(allMetadata);
        }
    }
}