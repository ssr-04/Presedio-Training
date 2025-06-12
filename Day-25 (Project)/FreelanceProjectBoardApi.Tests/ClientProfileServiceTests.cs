using Moq;
using FluentAssertions;
using AutoMapper;
using FreelanceProjectBoardApi.Services.Implementations;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.DTOs.ClientProfiles;
using FreelanceProjectBoardApi.Services.Interfaces;
using System.Linq.Expressions;
using FreelanceProjectBoardApi.Helpers;

namespace FreelanceProjectBoardApi.Tests.Services
{
    public class ClientProfileServiceTests
    {
        // Mocks for all dependencies
        private readonly Mock<IClientProfileRepository> _clientProfileRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly IClientProfileService _clientProfileService;

        public ClientProfileServiceTests()
        {
            // Instantiate mocks
            _clientProfileRepositoryMock = new Mock<IClientProfileRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();

            _clientProfileService = new ClientProfileService(
                _clientProfileRepositoryMock.Object,
                _userRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        #region CreateClientProfileAsync Tests

        [Fact]
        public async Task CreateClientProfileAsync_WithValidClientUserAndNoExistingProfile_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Client };
            var createDto = new CreateClientProfileDto { CompanyName = "Test Corp" };
            var clientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = userId, CompanyName = "Test Corp", User = user };
            var responseDto = new ClientProfileResponseDto { Id = clientProfile.Id, CompanyName = "Test Corp" };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);
            
