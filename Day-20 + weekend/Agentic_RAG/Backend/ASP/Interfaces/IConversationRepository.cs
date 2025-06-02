public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Conversation>> GetAllByUserIdAsync(Guid userId); // Get all conversations for a specific user
    Task AddAsync(Conversation conversation);
    Task UpdateAsync(Conversation conversation); // For updating LastUpdatedAt, Title
    Task DeleteAsync(Conversation conversation); // Or DeleteByIdAsync(Guid id)
    Task<bool> SaveChangesAsync(); // For Unit of Work pattern
}