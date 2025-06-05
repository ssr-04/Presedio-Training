using FileHandlingApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles; // For GetMimeType

namespace FileHandlingApi.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadsFolderPath;
        private readonly string _metadataFilePath;
        private readonly IWebHostEnvironment _environment;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public FileService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _environment = environment;
            
            _uploadsFolderPath = Path.Combine(_environment.ContentRootPath, configuration["FileStorage:UploadsFolderPath"] ?? throw new ArgumentNullException("FileStorage:UploadsFolderPath configuration value is missing"));
            _metadataFilePath = Path.Combine(_environment.ContentRootPath, configuration["FileStorage:MetadataFilePath"] ?? throw new ArgumentNullException("FileStorage:MetadataFilePath configuration value is missing"));

            // Ensuring the uploads directory exists
            if (!Directory.Exists(_uploadsFolderPath))
            {
                Directory.CreateDirectory(_uploadsFolderPath);
            }

            _contentTypeProvider = new FileExtensionContentTypeProvider();
        }

        public async Task<List<FileMetadata>> UploadFilesAsync(IEnumerable<IFormFile> files)
        {
            var uploadedMetadata = new List<FileMetadata>();
            var allMetadata = await GetAllFileMetadataInternalAsync(); // Load existing metadata

            foreach (var file in files)
            {
                if (file.Length == 0) continue; // Skiping empty files

                var id = Guid.NewGuid().ToString(); // Generating GUID for unique stored filename
                var originalFileName = Path.GetFileName(file.FileName); // Getting original name
                var fileExtension = Path.GetExtension(originalFileName);
                var storedFileName = $"{id}{fileExtension}"; // Full stored name with extension

                var filePath = Path.Combine(_uploadsFolderPath, storedFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Get MIME type for the stored file
                string contentType = "application/octet-stream"; // Default
                if (_contentTypeProvider.TryGetContentType(storedFileName, out var detectedContentType))
                {
                    contentType = detectedContentType;
                }
                 else if (!string.IsNullOrEmpty(file.ContentType)) // Fallback to client provided content type
                 {
                    contentType = file.ContentType;
                 }


                var metadata = new FileMetadata
                {
                    Id = id,
                    OriginalFileName = originalFileName,
                    ContentType = contentType,
                    FileSize = file.Length,
                    UploadedDate = DateTime.UtcNow
                };

                uploadedMetadata.Add(metadata);
                allMetadata.Add(metadata); // Add to the full list
            }

            await SaveAllFileMetadataAsync(allMetadata); // Save updated metadata
            return uploadedMetadata;
        }

        public async Task<(byte[] fileContent, string contentType, string originalFileName)?> DownloadFileAsync(string id)
        {
            var allMetadata = await GetAllFileMetadataInternalAsync();
            var metadata = allMetadata.FirstOrDefault(m => m.Id == id);

            if (metadata == null)
            {
                return null; // Metadata not found
            }

            var fileExtension = Path.GetExtension(metadata.OriginalFileName);
            var storedFileName = $"{id}{fileExtension}";
            var filePath = Path.Combine(_uploadsFolderPath, storedFileName);

            if (!File.Exists(filePath))
            {
                return null; // File not found on disk, even if metadata exists
            }

            var fileContent = await File.ReadAllBytesAsync(filePath);
            return (fileContent, metadata.ContentType, metadata.OriginalFileName);
        }

        public async Task<List<FileMetadata>> GetAllFileMetadataAsync()
        {
            return await GetAllFileMetadataInternalAsync();
        }

        // Private method to handle loading metadata from files
        private async Task<List<FileMetadata>> GetAllFileMetadataInternalAsync()
        {
            if (!File.Exists(_metadataFilePath))
            {
                return new List<FileMetadata>(); // empty list if file doesn't exist yet
            }

            try
            {
                var json = await File.ReadAllTextAsync(_metadataFilePath);
                return JsonSerializer.Deserialize<List<FileMetadata>>(json) ?? new List<FileMetadata>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading metadata file: {ex.Message}");
                return new List<FileMetadata>(); // Return empty list on error
            }
        }

        // Private method to handle saving metadata
        private async Task SaveAllFileMetadataAsync(List<FileMetadata> metadata)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true }; // Making JSON writeable
                var json = JsonSerializer.Serialize(metadata, options);
                await File.WriteAllTextAsync(_metadataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing metadata file: {ex.Message}");
            }
        }
    }
}