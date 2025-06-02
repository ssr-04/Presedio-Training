// --- For Outgoing Calls to Flask API ---
using System.Text.Json.Serialization;

public class FlaskMessageDto
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = null!; // "user" or "agent"
    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;
}

public class FlaskQueryRequestDto
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = null!;

    [JsonPropertyName("conversation_id")]
    public string? ConversationId { get; set; } // Matches Flask's optional string type

    [JsonPropertyName("conversation_history")]
    public List<FlaskMessageDto>? ConversationHistory { get; set; } // List of messages
}

// --- For Incoming Responses from Flask API ---
public class FlaskSourceDto
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;

    [JsonPropertyName("page_number")]
    public int PageNumber { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = null!; // e.g., "HR Policy_2023.pdf"

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!; // e.g., "Internal Document"
}

public class FlaskRAGResponseDto
{
    [JsonPropertyName("agent_path_taken")]
    public string AgentPathTaken { get; set; } = null!;

    [JsonPropertyName("answer")]
    public string Answer { get; set; } = null!;

    [JsonPropertyName("cache_similarity")]
    public double? CacheSimilarity { get; set; } // Nullable if not cached

    [JsonPropertyName("cached")]
    public bool Cached { get; set; }

    [JsonPropertyName("confidence_score")]
    public double ConfidenceScore { get; set; }

    [JsonPropertyName("sources")]
    public List<FlaskSourceDto> Sources { get; set; } = new List<FlaskSourceDto>();
}