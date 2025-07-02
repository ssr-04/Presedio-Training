using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceProjectBoardApi.Repositories
{
    public class RatingRepository : GenericRepository<Rating>, IRatingRepository

    {
        public RatingRepository(FreelanceContext context) : base(context)
        {
            
        }

        public async Task<double> GetAverageRatingForProjectAsync(Guid projectId)
        {
            var ratings = await _dbSet
                .Where(r => !r.IsDeleted && r.ProjectId == projectId)
                .Select(r => (double?)r.RatingValue)
                .ToListAsync();

            return ratings.Any() ? (ratings.Average() ?? 0.0) : 0.0;
        }

        public async Task<double> GetAverageRatingForUserAsync(Guid userId)
        {
            var ratings = await _dbSet
                .Where(r => !r.IsDeleted && r.RateeId == userId)
                .Select(r => (double)r.RatingValue)
                .ToListAsync();
            
            return ratings.Any() ? ratings.Average() : 0.0;
        }

        public async Task<IEnumerable<Rating>> GetRatingsGivenByUserAsync(Guid userId, bool includeRatee = false)
        {
            IQueryable<Rating> query = _dbSet.Where(r => !r.IsDeleted && r.RaterId == userId);
            if (await query.CountAsync() == 0)
            {
                return new List<Rating>();
            }
            if (includeRatee)
                {
                    query = query.Include(r => r.Ratee).Include(r => r.Project); // Include project context
                }
            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Rating>> GetRatingsReceivedByUserAsync(Guid userId, bool includeRater = false)
        {
            IQueryable<Rating> query = _dbSet.Where(r => !r.IsDeleted && r.RateeId == userId);
            if (await query.CountAsync() == 0)
            {
                return new List<Rating>();
            }
            if (includeRater)
            {
                query = query.Include(r => r.Rater).Include(r => r.Project); // Include project context
            }
            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }
    }
}