public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(long id);
    Task<IEnumerable<Message>> GetAllByConversationIdAsync(Guid conversationId); // Get all messages for a specific conversation
    Task AddAsync(Message message);
    // Update is less common for messages, but could be added if content can be edited
    Task DeleteAsync(Message message); // Or DeleteByIdAsync(long id)
    Task<bool> SaveChangesAsync(); // For Unit of Work pattern
}