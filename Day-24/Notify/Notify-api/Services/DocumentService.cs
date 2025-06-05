
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using NotifyAPI.DTOs;
using NotifyService.Hubs;
using NotifyService.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IMapper _mapper;

    public DocumentService(IDocumentRepository documentRepository,
                            IHubContext<NotificationHub> hubContext,
                            IMapper mapper)
    {
        _documentRepository = documentRepository;
        _hubContext = hubContext;
        _mapper = mapper;
    }

    public async Task<DocumentMetadataDto> UploadDocumentAsync(IFormFile file, DocumentUploadRequestDto metadata, string uploaderAuth0UserId)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null.", nameof(file));
        }
        if (string.IsNullOrEmpty(metadata.Title))
        {
            throw new ArgumentException("Document title cannot be empty.", nameof(metadata.Title));
        }

        // Reading file content into byte array
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileContent = memoryStream.ToArray();

        // Creating FileData model
        var newDocument = new FileData
        {
            Id = Guid.NewGuid().ToString(),
            Title = metadata.Title,
            Description = metadata.Description,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedDate = DateTime.UtcNow,
            UploadedByAuth0UserId = uploaderAuth0UserId,
            Content = fileContent
        };

        // Saving to database
        var addedDocument = await _documentRepository.AddDocumentAsync(newDocument);

        // Mapping to DTO for notification and response
        var documentDto = _mapper.Map<DocumentMetadataDto>(addedDocument);

        // Sending real-time notification to all connected clients
        await _hubContext.Clients.All.SendAsync("ReceiveDocumentNotification", documentDto);

        return documentDto; // Return the metadata DTO to the controller
    }

    public async Task<IEnumerable<DocumentMetadataDto>> GetAllDocumentMetadataAsync()
    {
        var documents = await _documentRepository.GetAllDocumentMetadataAsync();
        return _mapper.Map<IEnumerable<DocumentMetadataDto>>(documents);
    }

    public async Task<(byte[]? content, string contentType, string originalFileName)?> DownloadDocumentAsync(string documentId)
    {
        var document = await _documentRepository.GetDocumentContentByIdAsync(documentId);

        if (document == null || document.Content == null || document.Content.Length == 0)
        {
            return null;
        }

        return (document.Content, document.ContentType, document.OriginalFileName);
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        var deleted = await _documentRepository.DeleteDocumentAsync(documentId);
        if (deleted)
        {
            Console.WriteLine($"Document {documentId} deleted.");
            await _hubContext.Clients.All.SendAsync("DocumentDeleted", documentId);
        }
        return deleted;
    }
    public async Task<DocumentMetadataDto?> GetDocumentMetadataByIdAsync(string id)
    {
        var document = await _documentRepository.GetDocumentMetadataByIdAsync(id);
        return document == null ? null : _mapper.Map<DocumentMetadataDto>(document);
    }

}