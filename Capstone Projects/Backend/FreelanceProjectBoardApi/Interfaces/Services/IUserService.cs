using FreelanceProjectBoardApi.DTOs.Users;
using FreelanceProjectBoardApi.Helpers;

namespace FreelanceProjectBoardApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto?> GetUserByIdAsync(Guid id);
        Task<UserResponseDto?> GetUserByEmailAsync(string email);
        Task<UserResponseDto?> UpdateUserAsync(Guid id, UpdateUserDto updateDto);
        Task<bool> DeleteUserAsync(Guid id); // Soft delete
        Task<PageResult<UserResponseDto>> GetAllUsersAsync(UserFilter filter, PaginationParams pagination);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    }
}
