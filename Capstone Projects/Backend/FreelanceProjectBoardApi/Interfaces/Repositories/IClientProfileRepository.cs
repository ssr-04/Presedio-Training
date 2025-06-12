using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface IClientProfileRepository : IGenericRepository<ClientProfile>
    {
        Task<ClientProfile?> GetClientProfileDetailsAsync(Guid id);
        Task<PageResult<ClientProfile>> GetAllClientProfileAsync(ClientFilter filter, PaginationParams pagination);
    }
}