using Microsoft.EntityFrameworkCore;
public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(long id)
    {
        return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Message>> GetAllByConversationIdAsync(Guid conversationId)
    {
        // Order messages by creation time to maintain conversation flow
        return await _context.Messages
                                .Where(m => m.ConversationId == conversationId)
                                .OrderBy(m => m.CreatedAt)
                                .ToListAsync();
    }

    public async Task AddAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
    }

    public Task DeleteAsync(Message message)
    {
        _context.Messages.Remove(message);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}