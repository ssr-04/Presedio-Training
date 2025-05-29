using Microsoft.EntityFrameworkCore;

public class TransactionRepository : GenericRepository<Transaction, Guid>, ITransactionRepository
{
    public TransactionRepository(BankingDBContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await _dbSet.Include(t => t.Account).ToListAsync();
    }

    public override async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _dbSet
                         .Include(t => t.Account)
                         .FirstOrDefaultAsync(t => t.TransactionId == id);

    }
    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        IQueryable<Transaction> query = _dbSet.Include(t => t.Account)
                                                .Where(t => t.AccountId == accountId);

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        }

        // Order by descending to get most recent transactions at start
        return await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
    }

    // For transactions, we typically do not allow hard deletes or soft deletes
    // as they represent a historical ledger. If a transaction needs to be "undone",
    // a new reversing transaction is created.
    public override Task DeleteAsync(Guid id)
    {
        Console.WriteLine($"Attempted to delete transaction with ID: {id}. Transactions cannot be deleted.");
        return Task.CompletedTask;
    }
}