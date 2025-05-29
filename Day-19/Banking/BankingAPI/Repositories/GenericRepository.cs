
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

public class GenericRepository<TEntity, TId> : IGenericRepository<TEntity, TId> where TEntity : class
{
    /*
        NOTE: HERE SAVE CHANGES IS A SEPERATE METHOD INSEAD OF CALLING IT AFTER EACH CRUD OPERATIONS BECAUSE
        WE HAVE MORE CONTROL ON WHEN TO COMMIT CHANGES, LIKE GROUPING MULTIPLE OPERATIONS TOGETHER (USEFUL IN
        BANKING APLLICATION TO ENABLE ATOMICITY OF OPERATIONS AND MORE CONTROL)
        EXTRA: MINIMAL PERFORMANCE BOOST AS ROUNTRIP TO DB IS CALLED ONLY WHEN NEEDED
    */
    protected readonly BankingDBContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(BankingDBContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }
    public virtual async Task AddAsync(TEntity entity) //virtual so we can override in childs
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual async Task DeleteAsync(TId id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity); 
        }
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity); // marks the entity as modified, need to run save changes explicitly
        return Task.CompletedTask;
    }
}