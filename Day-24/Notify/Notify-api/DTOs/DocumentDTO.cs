
using Microsoft.AspNetCore.Http; 
using System.ComponentModel.DataAnnotations;

namespace NotifyAPI.DTOs
{
    public class DocumentUploadRequestDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
    }

    public class DocumentMetadataDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedDate { get; set; }
        public string UploadedBy { get; set; } = string.Empty; 
        public string? Description { get; set; }
    }


    public class DocumentDownloadResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}