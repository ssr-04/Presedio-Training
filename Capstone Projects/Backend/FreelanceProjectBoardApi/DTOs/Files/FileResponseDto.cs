using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.DTOs.Files
{
    public class FileResponseDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public FileCategory Category { get; set; }
        public Guid UploaderId { get; set; }
        public DateTime CreatedAt { get; set; }

        // FilePath/StoredFileName are internal details, so not exposed directly
    }
}
