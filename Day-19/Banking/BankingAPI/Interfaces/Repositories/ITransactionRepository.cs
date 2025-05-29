public interface ITransactionRepository : IGenericRepository<Transaction, Guid>
{
    Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
}