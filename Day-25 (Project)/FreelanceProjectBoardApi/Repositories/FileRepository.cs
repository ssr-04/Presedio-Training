using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Interfaces.Repositories;

namespace FreelanceProjectBoardApi.Repositories
{
    public class FileRepository : GenericRepository<Models.File>, IFileRepository
    {
        public FileRepository(FreelanceContext context) : base(context) { }


    }

}