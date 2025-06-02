
public class Message
{
    public long Id { get; set; } // Primary Key (BIGSERIAL in PostgreSQL)

    // Foreign Key to Conversation
    public Guid ConversationId { get; set; }

    public string Role { get; set; } = null!; // "user" or "agent"
    public string Content { get; set; } = null!; // The actual query text or agent's answer

    // Stores the full JSON response from the Flask RAG API for agent messages.
    // Will be mapped to JSONB in PostgreSQL.
    public string? FullResponseJson { get; set; } // Nullable, for user messages this will be null

    public DateTimeOffset CreatedAt { get; set; } // Timestamp of when the message was recorded

    // Navigation property for EF Core
    // Many-to-One: Message belongs to one Conversation
    public Conversation Conversation { get; set; } = null!; // 'null!' tells compiler it will be initialized by EF
}