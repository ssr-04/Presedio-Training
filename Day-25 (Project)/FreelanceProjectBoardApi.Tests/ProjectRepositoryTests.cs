// In FreelanceProjectBoardApi.Tests/Repositories/ProjectRepositoryTests.cs

using FluentAssertions;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;
using FreelanceProjectBoardApi.Helpers;
using Microsoft.AspNetCore.Diagnostics;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class ProjectRepositoryTests : RepositoryTestBase
{
    private readonly ProjectRepository _projectRepository;
    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly Guid CSharpSkillId = Guid.NewGuid();
    private static readonly Guid ReactSkillId = Guid.NewGuid();

    public ProjectRepositoryTests()
    {
        _projectRepository = new ProjectRepository(_context);
    }

    protected override void SeedDatabase()
    {
        var client = new User { Id = ClientId, Email = "client@test.com", Type = UserType.Client };
        _context.Users.Add(client);
        var csharpSkill = new Skill { Id = CSharpSkillId, Name = "C#" };
        _context.Skills.AddRange(csharpSkill, new Skill { Id = ReactSkillId, Name = "React" });

        var projects = new List<Project>
        {
            new Project { Id = ProjectId, Title = "API Development", Description = "A C# project.", Budget = 5000, Status = ProjectStatus.Open, ClientId = ClientId, Client = client },
            new Project { Title = "Frontend App", Description = "A React project.", Budget = 3000, Status = ProjectStatus.InProgress, ClientId = ClientId, Client = client  },
            new Project { Title = "Database Work", Description = "SQL needed.", Budget = 1500, Status = ProjectStatus.Open, ClientId = ClientId, Client = client  },
            new Project { Title = "Deleted Project", IsDeleted = true, ClientId = ClientId, Client = client  }
        };
        _context.Projects.AddRange(projects);
        _context.ProjectSkills.Add(new ProjectSkill { ProjectId = ProjectId, SkillId = CSharpSkillId, Project = projects[0], Skill = csharpSkill });
    }

    [Fact]
    public async Task GetAllProjectsAsync_NoFilter_ReturnsAllNonDeleted()
    {
        var result = await _projectRepository.GetAllProjectsAsync(new ProjectFilter(), new PaginationParams());
        result.Data.Should().HaveCount(3);
        result.pagination.TotalRecords.Should().Be(3);
    }

    [Theory]
    [InlineData("API", 1)]
    [InlineData("project", 2)]
    [InlineData("nonexistent", 0)]
    public async Task GetAllProjectsAsync_FilterBySearchQuery_ReturnsMatching(string query, int expectedCount)
    {
        var filter = new ProjectFilter();
        filter.SearchQuery = query;
        var result = await _projectRepository.GetAllProjectsAsync(filter, new PaginationParams());
        result.Data.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task GetAllProjectsAsync_FilterByStatus_ReturnsMatching()
    {
        var filter = new ProjectFilter();
        filter.Status = "Open";
        var result = await _projectRepository.GetAllProjectsAsync(filter, new PaginationParams());
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(p => p.Status == ProjectStatus.Open);
    }

    [Fact]
    public async Task GetAllProjectsAsync_FilterBySkill_ReturnsMatching()
    {
        var filter = new ProjectFilter();
        filter.SkillNames = new List<string> { "C#" };
        var result = await _projectRepository.GetAllProjectsAsync(filter, new PaginationParams());
        result.Data.Should().ContainSingle().Which.Title.Should().Be("API Development");
    }

    [Fact]
    public async Task GetAllProjectsAsync_SortByBudgetDesc_ReturnsCorrectOrder()
    {
        var pagination = new PaginationParams();
        pagination.SortBy = "Budget";
        pagination.SortOrder = "desc";
        var result = await _projectRepository.GetAllProjectsAsync(new ProjectFilter(), pagination);
        result.Data.Should().BeInDescendingOrder(p => p.Budget);
    }

    [Fact]
    public async Task GetAllProjectsAsync_SortByInvalidColumn_UsesDefaultSort()
    {
        var pagination = new PaginationParams();
        pagination.SortBy = "Invalid";
        pagination.SortOrder = "desc";
        var result = await _projectRepository.GetAllProjectsAsync(new ProjectFilter(), pagination);
        result.Data.Should().BeInDescendingOrder(p => p.CreatedAt);
    }

    [Fact]
    public async Task GetProjectDetailsAsync_WithValidId_ReturnsProjectWithIncludes()
    {
        var project = await _projectRepository.GetProjectDetailsAsync(ProjectId);
        project.Should().NotBeNull();
        project!.Client.Should().NotBeNull();
        project.ProjectSkills.Should().NotBeEmpty();
    }

}