using System.Linq.Expressions;

public interface IGenericRepository<TEntity, TId> where TEntity : class
{
    Task AddAsync(TEntity entity);
    Task<TEntity?> GetByIdAsync(TId id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate); // fnd based on predicate like x => x.IsActive == true
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TId id); // soft delete
    Task<int> SaveChangesAsync(); // save all changes made in this context
}