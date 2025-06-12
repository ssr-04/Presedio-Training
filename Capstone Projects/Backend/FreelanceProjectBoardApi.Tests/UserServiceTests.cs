
using AutoMapper;
using FluentAssertions;
using FreelanceProjectBoardApi.DTOs.Users;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Implementations;
using FreelanceProjectBoardApi.Services.Interfaces;
using Moq;

namespace FreelanceProjectBoardApi.Tests.Services
{
    public class UserServiceTests
    {
        // Mocking for all dependencies of UserService
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly IUserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();

            _userService = new UserService(_userRepositoryMock.Object, _mapperMock.Object);
        }

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUserResponseDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test@example.com" };
            var userResponseDto = new UserResponseDto { Id = userId, Email = "test@example.com" };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, false)).ReturnsAsync(user);
            _mapperMock.Setup(mapper => mapper.Map<UserResponseDto>(user)).Returns(userResponseDto);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(userResponseDto);
            _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId, false), Times.Once);
            _mapperMock.Verify(mapper => mapper.Map<UserResponseDto>(user), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, false)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_WhenUserExists_ShouldUpdateAndReturnDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userToUpdate = new User { Id = userId, Email = "original@example.com" };
            var updateDto = new UpdateUserDto { Email = "updated@example.com" }; 
            var updatedUserDto = new UserResponseDto { Id = userId, Email = "updated@example.com" };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, false)).ReturnsAsync(userToUpdate);
            _mapperMock.Setup(m => m.Map<UserResponseDto>(userToUpdate)).Returns(updatedUserDto);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedUserDto);

            // Verifies that the mapper was used to apply changes from DTO to entity
            _mapperMock.Verify(m => m.Map(updateDto, userToUpdate), Times.Once);
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(userToUpdate), Times.Once);
            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateDto = new UpdateUserDto();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, false)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateDto);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_WhenUserExists_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, false)).ReturnsAsync(user);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(repo => repo.DeleteAsync(userId), Times.Once);
            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, false)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_WhenUsersExist_ShouldReturnPagedResultOfDtos()
        {
            // Arrange
            var filter = new UserFilter();
            var pagination = new PaginationParams();
            var users = new List<User> { new User { Id = Guid.NewGuid() }, new User { Id = Guid.NewGuid() } };
            var userDtos = new List<UserResponseDto> { new UserResponseDto { Id = users[0].Id }, new UserResponseDto { Id = users[1].Id } };
            
            var paginationInfo = new PaginationInfo { TotalRecords = 2, PageNumber = 1, PageSize = 10, TotalPages = 1 };
            var pagedResultFromRepo = new PageResult<User> { Data = users, pagination = paginationInfo };

            _userRepositoryMock.Setup(r => r.GetAllUsersAsync(filter, pagination, true)).ReturnsAsync(pagedResultFromRepo);
            _mapperMock.Setup(m => m.Map<IEnumerable<UserResponseDto>>(users)).Returns(userDtos);

            // Act
            var result = await _userService.GetAllUsersAsync(filter, pagination);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEquivalentTo(userDtos);
            result.pagination.Should().BeEquivalentTo(paginationInfo);
        }

        #endregion

        #region ChangePasswordAsync Tests

        [Fact]
        public async Task ChangePasswordAsync_WithValidUserAndCorrectCurrentPassword_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var currentPassword = "password123";
            var newPassword = "newPassword456";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(currentPassword);
            var user = new User { Id = userId, PasswordHash = hashedPassword };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);

            // Act
            var result = await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);

            // Assert
            result.Should().BeTrue();
            // Verifies that the user passed to UpdateAsync has a new password hash
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Id == userId && u.PasswordHash != hashedPassword)), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithInvalidUserId_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.ChangePasswordAsync(userId, "any", "any");

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var correctPassword = "password123";
            var incorrectAttempt = "wrongPassword";
            var newPassword = "newPassword456";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);
            var user = new User { Id = userId, PasswordHash = hashedPassword };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);

            // Act
            var result = await _userService.ChangePasswordAsync(userId, incorrectAttempt, newPassword);

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
        #endregion
    }
}