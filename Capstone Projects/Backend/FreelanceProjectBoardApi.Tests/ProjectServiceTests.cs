using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using FreelanceProjectBoardApi.Services.Implementations;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.DTOs.Projects;
using FreelanceProjectBoardApi.Services.Interfaces;
using FreelanceProjectBoardApi.Helpers;
using Microsoft.AspNetCore.Http;
using FreelanceProjectBoardApi.DTOs.Files;
using System.Linq.Expressions;
using FreelanceProjectBoardApi.DTOs.Skills;

namespace FreelanceProjectBoardApi.Tests.Services
{
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IFreelancerProfileRepository> _freelancerRepositoryMock;
        private readonly Mock<ISkillRepository> _skillRepositoryMock;
        private readonly Mock<IProjectSkillRepository> _projectSkillRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IProposalService> _proposalServiceMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly IProjectService _projectService;

        public ProjectServiceTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _freelancerRepositoryMock = new Mock<IFreelancerProfileRepository>();
            _skillRepositoryMock = new Mock<ISkillRepository>();
            _projectSkillRepositoryMock = new Mock<IProjectSkillRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _proposalServiceMock = new Mock<IProposalService>();
            _mapperMock = new Mock<IMapper>();

            _projectService = new ProjectService(
                _projectRepositoryMock.Object,
                _userRepositoryMock.Object,
                _freelancerRepositoryMock.Object,
                _skillRepositoryMock.Object,
                _projectSkillRepositoryMock.Object,
                _fileServiceMock.Object,
                _proposalServiceMock.Object,
                _mapperMock.Object
            );

        }

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_WithValidClientAndFutureDeadline_ShouldSucceed()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var client = new User { Id = clientId, Type = UserType.Client };
            var futureDeadline = DateTime.Now.AddDays(10).ToString("dd-MM-yyyy HH:mm");
            var createDto = new CreateProjectDto { Title = "New Project", Deadline = futureDeadline, Skills = new List<CreateSkillDto>() };
            var project = new Project { Id = Guid.NewGuid(), ClientId = clientId, Title = "New Project", Status = ProjectStatus.Open, Client = client };
            var responseDto = new ProjectResponseDto { Id = project.Id, Title = "New Project" };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(clientId, false)).ReturnsAsync(client);
            _mapperMock.Setup(m => m.Map<Project>(createDto)).Returns(project);
            _projectRepositoryMock.Setup(r => r.AddAsync(project)).ReturnsAsync(project);
            _projectRepositoryMock.Setup(r => r.GetProjectDetailsAsync(project.Id)).ReturnsAsync(project);
            _mapperMock.Setup(m => m.Map<ProjectResponseDto>(project)).Returns(responseDto);

            // Act
            var result = await _projectService.CreateProjectAsync(clientId, createDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseDto);
            _projectRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);
            _projectRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateProjectAsync_WithPastDeadline_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var client = new User { Id = clientId, Type = UserType.Client };
            var pastDeadline = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy HH:mm");
            var createDto = new CreateProjectDto { Title = "Invalid Project", Deadline = pastDeadline };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(clientId, false)).ReturnsAsync(client);
            
            // Act
            Func<Task> act = async () => await _projectService.CreateProjectAsync(clientId, createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("deadline must be in the future.");
        }

        [Fact]
        public async Task CreateProjectAsync_WhenUserIsNotClient_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Freelancer }; // Not a client
            var createDto = new CreateProjectDto { Deadline = DateTime.Now.AddDays(1).ToString("dd-MM-yyyy HH:mm") };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);

            // Act
            var result = await _projectService.CreateProjectAsync(userId, createDto);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_WhenProjectExists_ShouldReturnDto()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var client = new User { Id = Guid.NewGuid(), Type = UserType.Client };
            var project = new Project { Id = projectId, Client = client };
            var responseDto = new ProjectResponseDto { Id = projectId };

            _projectRepositoryMock.Setup(r => r.GetProjectDetailsAsync(projectId)).ReturnsAsync(project);
            _mapperMock.Setup(m => m.Map<ProjectResponseDto>(project)).Returns(responseDto);

            // Act
            var result = await _projectService.GetProjectByIdAsync(projectId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseDto);
        }
        
        #endregion

        #region AssignFreelancerToProjectAsync Tests

        [Fact]
        public async Task AssignFreelancerToProjectAsync_WithOpenProjectAndValidFreelancer_ShouldSucceed()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var freelancerId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var clientUser = new User { Id = clientId, Type = UserType.Client };
            var client = new ClientProfile { User = clientUser };
            var freelancerUser = new User { Id = freelancerId, Type = UserType.Freelancer };
            var freelancer = new FreelancerProfile { User = freelancerUser };
            var proposalId = Guid.NewGuid();
            var project = new Project { Id = projectId, Status = ProjectStatus.Open, Client = clientUser };
            var proposal = new Proposal { Id = proposalId, FreelancerId = freelancerId, Project = project, Freelancer = freelancerUser };
            project.Proposals = new List<Proposal> { proposal };

            _projectRepositoryMock.Setup(r => r.GetProjectDetailsAsync(projectId)).ReturnsAsync(project);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(freelancerId, false)).ReturnsAsync(freelancerUser);
            _mapperMock.Setup(m => m.Map<ProjectResponseDto>(It.IsAny<Project>())).Returns(new ProjectResponseDto());

            // Act
            var result = await _projectService.AssignFreelancerToProjectAsync(projectId, freelancerId);

            // Assert
            result.Should().NotBeNull();
            project.Status.Should().Be(ProjectStatus.Assigned);
            project.AssignedFreelancerId.Should().Be(freelancerId);
            _projectRepositoryMock.Verify(r => r.UpdateAsync(project), Times.Once);
            _proposalServiceMock.Verify(p => p.UpdateProposalStatusAsync(proposalId, It.IsAny<DTOs.Proposals.UpdateProposalStatusDto>()), Times.Once);
        }

        [Fact]
        public async Task AssignFreelancerToProjectAsync_WhenProjectNotOpen_ShouldReturnNull()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var freelancerId = Guid.NewGuid();
            var client = new User { Id = Guid.NewGuid(), Type = UserType.Client };
            // Project is already assigned
            var project = new Project { Id = projectId, Status = ProjectStatus.Assigned, Client = client };

            _projectRepositoryMock.Setup(r => r.GetProjectDetailsAsync(projectId)).ReturnsAsync(project);

            // Act
            var result = await _projectService.AssignFreelancerToProjectAsync(projectId, freelancerId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region MarkProjectAsCompletedAsync Tests

        [Fact]
        public async Task MarkProjectAsCompletedAsync_WithAssignedProject_ShouldSucceed()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var freelancerId = Guid.NewGuid();
            var client = new User { Id = Guid.NewGuid(), Type = UserType.Client };
            var freelancer = new User { Id = Guid.NewGuid(), Type = UserType.Freelancer };
            var project = new Project { Id = projectId, Status = ProjectStatus.Assigned, AssignedFreelancerId = freelancerId, Client = client };
            var freelancerProfile = new FreelancerProfile { Id = freelancerId, ProjectsCompleted = 5, User=freelancer };

            _projectRepositoryMock.Setup(r => r.GetProjectDetailsAsync(projectId)).ReturnsAsync(project);
            _freelancerRepositoryMock.Setup(r => r.GetByIdAsync(freelancerId, false)).ReturnsAsync(freelancerProfile);

            // Act
            await _projectService.MarkProjectAsCompletedAsync(projectId);

            // Assert
            project.Status.Should().Be(ProjectStatus.Completed);
            project.CompletionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            freelancerProfile.ProjectsCompleted.Should().Be(6); // Verify increment
            _projectRepositoryMock.Verify(r => r.UpdateAsync(project), Times.Once);
            _freelancerRepositoryMock.Verify(r => r.UpdateAsync(freelancerProfile), Times.Once);
        }
        
        #endregion

        #region CancelProjectAsync Tests

        [Fact]
        public async Task CancelProjectAsync_WithOpenProject_ShouldSucceed()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var client = new User { Id = Guid.NewGuid(), Type = UserType.Client };
            var project = new Project { Id = projectId, Status = ProjectStatus.Open, Client = client };
            
            _projectRepositoryMock.Setup(r => r.GetProjectDetailsAsync(projectId)).ReturnsAsync(project);
            
            // Act
            await _projectService.CancelProjectAsync(projectId);

            // Assert
            project.Status.Should().Be(ProjectStatus.Cancelled);
            _projectRepositoryMock.Verify(r => r.UpdateAsync(project), Times.Once);
        }
        
        #endregion

        #region UploadProjectAttachmentAsync Tests

        [Fact]
        public async Task UploadProjectAttachmentAsync_WhenProjectExists_ShouldCallFileService()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            var project = new Project { Id = projectId, Client = new User { Id = Guid.NewGuid(), Type = UserType.Client } };
            var mockFile = new Mock<IFormFile>();
            var fileResponseDto = new FileResponseDto { Id = Guid.NewGuid() };

            _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, false)).ReturnsAsync(project);
            _fileServiceMock.Setup(fs => fs.UploadFileAsync(mockFile.Object, uploaderId, FileCategory.ProjectAttachment, projectId)).ReturnsAsync(fileResponseDto);

            // Act
            var result = await _projectService.UploadProjectAttachmentAsync(projectId, uploaderId, mockFile.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(fileResponseDto);
            _fileServiceMock.Verify(fs => fs.UploadFileAsync(mockFile.Object, uploaderId, FileCategory.ProjectAttachment, projectId), Times.Once);
        }
        
        #endregion
    }
}