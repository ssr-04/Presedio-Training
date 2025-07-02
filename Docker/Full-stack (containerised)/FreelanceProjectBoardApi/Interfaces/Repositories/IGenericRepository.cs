using System.Linq.Expressions;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, bool includeDeleted = false);
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "",
            bool includeDeleted = false
        );
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id); // Soft delete
        Task HardDeleteAsync(Guid id); //Permanent/Physical delete
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, bool includeDeleted = false);
        Task SaveChangesAsync();
    }
}