
public class User
{
    public Guid Id { get; set; } // Primary Key (UUID in PostgreSQL)
    public required string Username { get; set; } // Unique, for login
    public required string Email { get; set; } // Unique, optional
    public required string PasswordHash { get; set; } // Stores the hashed password

    public DateTimeOffset CreatedAt { get; set; } // Timestamp of user registration
    public DateTimeOffset? LastLoginAt { get; set; } // Nullable, timestamp of last login

    // Navigation property for EF Core (One-to-Many: User has many Conversations)
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}