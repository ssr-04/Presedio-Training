using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceProjectBoardApi.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(FreelanceContext context) : base(context)
        {
        }

        // Using ExecuteUpdateAsync for a bulk update is much more performant than fetching all entities.
        public async Task<int> MarkAllAsReadAsync(Guid receiverId)
        {
            return await _dbSet
                .Where(n => !n.IsDeleted && n.ReceiverId == receiverId && !n.IsRead)
                .ExecuteUpdateAsync(updates => updates.SetProperty(n => n.IsRead, true));
        }
    }
}