using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface IFreelancerProfileRepository : IGenericRepository<FreelancerProfile>
    {
        Task<FreelancerProfile?> GetFreelancerProfileDetailsAsync(Guid id); // Include freelancer related details
        Task<PageResult<FreelancerProfile>> GetAllFreelancerProfilesAsync(FreelancerFilter filter, PaginationParams pagination);

    }
}