using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotifyService.Repositories
{
    public interface IDocumentRepository
    {
        Task<FileData> AddDocumentAsync(FileData document);
        Task<FileData?> GetDocumentByIdAsync(string id);
        Task<List<FileData>> GetAllDocumentMetadataAsync(); // Returns only metadata without content
        Task<FileData?> GetDocumentContentByIdAsync(string id); // for content download
        Task<FileData?> GetDocumentMetadataByIdAsync(string id); 
        Task<bool> DeleteDocumentAsync(string id);
    }
}