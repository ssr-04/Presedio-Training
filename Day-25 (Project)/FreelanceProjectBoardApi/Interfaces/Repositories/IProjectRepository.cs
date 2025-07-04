using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Helpers;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<Project?> GetProjectDetailsAsync(Guid id);
        Task<PageResult<Project>> GetAllProjectsAsync(ProjectFilter filter, PaginationParams pagination);
        Task<IEnumerable<Project>> GetProjectsByUser(Guid userId);
    }
}