            _clientProfileRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<ClientProfile, bool>>>(), null, "", false))
                .ReturnsAsync(new List<ClientProfile>());

            _mapperMock.Setup(m => m.Map<ClientProfile>(createDto)).Returns(clientProfile);
            _mapperMock.Setup(m => m.Map<ClientProfileResponseDto>(clientProfile)).Returns(responseDto);

            // Act
            var result = await _clientProfileService.CreateClientProfileAsync(userId, createDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseDto);

            _clientProfileRepositoryMock.Verify(r => r.AddAsync(clientProfile), Times.Once);
            _clientProfileRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateClientProfileAsync_WhenUserNotFound_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createDto = new CreateClientProfileDto();

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync((User?)null);

            // Act
            var result = await _clientProfileService.CreateClientProfileAsync(userId, createDto);

            // Assert
            result.Should().BeNull();
            _clientProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ClientProfile>()), Times.Never);
        }

        [Fact]
        public async Task CreateClientProfileAsync_WhenUserIsNotAClient_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            // User is a Freelancer
            var user = new User { Id = userId, Type = UserType.Freelancer };
            var createDto = new CreateClientProfileDto();

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);

            // Act
            var result = await _clientProfileService.CreateClientProfileAsync(userId, createDto);

            // Assert
            result.Should().BeNull();
            _clientProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ClientProfile>()), Times.Never);
        }

        [Fact]
        public async Task CreateClientProfileAsync_WhenProfileAlreadyExists_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Client };
            var createDto = new CreateClientProfileDto();
            var existingProfile = new List<ClientProfile> { new ClientProfile { UserId = userId, User = user } };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);
            // Simulates that a profile already exists
            _clientProfileRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<ClientProfile, bool>>>(), null, "", false))
                .ReturnsAsync(existingProfile);

            // Act
            var result = await _clientProfileService.CreateClientProfileAsync(userId, createDto);

            // Assert
            result.Should().BeNull();
            _clientProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ClientProfile>()), Times.Never);
        }

        #endregion

        #region GetClientProfileByIdAsync Tests

        [Fact]
        public async Task GetClientProfileByIdAsync_WhenProfileExists_ShouldReturnDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Client };
            var profileId = Guid.NewGuid();
            var profile = new ClientProfile { Id = profileId, CompanyName = "GetTest Corp", User = user };
            var responseDto = new ClientProfileResponseDto { Id = profileId, CompanyName = "GetTest Corp" };

            _clientProfileRepositoryMock.Setup(r => r.GetClientProfileDetailsAsync(profileId)).ReturnsAsync(profile);
            _mapperMock.Setup(m => m.Map<ClientProfileResponseDto>(profile)).Returns(responseDto);

            // Act
            var result = await _clientProfileService.GetClientProfileByIdAsync(profileId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseDto);
        }

        [Fact]
        public async Task GetClientProfileByIdAsync_WhenProfileDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            _clientProfileRepositoryMock.Setup(r => r.GetClientProfileDetailsAsync(profileId)).ReturnsAsync((ClientProfile?)null);

            // Act
            var result = await _clientProfileService.GetClientProfileByIdAsync(profileId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateClientProfileAsync Tests

        [Fact]
        public async Task UpdateClientProfileAsync_WhenProfileExists_ShouldUpdateAndReturnDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Client };
            var profileId = Guid.NewGuid();
            var profileToUpdate = new ClientProfile { Id = profileId, CompanyName = "Original Name", User=user };
            var updateDto = new UpdateClientProfileDto { CompanyName = "Updated Name" };
            var updatedDto = new ClientProfileResponseDto { Id = profileId, CompanyName = "Updated Name" };

            _clientProfileRepositoryMock.Setup(r => r.GetByIdAsync(profileId, false)).ReturnsAsync(profileToUpdate);
            _mapperMock.Setup(m => m.Map<ClientProfileResponseDto>(profileToUpdate)).Returns(updatedDto);

            // Act
            var result = await _clientProfileService.UpdateClientProfileAsync(profileId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedDto);

            // Verify mapper applies the changes from DTO to the entity
            _mapperMock.Verify(m => m.Map(updateDto, profileToUpdate), Times.Once);
            _clientProfileRepositoryMock.Verify(r => r.UpdateAsync(profileToUpdate), Times.Once);
            _clientProfileRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateClientProfileAsync_WhenProfileDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            var updateDto = new UpdateClientProfileDto();
            _clientProfileRepositoryMock.Setup(r => r.GetByIdAsync(profileId, false)).ReturnsAsync((ClientProfile?)null);

            // Act
            var result = await _clientProfileService.UpdateClientProfileAsync(profileId, updateDto);

            // Assert
            result.Should().BeNull();
            _clientProfileRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ClientProfile>()), Times.Never);
        }

        #endregion

        #region DeleteClientProfileAsync Tests

        [Fact]
        public async Task DeleteClientProfileAsync_WhenProfileExists_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Client };
            var profileId = Guid.NewGuid();
            var profile = new ClientProfile { Id = profileId, User=user};
            _clientProfileRepositoryMock.Setup(r => r.GetByIdAsync(profileId, false)).ReturnsAsync(profile);

            // Act
            var result = await _clientProfileService.DeleteClientProfileAsync(profileId);

            // Assert
            result.Should().BeTrue();
            _clientProfileRepositoryMock.Verify(r => r.DeleteAsync(profileId), Times.Once);
            _clientProfileRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteClientProfileAsync_WhenProfileDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            _clientProfileRepositoryMock.Setup(r => r.GetByIdAsync(profileId, false)).ReturnsAsync((ClientProfile?)null);

            // Act
            var result = await _clientProfileService.DeleteClientProfileAsync(profileId);

            // Assert
            result.Should().BeFalse();
            _clientProfileRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region GetAllClientProfilesAsync Tests

        [Fact]
        public async Task GetAllClientProfilesAsync_ShouldReturnPagedResultOfDtos()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var user1 = new User { Id = userId1, Type = UserType.Client };
            var userId2 = Guid.NewGuid();
            var user2 = new User { Id = userId2, Type = UserType.Client };
            var filter = new ClientFilter();
            var pagination = new PaginationParams();
            var profiles = new List<ClientProfile> { new ClientProfile { Id = Guid.NewGuid(), User=user1 }, new ClientProfile { Id = Guid.NewGuid(), User=user2 } };
            var profileDtos = new List<ClientProfileResponseDto> { new ClientProfileResponseDto { Id = profiles[0].Id }, new ClientProfileResponseDto { Id = profiles[1].Id } };
            
            var paginationInfo = new PaginationInfo { TotalRecords = 2, PageNumber = 1, PageSize = 10, TotalPages = 1 };
            var pagedResultFromRepo = new PageResult<ClientProfile> { Data = profiles, pagination = paginationInfo };

            _clientProfileRepositoryMock.Setup(r => r.GetAllClientProfileAsync(filter, pagination)).ReturnsAsync(pagedResultFromRepo);
            _mapperMock.Setup(m => m.Map<IEnumerable<ClientProfileResponseDto>>(profiles)).Returns(profileDtos);

            // Act
            var result = await _clientProfileService.GetAllClientProfilesAsync(filter, pagination);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEquivalentTo(profileDtos);
            result.pagination.Should().BeEquivalentTo(paginationInfo);
        }

        #endregion
    }
}