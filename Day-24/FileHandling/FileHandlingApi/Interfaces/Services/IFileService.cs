using FileHandlingApi.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileHandlingApi.Services
{
    public interface IFileService
    {
        Task<List<FileMetadata>> UploadFilesAsync(IEnumerable<IFormFile> files);
        Task<(byte[] fileContent, string contentType, string originalFileName)?> DownloadFileAsync(string id);
        Task<List<FileMetadata>> GetAllFileMetadataAsync();
    }
}