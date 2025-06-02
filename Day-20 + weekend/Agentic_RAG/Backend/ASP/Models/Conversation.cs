
public class Conversation
{
    public Guid Id { get; set; } // Primary Key (UUID), this will be the conversation_id

    // Foreign Key to User
    public Guid UserId { get; set; }

    public string? Title { get; set; } // Nullable, e.g., derived from first query

    public DateTimeOffset CreatedAt { get; set; } // Timestamp of conversation start
    public DateTimeOffset LastUpdatedAt { get; set; } // Timestamp of last message in conversation

    // Navigation properties for EF Core
    // Many-to-One: Conversation belongs to one User
    public User User { get; set; } = null!; // 'null!' tells compiler it will be initialized by EF

    // One-to-Many: Conversation has many Messages
    public ICollection<Message> Messages { get; set; } = [];
}