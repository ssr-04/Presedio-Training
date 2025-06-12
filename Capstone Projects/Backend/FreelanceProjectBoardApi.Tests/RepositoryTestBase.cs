using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public abstract class RepositoryTestBase : IDisposable
{
    protected readonly FreelanceContext _context;

    protected RepositoryTestBase()
    {
        var options = new DbContextOptionsBuilder<FreelanceContext>()
            // Using a unique name for each test class to ensure isolation
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FreelanceContext(options, new HttpContextAccessor()); 
        
        SeedDatabase();
        _context.SaveChanges();
    }
    

    protected virtual void SeedDatabase()
    {
        
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}