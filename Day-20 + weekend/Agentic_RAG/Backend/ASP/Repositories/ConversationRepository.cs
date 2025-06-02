using Microsoft.EntityFrameworkCore;
public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;

    public ConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id)
    {
        // Eager load Messages if you often need messages when fetching a conversation by ID.
        // However, for 'get last 5 exchanges' you'll likely use GetAllByConversationIdAsync
        // and filter there. So for a pure GetById, often no Include is needed unless specifically for UI/service logic.
        return await _context.Conversations
                                // .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(5)) // Example if you want top 5 here
                                .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Conversation>> GetAllByUserIdAsync(Guid userId)
    {
        // Order by last updated to show most recent conversations first
        return await _context.Conversations
                                .Where(c => c.UserId == userId)
                                .OrderByDescending(c => c.LastUpdatedAt)
                                .ToListAsync();
    }

    public async Task AddAsync(Conversation conversation)
    {
        await _context.Conversations.AddAsync(conversation);
    }

    public Task UpdateAsync(Conversation conversation)
    {
        _context.Conversations.Update(conversation);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Conversation conversation)
    {
        _context.Conversations.Remove(conversation);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}