using Microsoft.EntityFrameworkCore;

public class AccountRepository : GenericRepository<Account, Guid>, IAccountRepository
    {
        public AccountRepository(BankingDBContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _dbSet.Include(a => a.Customer)
                                .Where(a => a.IsActive)
                                .ToListAsync();
        }

        public async override Task<Account?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                            .Include(a => a.Customer) 
                            .FirstOrDefaultAsync(a => a.AccountId == id);

        }

        public async Task<Account?> GetAccountByAccountNumberAsync(string accountNumber)
        {
            return await _dbSet
                                .Include(a => a.Customer)
                                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(Guid customerId)
        {
            return await _dbSet
                                .Include(a => a.Customer)
                                .Where(a => a.CustomerId == customerId).ToListAsync();
        }

        // Overriding DeleteAsync for soft delete for Account
        public override async Task DeleteAsync(Guid id)
        {
            var account = await GetByIdAsync(id);
            if (account != null)
            {
                account.IsActive = false; // Setting IsActive to false for soft delete
                _context.Entry(account).State = EntityState.Modified; // Mark as modified to save changes later
            }
        }
    }