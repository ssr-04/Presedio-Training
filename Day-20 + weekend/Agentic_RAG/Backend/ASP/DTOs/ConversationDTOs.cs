// Used for both New and Continue conversation requests
using System.ComponentModel.DataAnnotations;

public class QueryRequestDto
{
    [Required(ErrorMessage = "Query text is required.")]
    [MinLength(1, ErrorMessage = "Query cannot be empty.")]
    public string Query { get; set; } = null!;
}

// A simplified Source DTO for our own API responses, similar to Flask's but can be adapted
public class SourceResponseDto
{
    public string Content { get; set; } = null!;
    public int PageNumber { get; set; }
    public string Source { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Link { get; set; } // If you ever add external links
}

// Represents the full RAG response to send to the frontend
public class RAGResponseDto
{
    public string Answer { get; set; } = null!;
    public double ConfidenceScore { get; set; }
    public List<SourceResponseDto> Sources { get; set; } = new List<SourceResponseDto>();
    public string AgentPathTaken { get; set; } = null!;
    public bool Cached { get; set; }
    public double? CacheSimilarity { get; set; }
    public Guid? ConversationId { get; set; } // Added for new conversation responses
    public long? MessageId { get; set; } // Added for referencing the stored message
}

public class NewConversationResponseDto : RAGResponseDto
{
    // Inherits all properties from RAGResponseDto, but ensures ConversationId is always present
    [Required]
    public new Guid ConversationId { get; set; }
    [Required]
    public new long MessageId { get; set; }
}

public class ContinueConversationResponseDto : RAGResponseDto
{
    // Inherits all properties from RAGResponseDto
    [Required]
    public new Guid ConversationId { get; set; }
    [Required]
    public new long MessageId { get; set; }
}

public class ConversationSummaryDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
    public string? LastMessagePreview { get; set; } // Optional: A snippet of the last message
}

public class MessageResponseDto
{
    public long Id { get; set; }
    public string Role { get; set; } = null!; // "user" or "agent"
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}

public class ConversationDetailsDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
    public List<MessageResponseDto> Messages { get; set; } = new List<MessageResponseDto>(); // Will contain last 5 exchanges
}