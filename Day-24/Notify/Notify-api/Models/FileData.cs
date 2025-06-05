using System.ComponentModel.DataAnnotations;

public class FileData
{
    [Key]
    [StringLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString(); 

    [Required]
    [StringLength(255)] // Max length for filename
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)] 
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; } 

    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(256)] 
    public string UploadedByAuth0UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty; 

    [StringLength(1000)] 
    public string? Description { get; set; }

    [Required]
    public byte[] Content { get; set; } = Array.Empty<byte>(); 
}