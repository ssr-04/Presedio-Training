using Microsoft.EntityFrameworkCore;
using NotifyService.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotifyService.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly FileManagement _context;

        public DocumentRepository(FileManagement context)
        {
            _context = context;
        }

        public async Task<FileData> AddDocumentAsync(FileData document)
        {
            if (string.IsNullOrEmpty(document.Id))
            {
                document.Id = Guid.NewGuid().ToString();
            }

            if (document.UploadedDate == default)
            {
                document.UploadedDate = DateTime.UtcNow;
            }

            _context.Files.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<FileData?> GetDocumentByIdAsync(string id)
        {
            return await _context.Files.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<FileData>> GetAllDocumentMetadataAsync()
        {
            // Selects only the metadata fields, excludes the large byte[] Content field.
            return await _context.Files
                                 .Select(f => new FileData
                                 {
                                     Id = f.Id,
                                     Title = f.Title,
                                     OriginalFileName = f.OriginalFileName,
                                     ContentType = f.ContentType,
                                     FileSize = f.FileSize,
                                     UploadedDate = f.UploadedDate,
                                     UploadedByAuth0UserId = f.UploadedByAuth0UserId,
                                     Description = f.Description,
                                     Content = Array.Empty<byte>() // Provide an empty array for this partial object
                                 })
                                 .OrderByDescending(f => f.UploadedDate)
                                 .ToListAsync();
        }

        public async Task<FileData?> GetDocumentContentByIdAsync(string id)
        {
            // Specifically fetches the FileData object with its Content.
            return await _context.Files.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<bool> DeleteDocumentAsync(string id)
        {
            var document = await _context.Files.FirstOrDefaultAsync(f => f.Id == id);
            if (document == null)
            {
                return false; // Document not found
            }

            _context.Files.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FileData?> GetDocumentMetadataByIdAsync(string id)
        {
            return await _context.Files
                                 .Where(f => f.Id == id)
                                 .Select(f => new FileData
                                 {
                                     Id = f.Id,
                                     Title = f.Title,
                                     OriginalFileName = f.OriginalFileName,
                                     ContentType = f.ContentType,
                                     FileSize = f.FileSize,
                                     UploadedDate = f.UploadedDate,
                                     UploadedByAuth0UserId = f.UploadedByAuth0UserId,
                                     Description = f.Description,
                                     Content = Array.Empty<byte>()
                                 })
                                 .FirstOrDefaultAsync();
        }

    }
    
}