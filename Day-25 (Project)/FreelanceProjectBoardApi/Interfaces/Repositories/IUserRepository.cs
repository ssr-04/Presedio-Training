using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetUserByEmailAsync(string email, bool includeProfiles = false);
        Task<PageResult<User>> GetAllUsersAsync(UserFilter filter, PaginationParams pagination, bool includeProfiles = false);
    }
}