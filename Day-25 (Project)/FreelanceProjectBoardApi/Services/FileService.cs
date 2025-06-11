using AutoMapper;
using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Interfaces;

namespace FreelanceProjectBoardApi.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IMapper _mapper;
        private readonly string _uploadFolderPath;

        public FileService(IFileRepository fileRepository, IMapper mapper, IConfiguration configuration)
        {
            _fileRepository = fileRepository;
            _mapper = mapper;
            _uploadFolderPath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            // Ensure the upload folder exists
            if (!Directory.Exists(_uploadFolderPath))
            {
                Directory.CreateDirectory(_uploadFolderPath);
            }
        }

        public async Task<FileResponseDto?> UploadFileAsync(IFormFile file, Guid uploaderId, FileCategory category, Guid? associatedEntityId = null)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // Generate a unique file name to avoid collisions
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_uploadFolderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileMetadata = new Models.File
            {
                FileName = file.FileName, // Original file name
                StoredFileName = uniqueFileName, // Unique name on disk
                MimeType = file.ContentType,
                FileSize = file.Length,
                UploaderId = uploaderId,
                Category = category,
                ProjectId = category == FileCategory.ProjectAttachment && associatedEntityId.HasValue ? associatedEntityId : null,
                ProposalId = category == FileCategory.ProposalAttachment && associatedEntityId.HasValue ? associatedEntityId : null
                // Add logic for ResumeFileId, ProfilePictureFileId if File entity has direct FKs for them,
                // otherwise, these are handled in Profile services linking to File.Id.
            };

            await _fileRepository.AddAsync(fileMetadata);
            await _fileRepository.SaveChangesAsync();

            return _mapper.Map<FileResponseDto>(fileMetadata);
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> DownloadFileAsync(Guid fileId)
        {
            var fileMetadata = await _fileRepository.GetByIdAsync(fileId);
            if (fileMetadata == null)
            {
                return null;
            }

            var filePath = Path.Combine(_uploadFolderPath, fileMetadata.StoredFileName);
            if (!System.IO.File.Exists(filePath))
            {
                return null; // File not found on disk
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return (stream, fileMetadata.MimeType, fileMetadata.FileName);
        }

        public async Task<bool> DeleteFileAsync(Guid fileId)
        {
            var fileMetadata = await _fileRepository.GetByIdAsync(fileId, includeDeleted: true); // Can delete soft-deleted metadata too
            if (fileMetadata == null)
            {
                return false;
            }

            // First, delete the physical file
            var filePath = Path.Combine(_uploadFolderPath, fileMetadata.StoredFileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Then, soft delete the metadata from the database
            await _fileRepository.DeleteAsync(fileId);
            await _fileRepository.SaveChangesAsync();
            return true;
        }

        public async Task<FileResponseDto?> GetFileMetadataAsync(Guid fileId)
        {
            var fileMetadata = await _fileRepository.GetByIdAsync(fileId);
            return fileMetadata == null ? null : _mapper.Map<FileResponseDto>(fileMetadata);
        }
    }
}
