using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.Models; // For FileCategory
using Microsoft.AspNetCore.Http; // For IFormFile

namespace FreelanceProjectBoardApi.Services.Interfaces
{
    public interface IFileService
    {
        Task<FileResponseDto?> UploadFileAsync(IFormFile file, Guid uploaderId, FileCategory category, Guid? associatedEntityId = null);
        Task<(Stream Stream, string ContentType, string FileName)?> DownloadFileAsync(Guid fileId);
        Task<bool> DeleteFileAsync(Guid fileId);
        Task<FileResponseDto?> GetFileMetadataAsync(Guid fileId);
    }
}
