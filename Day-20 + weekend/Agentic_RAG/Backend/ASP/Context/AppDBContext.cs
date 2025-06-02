using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Define DbSet properties for each of your models
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- User Model Configuration ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id); // Primary Key
            entity.Property(u => u.Id).ValueGeneratedOnAdd(); // EF generates GUID on Add
            entity.Property(u => u.Username).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Username).IsUnique(); // Unique index for Username

            entity.Property(u => u.Email).HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique(); // Unique index for Email (assuming unique email required)

            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255); // Adjust length based on hashing algorithm output
            entity.Property(u => u.CreatedAt).IsRequired();
            // LastLoginAt is nullable by default for DateTimeOffset?

            // One-to-Many relationship: User has many Conversations
            entity.HasMany(u => u.Conversations)
                    .WithOne(c => c.User)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // If User is deleted, delete their Conversations
        });

        // --- Conversation Model Configuration ---
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(c => c.Id); // Primary Key
            entity.Property(c => c.Id).ValueGeneratedOnAdd(); // EF generates GUID on Add
            entity.Property(c => c.Title).HasMaxLength(255); // Title is nullable by default for string?
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.Property(c => c.LastUpdatedAt).IsRequired();

            // Foreign Key is configured on the User side (UserId)
            // Navigation property is configured above
            // Many-to-One: Conversation belongs to one User
            entity.HasOne(c => c.User) // Conversation has one User
                    .WithMany(u => u.Conversations) // User has many Conversations
                    .HasForeignKey(c => c.UserId) // Foreign key in Conversation table
                    .IsRequired(); // UserId is required

            // One-to-Many relationship: Conversation has many Messages
            entity.HasMany(c => c.Messages)
                    .WithOne(m => m.Conversation)
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade); // If Conversation is deleted, delete its Messages
        });

        // --- Message Model Configuration ---
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id); // Primary Key
            entity.Property(m => m.Id).ValueGeneratedOnAdd(); // PostgreSQL BIGSERIAL will auto-generate

            entity.Property(m => m.Role).IsRequired().HasMaxLength(50);
            entity.Property(m => m.Content).IsRequired(); // TEXT in PostgreSQL (no max length specified)

            // Configure FullResponseJson to be stored as JSONB in PostgreSQL
            entity.Property(m => m.FullResponseJson)
                    .HasColumnType("jsonb"); // Specific to Npgsql for JSONB type

            entity.Property(m => m.CreatedAt).IsRequired();

            // Foreign Key is configured on the Conversation side (ConversationId)
            // Navigation property is configured above
            // Many-to-One: Message belongs to one Conversation
            entity.HasOne(m => m.Conversation) // Message has one Conversation
                    .WithMany(c => c.Messages) // Conversation has many Messages
                    .HasForeignKey(m => m.ConversationId) // Foreign key in Message table
                    .IsRequired(); // ConversationId is required
        });
    }
}