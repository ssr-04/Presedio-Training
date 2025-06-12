using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface IFileRepository : IGenericRepository<Models.File>
    {
        // To Add any file-specific methods if needed in future
    }
}
