using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using FreelanceProjectBoardApi.Services.Implementations;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.DTOs.Proposals;
using FreelanceProjectBoardApi.Services.Interfaces;
using System.Linq.Expressions;

namespace FreelanceProjectBoardApi.Tests.Services
{
    public class ProposalServiceTests
    {
        private readonly Mock<IProposalRepository> _proposalRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly IProposalService _proposalService;

        public ProposalServiceTests()
        {
            _proposalRepositoryMock = new Mock<IProposalRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _mapperMock = new Mock<IMapper>();

            _proposalService = new ProposalService(
                _proposalRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _userRepositoryMock.Object,
                _fileServiceMock.Object,
                _mapperMock.Object
            );
        }

        #region CreateProposalAsync Tests

        [Fact]
        public async Task CreateProposalAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var freelancerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var freelancer = new User { Id = freelancerId, Type = UserType.Freelancer };
            var project = new Project { Id = projectId, Status = ProjectStatus.Open, Client = new User { Type = UserType.Client } };
            var futureDeadline = DateTime.Now.AddDays(5).ToString("dd-MM-yyyy HH:mm");
            var createDto = new CreateProposalDto { ProjectId = projectId, ProposedDeadline = futureDeadline };
            var proposal = new Proposal { Id = Guid.NewGuid(), FreelancerId = freelancerId, ProjectId = projectId, Project = project, Freelancer = freelancer };
            var responseDto = new ProposalResponseDto { Id = proposal.Id };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(freelancerId, false)).ReturnsAsync(freelancer);
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, false)).ReturnsAsync(project);
            _proposalRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Proposal, bool>>>(), null, "", false))
                .ReturnsAsync(new List<Proposal>());
            _mapperMock.Setup(m => m.Map<Proposal>(createDto)).Returns(proposal);
            _proposalRepositoryMock.Setup(r => r.GetProposalDetailsAsync(proposal.Id)).ReturnsAsync(proposal);
            _mapperMock.Setup(m => m.Map<ProposalResponseDto>(proposal)).Returns(responseDto);

            // Act
            var result = await _proposalService.CreateProposalAsync(freelancerId, createDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseDto);
            _proposalRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Proposal>()), Times.Once);
            _proposalRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateProposalAsync_WhenProjectIsNotOpen_ShouldReturnNull()
        {
            // Arrange
            var freelancerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var Client = new User { Id = Guid.NewGuid() };
            var freelancer = new User { Id = freelancerId, Type = UserType.Freelancer };
            var project = new Project { Id = projectId, Status = ProjectStatus.Assigned, Client = Client }; // Not Open
            var createDto = new CreateProposalDto { ProjectId = projectId, ProposedDeadline = DateTime.Now.AddDays(1).ToString("dd-MM-yyyy HH:mm") };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(freelancerId, false)).ReturnsAsync(freelancer);
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, false)).ReturnsAsync(project);

            // Act
            var result = await _proposalService.CreateProposalAsync(freelancerId, createDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateProposalAsync_WhenProposalAlreadyExists_ShouldReturnNull()
        {
            // Arrange
            var freelancerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var freelancer = new User { Id = freelancerId, Type = UserType.Freelancer };
            var project = new Project { Id = projectId, Status = ProjectStatus.Open, Client = new User { Type = UserType.Client } };
            var createDto = new CreateProposalDto { ProjectId = projectId, ProposedDeadline = DateTime.Now.AddDays(1).ToString("dd-MM-yyyy HH:mm") };
            var existingProposals = new List<Proposal> { new Proposal { FreelancerId = freelancerId, ProjectId = projectId, Project=project,Freelancer = freelancer } };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(freelancerId, false)).ReturnsAsync(freelancer);
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId, false)).ReturnsAsync(project);
            _proposalRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Proposal, bool>>>(), null, "", false))
                .ReturnsAsync(existingProposals);

            // Act
            var result = await _proposalService.CreateProposalAsync(freelancerId, createDto);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetProposalByIdAsync Tests

        [Fact]
        public async Task GetProposalByIdAsync_WhenProposalExists_ShouldReturnDto()
        {
            // Arrange
            var proposalId = Guid.NewGuid();
            var freelancer = new User { Id = Guid.NewGuid(), Type = UserType.Freelancer };
            var proposal = new Proposal { Id = proposalId, Project = new Project { Client = new User { Id = Guid.NewGuid(), Type = UserType.Client }}, Freelancer = freelancer }; // Basic nesting to avoid nulls
            var responseDto = new ProposalResponseDto { Id = proposalId };

            _proposalRepositoryMock.Setup(r => r.GetProposalDetailsAsync(proposalId)).ReturnsAsync(proposal);
            _mapperMock.Setup(m => m.Map<ProposalResponseDto>(proposal)).Returns(responseDto);

            // Act
            var result = await _proposalService.GetProposalByIdAsync(proposalId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseDto);
        }

        #endregion

        #region UpdateProposalStatusAsync Tests

        [Fact]
        public async Task UpdateProposalStatusAsync_FromPendingToAccepted_ShouldSucceed()
        {
            // Arrange
            var proposalId = Guid.NewGuid();
            var freelancer = new User { Id = Guid.NewGuid(), Type = UserType.Freelancer };
            var proposal = new Proposal { Id = proposalId, Status = ProposalStatus.Pending, Project = new Project { Client = new User() }, Freelancer = freelancer };
            var updateDto = new UpdateProposalStatusDto { NewStatus = "Accepted" };

            _proposalRepositoryMock.Setup(r => r.GetByIdAsync(proposalId, false)).ReturnsAsync(proposal);
            _mapperMock.Setup(m => m.Map<ProposalResponseDto>(It.IsAny<Proposal>()))
                .Returns((Proposal p) => new ProposalResponseDto { Status = p.Status.ToString() });

            // Act
            var result = await _proposalService.UpdateProposalStatusAsync(proposalId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Accepted");
            proposal.Status.Should().Be(ProposalStatus.Accepted);
            _proposalRepositoryMock.Verify(r => r.UpdateAsync(proposal), Times.Once);
            _proposalRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProposalStatusAsync_OnFinalizedProposal_ShouldReturnNull()
        {
            // Arrange
            var proposalId = Guid.NewGuid();
            var freelancer = new User { Id = Guid.NewGuid(), Type = UserType.Freelancer };
            var proposal = new Proposal { Id = proposalId, Status = ProposalStatus.Accepted, Freelancer = freelancer, Project = new Project { Client = new User() } }; // Already accepted
            var updateDto = new UpdateProposalStatusDto { NewStatus = "Withdrawn" };

            _proposalRepositoryMock.Setup(r => r.GetByIdAsync(proposalId, false)).ReturnsAsync(proposal);

            // Act
            var result = await _proposalService.UpdateProposalStatusAsync(proposalId, updateDto);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteProposalAsync Tests

        [Fact]
        public async Task DeleteProposalAsync_WhenProposalIsPending_ShouldReturnTrue()
        {
            // Arrange
            var proposalId = Guid.NewGuid();
            var freelancer = new User { Id = Guid.NewGuid(), Type = UserType.Freelancer };
            var proposal = new Proposal { Id = proposalId, Status = ProposalStatus.Pending, Freelancer=freelancer, Project = new Project { Client = new User() } };

            _proposalRepositoryMock.Setup(r => r.GetByIdAsync(proposalId, false)).ReturnsAsync(proposal);

            // Act
            var result = await _proposalService.DeleteProposalAsync(proposalId);

            // Assert
            result.Should().BeTrue();
            _proposalRepositoryMock.Verify(r => r.DeleteAsync(proposalId), Times.Once);
        }

        [Fact]
        public async Task DeleteProposalAsync_WhenProposalIsNotPending_ShouldReturnFalse()
        {
            // Arrange
            var proposalId = Guid.NewGuid();
            var freelancer = new User { Id = Guid.NewGuid(), Type = UserType.Freelancer };
            var proposal = new Proposal { Id = proposalId, Status = ProposalStatus.Accepted, Project = new Project { Client = new User() }, Freelancer = freelancer }; // Not pending

            _proposalRepositoryMock.Setup(r => r.GetByIdAsync(proposalId, false)).ReturnsAsync(proposal);



            // Act
            var result = await _proposalService.DeleteProposalAsync(proposalId);

            // Assert
            result.Should().BeFalse();
            _proposalRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }
        
        #endregion
    }
}