using System;

namespace FileHandlingApi.Models
{
    public class FileMetadata
    {
        public string Id { get; set; } = string.Empty; // GUID for the stored file name (uniqueness)
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty; // MIME type (e.g., "image/jpeg", "application/pdf")
        public long FileSize { get; set; } // Size in bytes
        public DateTime UploadedDate { get; set; }
    }
}