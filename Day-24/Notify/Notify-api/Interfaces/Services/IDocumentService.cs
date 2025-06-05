using System.Collections.Generic;
using System.Threading.Tasks;
using NotifyAPI.DTOs;


public interface IDocumentService
{
    Task<DocumentMetadataDto> UploadDocumentAsync(IFormFile file, DocumentUploadRequestDto metadata, string uploaderAuth0UserId);
    Task<IEnumerable<DocumentMetadataDto>> GetAllDocumentMetadataAsync();
    Task<(byte[]? content, string contentType, string originalFileName)?> DownloadDocumentAsync(string documentId);
    Task<bool> DeleteDocumentAsync(string documentId);
    Task<DocumentMetadataDto?> GetDocumentMetadataByIdAsync(string id); 
}
