using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.Models
{
    public enum FileCategory
    {
        ProfilePicture, Resume, ProjectAttachment, ProposalAttachment, ProjectSubmission, Other
    }

    public class File : BaseEntity
    {
        [Required]
        [MaxLength(256)]
        public string FileName { get; set; } = string.Empty; // Original file name

        [Required]
        [MaxLength(256)]
        public string StoredFileName { get; set; } = string.Empty; // Unique name (Guid)

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty; // Relative path on the server

        [Required]
        [MaxLength(100)]
        public string MimeType { get; set; } = string.Empty;

        public long FileSize { get; set; } // in bytes

        public Guid UploaderId { get; set; } // FK to User who uploaded
        public User Uploader { get; set; } = default!; // Navigation property

        public FileCategory Category { get; set; } = FileCategory.Other;

        // Foreign keys to link files to other models
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }

        public Guid? ProposalId { get; set; }
        public Proposal? Proposal { get; set; }

    }
}
