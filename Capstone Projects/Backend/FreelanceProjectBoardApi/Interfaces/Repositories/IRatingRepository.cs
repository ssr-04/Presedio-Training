using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface IRatingRepository : IGenericRepository<Rating>
    {
        Task<IEnumerable<Rating>> GetRatingsReceivedByUserAsync(Guid userId, bool includeRater = false);
        Task<IEnumerable<Rating>> GetRatingsGivenByUserAsync(Guid userId, bool includeRatee = false);
        Task<double> GetAverageRatingForUserAsync(Guid userId);
        Task<double> GetAverageRatingForProjectAsync(Guid projectId);
    }
}