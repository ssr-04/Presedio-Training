using Microsoft.EntityFrameworkCore;
public class CustomerRepository : GenericRepository<Customer, Guid>, ICustomerRepository
{
    public CustomerRepository(BankingDBContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _dbSet.Where(c => c.IsActive)
                        .Include(c => c.Accounts).ToListAsync(); //eager loads accounts
    }

    public override async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _dbSet
                        .Include(c => c.Accounts) // Eager load related accounts
                        .FirstOrDefaultAsync(c => c.CustomerId == id);
    }

    public async Task<Customer?> GetCustomerByNationalIdAsync(string nationalId)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.NationalId == nationalId);
    }

    // Override DeleteAsync for soft delete
    public override async Task DeleteAsync(Guid id)
    {
        var customer = await GetByIdAsync(id);
        if (customer != null)
        {
            customer.IsActive = false; // Setting IsActive to false for soft delete
            _context.Entry(customer).State = EntityState.Modified; // Marking as modified to save changes later
        }
    }
}