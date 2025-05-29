public interface IAccountRepository : IGenericRepository<Account, Guid>
{
    Task<Account?> GetAccountByAccountNumberAsync(string accountNumber);
    Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(Guid customerId);
}