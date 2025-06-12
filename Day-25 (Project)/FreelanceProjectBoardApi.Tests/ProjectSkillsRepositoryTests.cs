using FluentAssertions;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class ProjectSkillRepositoryTests : RepositoryTestBase
{
    private readonly ProjectSkillRepository _projectSkillRepository;
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly Guid SkillId1 = Guid.NewGuid();
    private static readonly Guid SkillId2 = Guid.NewGuid();
    private static readonly Guid UnrelatedSkillId = Guid.NewGuid();

    public ProjectSkillRepositoryTests()
    {
        _projectSkillRepository = new ProjectSkillRepository(_context);
    }

    protected override void SeedDatabase()
    {
        var user = new User { Email = "client@test.com", Type = UserType.Client };
        _context.Users.Add(user);

        var project = new Project { Id = ProjectId, Title = "Complex Project", ClientId = user.Id, Client = user };
        _context.Projects.Add(project);

        var skills = new List<Skill>
        {
            new Skill { Id = SkillId1, Name = "Azure" },
            new Skill { Id = SkillId2, Name = "Terraform" },
            new Skill { Id = UnrelatedSkillId, Name = "Photoshop" }
        };
        _context.Skills.AddRange(skills);

        var projectSkills = new List<ProjectSkill>
        {
            new ProjectSkill { ProjectId = ProjectId, SkillId = SkillId1, Project = project, Skill = skills[0] },
            new ProjectSkill { ProjectId = ProjectId, SkillId = SkillId2, Project= project, Skill = skills[1] }
        };
        _context.ProjectSkills.AddRange(projectSkills);
    }

    [Fact]
    public async Task GetSkillsForProjectAsync_WithValidProjectId_ReturnsAllRequiredSkills()
    {
        // Act
        var result = await _projectSkillRepository.GetSkillsForProjectAsync(ProjectId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(ps => ps.Skill.Name == "Azure");
        result.Should().Contain(ps => ps.Skill.Name == "Terraform");
        result.Should().NotContain(ps => ps.Skill.Name == "Photoshop");
    }

    [Fact]
    public async Task GetSkillsForProjectAsync_WithInvalidProjectId_ReturnsEmptyList()
    {
        // Act
        var result = await _projectSkillRepository.GetSkillsForProjectAsync(Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ProjectRequiresSkillAsync_WhenSkillIsRequired_ReturnsTrue()
    {
        // Act
        var result = await _projectSkillRepository.ProjectRequiresSkillAsync(ProjectId, SkillId1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ProjectRequiresSkillAsync_WhenSkillIsNotRequired_ReturnsFalse()
    {
        // Act
        var result = await _projectSkillRepository.ProjectRequiresSkillAsync(ProjectId, UnrelatedSkillId);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task ProjectRequiresSkillAsync_WithInvalidProjectId_ReturnsFalse()
    {
        // Act
        var result = await _projectSkillRepository.ProjectRequiresSkillAsync(Guid.NewGuid(), SkillId1);

        // Assert
        result.Should().BeFalse();
    }
}